// Copyright (c) 2026 MabinogiCode. All rights reserved.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FastWinFormsCharts3D.DataModels;

namespace FastWinFormsCharts3D.Benchmarks;

/// <summary>
/// Benchmarks for <see cref="FastWinFormsCharts3D.Charts.Scatter.ScatterChart3D"/> data path.
/// GPU rendering benchmarks are added in v0.3 (require an off-screen GL context).
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class ScatterRenderBenchmark
{
    private DataPoint3D[] _points = [];

    /// <summary>Gets or sets the number of data points used in the benchmark.</summary>
    [Params(1_000, 100_000, 1_000_000)]
    public int PointCount { get; set; }

    /// <summary>Sets up test data before each benchmark run.</summary>
    [GlobalSetup]
    public void Setup()
    {
        Random rng = new(42);
        _points = new DataPoint3D[PointCount];

        for (int i = 0; i < PointCount; i++)
        {
            _points[i] = new DataPoint3D(
                (float)(rng.NextDouble() * 2 - 1),
                (float)(rng.NextDouble() * 2 - 1),
                (float)(rng.NextDouble() * 2 - 1));
        }
    }

    /// <summary>Benchmarks bulk DataSeries3D construction and SetPoints.</summary>
    [Benchmark(Description = "DataSeries3D.SetPoints")]
    public void BulkSetPoints()
    {
        DataSeries3D series = new("bench", []);
        series.SetPoints(_points);
    }
}
