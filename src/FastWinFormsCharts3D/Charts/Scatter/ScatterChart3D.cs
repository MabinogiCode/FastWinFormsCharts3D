// Copyright (c) 2026 MabinogiCode. All rights reserved.

using FastWinFormsCharts3D.Camera;
using FastWinFormsCharts3D.Charts.Abstractions;
using FastWinFormsCharts3D.DataModels;
using Silk.NET.OpenGL;

namespace FastWinFormsCharts3D.Charts.Scatter;

/// <summary>
/// A 3D scatter chart that renders one or more <see cref="DataSeries3D"/> as point clouds.
/// </summary>
/// <remarks>
/// <para>Implementation details (filled in v0.3):</para>
/// <list type="bullet">
///   <item>Each series maps to one VBO + VAO pair.</item>
///   <item>GLSL shaders are loaded from embedded resources (<c>scatter.vert</c>, <c>scatter.frag</c>).</item>
///   <item>Subscribes to <see cref="DataSeries3D.DataChanged"/> for incremental GPU updates.</item>
/// </list>
/// </remarks>
public sealed class ScatterChart3D : IChart3D
{
    private readonly List<DataSeries3D> _series = [];
    private bool _disposed;

    /// <inheritdoc />
    public string Title { get; set; } = "Scatter 3D";

    /// <inheritdoc />
    public bool IsInitialized { get; private set; }

    /// <summary>Gets the series currently registered with this chart.</summary>
    public IReadOnlyList<DataSeries3D> Series => _series;

    /// <summary>Adds a series to the chart.</summary>
    /// <param name="series">The series to add.</param>
    public void AddSeries(DataSeries3D series)
    {
        _series.Add(series);
    }

    /// <summary>Removes a series by name.</summary>
    /// <param name="seriesName">The name of the series to remove.</param>
    public void RemoveSeries(string seriesName)
    {
        DataSeries3D? target = _series.Find(s => s.Name == seriesName);
        if (target is not null)
        {
            _series.Remove(target);
        }
    }

    /// <inheritdoc />
    public void Initialize(GL gl)
    {
        // TODO (v0.3): compile scatter.vert / scatter.frag, create VAOs/VBOs per series.
        IsInitialized = true;
    }

    /// <inheritdoc />
    public void Render(GL gl, Camera3D camera)
    {
        // TODO (v0.3): bind shader, upload MVP uniforms, draw each visible series.
    }

    /// <inheritdoc />
    public void Resize(int width, int height)
    {
        // TODO (v0.3): update projection matrix aspect ratio.
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // TODO (v0.3): release GPU resources (VAOs, VBOs, shader program).
        _disposed = true;
    }
}
