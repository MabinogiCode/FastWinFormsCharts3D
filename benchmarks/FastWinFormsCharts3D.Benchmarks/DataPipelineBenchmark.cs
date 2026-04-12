// Copyright (c) 2026 MabinogiCode. All rights reserved.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FastWinFormsCharts3D.DataModels;

namespace FastWinFormsCharts3D.Benchmarks;

/// <summary>
/// Benchmarks for <see cref="DataPipeline"/> — measures the overhead of the
/// background-thread compute → UI-thread marshal pattern.
/// </summary>
/// <remarks>
/// BenchmarkDotNet has no <see cref="SynchronizationContext"/> so
/// <see cref="DataPipeline.PostAsync(DataSeries3D, Func{DataPoint3D[]})"/> falls
/// back to an inline <see cref="DataSeries3D.SetPoints"/> call after the
/// <see cref="Task.Run"/> completes.  The benchmark therefore measures:
/// <list type="bullet">
///   <item>Thread-pool scheduling + return overhead (<c>Task.Run</c>).</item>
///   <item>Cloning cost of the result array before handing it off.</item>
/// </list>
/// </remarks>
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class DataPipelineBenchmark
{
    private DataSeries3D _series = null!;
    private DataPoint3D[] _sourcePoints = [];

    /// <summary>Gets or sets the number of data points generated on the background thread.</summary>
    [Params(10_000, 1_000_000)]
    public int PointCount { get; set; }

    /// <summary>Allocates source data shared across all iterations.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _series = new DataSeries3D("pipeline-bench", []);

        Random rng = new(42);
        _sourcePoints = new DataPoint3D[PointCount];

        for (int i = 0; i < PointCount; i++)
        {
            _sourcePoints[i] = new DataPoint3D(
                (float)(rng.NextDouble() * 2 - 1),
                (float)(rng.NextDouble() * 2 - 1),
                (float)(rng.NextDouble() * 2 - 1));
        }
    }

    /// <summary>
    /// Measures round-trip latency of <see cref="DataPipeline.PostAsync(DataSeries3D, Func{DataPoint3D[]})"/>:
    /// array clone on the thread pool, then <see cref="DataSeries3D.SetPoints"/> inline
    /// (no <see cref="SynchronizationContext"/> in benchmark host).
    /// </summary>
    [Benchmark(Description = "DataPipeline.PostAsync (clone + SetPoints)")]
    public async Task PostAsync()
    {
        await DataPipeline.PostAsync(_series, () => (DataPoint3D[])_sourcePoints.Clone());
    }
}
