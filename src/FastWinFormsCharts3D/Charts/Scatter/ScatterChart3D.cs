// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Drawing;
using System.Numerics;
using FastWinFormsCharts3D.Camera;
using FastWinFormsCharts3D.Charts.Abstractions;
using FastWinFormsCharts3D.DataModels;
using FastWinFormsCharts3D.Rendering;
using Silk.NET.OpenGL;

namespace FastWinFormsCharts3D.Charts.Scatter;

/// <summary>
/// A 3D scatter chart that renders one or more <see cref="DataSeries3D"/> as interactive point clouds.
/// </summary>
/// <remarks>
/// <para>Rendering pipeline:</para>
/// <list type="number">
///   <item>Shaders compiled from embedded resources on <see cref="Initialize"/>.</item>
///   <item>Each series maps to one VAO + VBO pair (tightly packed XYZ floats).</item>
///   <item>MVP uploaded per frame; model matrix is Identity (data in world space).</item>
///   <item>X/Y/Z axis lines rendered last with fixed colors.</item>
/// </list>
/// <para>
/// <see cref="DataSeries3D.DataChanged"/> triggers an incremental GPU re-upload on the UI thread.
/// </para>
/// </remarks>
public sealed class ScatterChart3D : IChart3D
{
    // ── Embedded resource names ───────────────────────────────────────────────
    private const string VertResourceName = "FastWinFormsCharts3D.Rendering.Shaders.scatter.vert";
    private const string FragResourceName = "FastWinFormsCharts3D.Rendering.Shaders.scatter.frag";

    // ── Axis geometry: 3 lines × 2 vertices × 3 floats ───────────────────────
    private static readonly float[] AxisVertices =
    [
        -1f, 0f, 0f,   1f, 0f, 0f,   // X axis
         0f,-1f, 0f,   0f, 1f, 0f,   // Y axis
         0f, 0f,-1f,   0f, 0f, 1f,   // Z axis
    ];

    private static readonly (Vector4 Color, int FirstVertex, int VertexCount)[] AxisSegments =
    [
        (new Vector4(0.9f, 0.25f, 0.25f, 1f), 0, 2),   // X — red
        (new Vector4(0.25f, 0.9f, 0.25f, 1f), 2, 2),   // Y — green
        (new Vector4(0.25f, 0.5f,  1.0f, 1f), 4, 2),   // Z — blue
    ];

    // ── GPU state ─────────────────────────────────────────────────────────────
    private GL? _gl;
    private ShaderProgram? _shaderProgram;
    private readonly Dictionary<string, GpuSeriesData> _gpuBuffers = [];
    private VertexArrayObject? _axisVao;
    private VertexBuffer? _axisVbo;

    // ── Domain state ──────────────────────────────────────────────────────────
    private readonly List<DataSeries3D> _series = [];
    private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;
    private int _viewportWidth = 800;
    private int _viewportHeight = 600;
    private bool _disposed;

    // ── IChart3D ──────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public string Title { get; set; } = "Scatter 3D";

    /// <inheritdoc />
    public bool IsInitialized { get; private set; }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Gets the series currently registered with this chart.</summary>
    public IReadOnlyList<DataSeries3D> Series => _series;

    /// <summary>
    /// Adds a series. If the chart is already initialized, uploads the series to the GPU immediately.
    /// </summary>
    /// <param name="series">The series to add.</param>
    public void AddSeries(DataSeries3D series)
    {
        _series.Add(series);
        series.DataChanged += OnSeriesDataChanged;

        if (IsInitialized && _gl is not null)
        {
            UploadSeriesToGpu(_gl, series);
        }
    }

    /// <summary>Removes a series by name and releases its GPU resources.</summary>
    /// <param name="seriesName">The name of the series to remove.</param>
    public void RemoveSeries(string seriesName)
    {
        DataSeries3D? target = _series.Find(static s => s.Name == seriesName);
        if (target is null)
        {
            return;
        }

        target.DataChanged -= OnSeriesDataChanged;
        _series.Remove(target);
        FreeGpuSeries(seriesName);
    }

    // ── IChart3D implementation ───────────────────────────────────────────────

    /// <inheritdoc />
    public void Initialize(GL gl)
    {
        _gl = gl;
        _shaderProgram = ShaderProgram.FromEmbeddedResource(gl, VertResourceName, FragResourceName);

        foreach (DataSeries3D series in _series)
        {
            UploadSeriesToGpu(gl, series);
        }

        InitializeAxes(gl);
        UpdateProjection();

        IsInitialized = true;
    }

    /// <inheritdoc />
    public void Render(GL gl, Camera3D camera)
    {
        if (_shaderProgram is null)
        {
            return;
        }

        // Model matrix = Identity (data already in world space).
        // System.Numerics order: Model × View × Projection.
        // ShaderProgram.SetUniform uploads with transpose=true to convert row-major → column-major.
        Matrix4x4 mvp = camera.ViewMatrix * _projectionMatrix;

        _shaderProgram.Use();
        _shaderProgram.SetUniform("uMVP", mvp);

        // ── Point cloud series ─────────────────────────────────────────────
        foreach (DataSeries3D series in _series)
        {
            if (!series.IsVisible)
            {
                continue;
            }

            if (!_gpuBuffers.TryGetValue(series.Name, out GpuSeriesData? gpuData))
            {
                continue;
            }

            Color c = series.Color;
            _shaderProgram.SetUniform(
                "uColor",
                new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f));
            _shaderProgram.SetUniform("uPointSize", series.MarkerSize);

            gpuData.Vao.Bind();
            gl.DrawArrays(PrimitiveType.Points, 0, (uint)gpuData.Count);
            gpuData.Vao.Unbind();
        }

        // ── X / Y / Z axis lines ───────────────────────────────────────────
        RenderAxes(gl);
    }

    /// <inheritdoc />
    public void Resize(int width, int height)
    {
        _viewportWidth = width;
        _viewportHeight = height;
        UpdateProjection();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (DataSeries3D series in _series)
            {
                series.DataChanged -= OnSeriesDataChanged;
            }

            _shaderProgram?.Dispose();

            foreach (GpuSeriesData data in _gpuBuffers.Values)
            {
                data.Vao.Dispose();
                data.Vbo.Dispose();
            }

            _gpuBuffers.Clear();
            _axisVao?.Dispose();
            _axisVbo?.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void UpdateProjection()
    {
        float aspect = (_viewportWidth > 0 && _viewportHeight > 0)
            ? (float)_viewportWidth / _viewportHeight
            : 16f / 9f;

        _projectionMatrix = Projection.Perspective(45f, aspect, 0.1f, 1000f);
    }

    private void UploadSeriesToGpu(GL gl, DataSeries3D series)
    {
        FreeGpuSeries(series.Name);

        DataPoint3D[] points = [.. series.Points];
        if (points.Length == 0)
        {
            return;
        }

        // DataPoint3D is an unmanaged struct: float X, Y, Z — 12 bytes per vertex.
        VertexBuffer vbo = VertexBuffer.Create(gl, points);
        VertexArrayObject vao = new(gl);

        vbo.Bind();
        // location=0, size=3 (xyz), float, not-normalized, stride=12, offset=0
        vao.AddVertexAttributePointer(0, 3, VertexAttribPointerType.Float, false, (uint)(3 * sizeof(float)), 0);
        vao.Unbind();
        vbo.Unbind();

        _gpuBuffers[series.Name] = new GpuSeriesData(vao, vbo, points.Length);
    }

    private void FreeGpuSeries(string seriesName)
    {
        if (_gpuBuffers.TryGetValue(seriesName, out GpuSeriesData? existing))
        {
            existing.Vao.Dispose();
            existing.Vbo.Dispose();
            _gpuBuffers.Remove(seriesName);
        }
    }

    private void OnSeriesDataChanged(object? sender, EventArgs e)
    {
        if (_gl is null || sender is not DataSeries3D series)
        {
            return;
        }

        UploadSeriesToGpu(_gl, series);
    }

    private void InitializeAxes(GL gl)
    {
        _axisVbo = VertexBuffer.Create(gl, AxisVertices);
        _axisVao = new VertexArrayObject(gl);
        _axisVbo.Bind();
        _axisVao.AddVertexAttributePointer(0, 3, VertexAttribPointerType.Float, false, (uint)(3 * sizeof(float)), 0);
        _axisVao.Unbind();
        _axisVbo.Unbind();
    }

    private void RenderAxes(GL gl)
    {
        if (_shaderProgram is null || _axisVao is null)
        {
            return;
        }

        _shaderProgram.SetUniform("uPointSize", 1f);

        foreach ((Vector4 color, int firstVertex, int count) in AxisSegments)
        {
            _shaderProgram.SetUniform("uColor", color);
            _axisVao.Bind();
            gl.DrawArrays(PrimitiveType.Lines, firstVertex, (uint)count);
            _axisVao.Unbind();
        }
    }

    // ── Nested types ──────────────────────────────────────────────────────────

    private sealed record GpuSeriesData(VertexArrayObject Vao, VertexBuffer Vbo, int Count);
}
