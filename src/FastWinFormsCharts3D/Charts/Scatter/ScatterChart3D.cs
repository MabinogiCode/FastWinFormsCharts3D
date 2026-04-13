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
///   <item>Frustum culling: each series AABB is tested via <see cref="FrustumCuller.IsVisible"/>
///     each frame; invisible series skip the draw call entirely.</item>
///   <item>LOD: when <see cref="MaxRenderPoints"/> is set, data is stride-sampled down to that
///     count at VBO-upload time, reducing GPU vertex throughput for large datasets.</item>
///   <item>X/Y/Z axis lines rendered via <see cref="AxisRenderer"/> (flat-colour shader).</item>
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

    // ── GPU state ─────────────────────────────────────────────────────────────
    private GL? _gl;
    private ShaderProgram? _shaderProgram;
    private readonly Dictionary<string, GpuSeriesData> _gpuBuffers = [];
    private AxisRenderer? _axisRenderer;

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
    /// Gets or sets the maximum number of points uploaded to the GPU per series.
    /// When a series exceeds this value, its data is stride-sampled to
    /// <see cref="MaxRenderPoints"/> entries before VBO upload (LOD).
    /// Set to <c>0</c> to disable the cap (default — all points are uploaded).
    /// </summary>
    /// <remarks>
    /// The cap is applied at VBO-upload time (i.e., on <see cref="DataSeries3D.DataChanged"/>
    /// or at <see cref="Initialize"/>). Changing this property takes effect on the next
    /// data-changed event or chart re-initialization.
    /// </remarks>
    public int MaxRenderPoints { get; set; }

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

        _axisRenderer = new AxisRenderer();
        _axisRenderer.Initialize(gl);

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

            // Frustum cull: skip the draw call when the series AABB is entirely
            // outside the view frustum. The test is conservative (never culls visible data).
            if (!FrustumCuller.IsVisible(gpuData.Bounds, mvp))
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
        _axisRenderer?.Render(gl, mvp);
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
            _axisRenderer?.Dispose();
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

        DataPoint3D[] allPoints = [.. series.Points];
        if (allPoints.Length == 0)
        {
            return;
        }

        // LOD: when MaxRenderPoints is set and the series exceeds it, stride-sample
        // the data down to MaxRenderPoints entries before GPU upload.
        DataPoint3D[] points;
        if (MaxRenderPoints > 0 && allPoints.Length > MaxRenderPoints)
        {
            int stride = allPoints.Length / MaxRenderPoints;
            points = new DataPoint3D[MaxRenderPoints];

            for (int i = 0; i < MaxRenderPoints; i++)
            {
                points[i] = allPoints[i * stride];
            }
        }
        else
        {
            points = allPoints;
        }

        BoundingBox3D bounds = ComputeBounds(points);

        // DataPoint3D is an unmanaged struct: float X, Y, Z — 12 bytes per vertex.
        VertexBuffer vbo = VertexBuffer.Create(gl, points);
        VertexArrayObject vao = new(gl);

        vbo.Bind();
        // location=0, size=3 (xyz), float, not-normalized, stride=12, offset=0
        vao.AddVertexAttributePointer(0, 3, VertexAttribPointerType.Float, false, (uint)(3 * sizeof(float)), 0);
        vao.Unbind();
        vbo.Unbind();

        _gpuBuffers[series.Name] = new GpuSeriesData(vao, vbo, points.Length, bounds);
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

    private static BoundingBox3D ComputeBounds(ReadOnlySpan<DataPoint3D> points)
    {
        float minX = points[0].X, minY = points[0].Y, minZ = points[0].Z;
        float maxX = minX, maxY = minY, maxZ = minZ;

        for (int i = 1; i < points.Length; i++)
        {
            DataPoint3D p = points[i];

            if (p.X < minX)
            {
                minX = p.X;
            }
            else if (p.X > maxX)
            {
                maxX = p.X;
            }

            if (p.Y < minY)
            {
                minY = p.Y;
            }
            else if (p.Y > maxY)
            {
                maxY = p.Y;
            }

            if (p.Z < minZ)
            {
                minZ = p.Z;
            }
            else if (p.Z > maxZ)
            {
                maxZ = p.Z;
            }
        }

        return new BoundingBox3D(minX, minY, minZ, maxX, maxY, maxZ);
    }

    // ── Nested types ──────────────────────────────────────────────────────────

    private sealed record GpuSeriesData(VertexArrayObject Vao, VertexBuffer Vbo, int Count, BoundingBox3D Bounds);
}
