// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Drawing;
using System.Numerics;
using FastWinFormsCharts3D.Camera;
using FastWinFormsCharts3D.Charts.Abstractions;
using FastWinFormsCharts3D.DataModels;
using FastWinFormsCharts3D.Rendering;
using Silk.NET.OpenGL;

namespace FastWinFormsCharts3D.Charts.Line;

/// <summary>
/// A 3D line chart that renders one or more <see cref="LineSeries3D"/> as smooth tube meshes.
/// </summary>
/// <remarks>
/// <para>Rendering pipeline:</para>
/// <list type="number">
///   <item>Shaders compiled from embedded resources on <see cref="Initialize"/>.</item>
///   <item>Each series is tessellated into an N-sided tube (8 faces) on the CPU; geometry
///     is uploaded as an interleaved VBO (XYZ + cumulative arc-length) + EBO.</item>
///   <item>MVP and per-series uniforms (colour, dashed params) uploaded per frame.</item>
///   <item>Dashed pattern: fragment shader discards fragments where
///     <c>mod(vDistance, dashLength + gapLength) &gt; dashLength</c>.</item>
///   <item>X/Y/Z axes rendered via <see cref="AxisRenderer"/>.</item>
/// </list>
/// <para>
/// <see cref="LineSeries3D.DataChanged"/> triggers a full tube rebuild for that series.
/// </para>
/// </remarks>
public sealed class LineChart3D : IChart3D
{
    // ── Embedded resource names ───────────────────────────────────────────────
    private const string VertResourceName = "FastWinFormsCharts3D.Rendering.Shaders.line.vert";
    private const string FragResourceName = "FastWinFormsCharts3D.Rendering.Shaders.line.frag";

    // ── Tube tessellation quality ─────────────────────────────────────────────
    private const int TubeSides = 8;

    // ── GPU state ─────────────────────────────────────────────────────────────
    private GL? _gl;
    private ShaderProgram? _shaderProgram;
    private readonly Dictionary<string, GpuTubeData> _gpuBuffers = [];
    private AxisRenderer? _axisRenderer;

    // ── Domain state ──────────────────────────────────────────────────────────
    private readonly List<LineSeries3D> _series = [];
    private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;
    private int _viewportWidth = 800;
    private int _viewportHeight = 600;
    private bool _disposed;

    // ── IChart3D ──────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public string Title { get; set; } = "Line 3D";

    /// <inheritdoc />
    public bool IsInitialized { get; private set; }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Gets the series currently registered with this chart.</summary>
    public IReadOnlyList<LineSeries3D> Series => _series;

    /// <summary>
    /// Adds a series. If the chart is already initialized, uploads tube geometry to the GPU immediately.
    /// </summary>
    /// <param name="series">The line series to add.</param>
    public void AddSeries(LineSeries3D series)
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
        LineSeries3D? target = _series.Find(static s => s.Name == seriesName);
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

        foreach (LineSeries3D series in _series)
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

        Matrix4x4 mvp = camera.ViewMatrix * _projectionMatrix;

        _shaderProgram.Use();
        _shaderProgram.SetUniform("uMVP", mvp);

        foreach (LineSeries3D series in _series)
        {
            if (!series.IsVisible || !_gpuBuffers.TryGetValue(series.Name, out GpuTubeData? gpu))
            {
                continue;
            }

            Color c = series.Color;
            _shaderProgram.SetUniform("uColor", new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f));
            _shaderProgram.SetUniform("uIsDashed", series.IsDashed ? 1f : 0f);
            _shaderProgram.SetUniform("uDashLength", series.DashLength);
            _shaderProgram.SetUniform("uGapLength", series.GapLength);

            gpu.Vao.Bind();
            gl.DrawElements(PrimitiveType.Triangles, (uint)gpu.IndexCount, DrawElementsType.UnsignedInt, 0);
            gpu.Vao.Unbind();
        }

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
            foreach (LineSeries3D series in _series)
            {
                series.DataChanged -= OnSeriesDataChanged;
            }

            _shaderProgram?.Dispose();

            foreach (GpuTubeData gpu in _gpuBuffers.Values)
            {
                gpu.Vao.Dispose();
                gpu.Vbo.Dispose();
                gpu.Ebo.Dispose();
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

    private void UploadSeriesToGpu(GL gl, LineSeries3D series)
    {
        FreeGpuSeries(series.Name);

        (float[] vertices, uint[] indices) = BuildTubeGeometry(series);
        if (indices.Length == 0)
        {
            return;
        }

        // Interleaved VBO: [x, y, z, dist, x, y, z, dist, ...]  (stride = 16 bytes)
        VertexBuffer vbo = VertexBuffer.Create(gl, vertices);
        VertexBuffer ebo = VertexBuffer.Create(gl, indices, BufferTargetARB.ElementArrayBuffer);

        // VAO constructor binds it — EBO must be re-bound while VAO is active.
        VertexArrayObject vao = new(gl);
        ebo.Bind();
        vbo.Bind();
        vao.AddVertexAttributePointer(0, 3, VertexAttribPointerType.Float, false, (uint)(4 * sizeof(float)), 0);
        vao.AddVertexAttributePointer(1, 1, VertexAttribPointerType.Float, false, (uint)(4 * sizeof(float)), 3 * sizeof(float));
        vao.Unbind();
        vbo.Unbind();

        _gpuBuffers[series.Name] = new GpuTubeData(vao, vbo, ebo, indices.Length);
    }

    private void FreeGpuSeries(string seriesName)
    {
        if (_gpuBuffers.TryGetValue(seriesName, out GpuTubeData? existing))
        {
            existing.Vao.Dispose();
            existing.Vbo.Dispose();
            existing.Ebo.Dispose();
            _gpuBuffers.Remove(seriesName);
        }
    }

    private void OnSeriesDataChanged(object? sender, EventArgs e)
    {
        if (_gl is null || sender is not LineSeries3D series)
        {
            return;
        }

        UploadSeriesToGpu(_gl, series);
    }

    // ── Tube geometry ─────────────────────────────────────────────────────────

    private static (float[] vertices, uint[] indices) BuildTubeGeometry(LineSeries3D series)
    {
        ReadOnlySpan<DataPoint3D> pts = series.Points;
        int m = pts.Length;

        if (m < 2)
        {
            return ([], []);
        }

        int n = TubeSides;
        float radius = series.Radius;

        // Compute a smoothed tangent direction at each point.
        Vector3[] tangents = ComputeTangents(pts);

        // Interleaved vertex data: (x, y, z, cumDist) per ring vertex.
        float[] verts = new float[m * n * 4];
        float cumDist = 0f;
        int vi = 0;

        for (int i = 0; i < m; i++)
        {
            if (i > 0)
            {
                cumDist += Vector3.Distance(ToVec3(pts[i]), ToVec3(pts[i - 1]));
            }

            Vector3 center = ToVec3(pts[i]);
            (Vector3 u, Vector3 v) = ComputeRingBasis(tangents[i]);

            for (int j = 0; j < n; j++)
            {
                float angle = j * MathF.Tau / n;
                Vector3 p = center + radius * ((MathF.Cos(angle) * u) + (MathF.Sin(angle) * v));
                verts[vi++] = p.X;
                verts[vi++] = p.Y;
                verts[vi++] = p.Z;
                verts[vi++] = cumDist;
            }
        }

        // Index buffer: (M-1) segments × N quads × 2 triangles × 3 indices.
        uint[] indices = new uint[(m - 1) * n * 6];
        int idx = 0;

        for (int i = 0; i < m - 1; i++)
        {
            for (int j = 0; j < n; j++)
            {
                uint v00 = (uint)(i * n + j);
                uint v01 = (uint)(i * n + ((j + 1) % n));
                uint v10 = (uint)((i + 1) * n + j);
                uint v11 = (uint)((i + 1) * n + ((j + 1) % n));

                indices[idx++] = v00; indices[idx++] = v10; indices[idx++] = v01;
                indices[idx++] = v01; indices[idx++] = v10; indices[idx++] = v11;
            }
        }

        return (verts, indices);
    }

    private static Vector3[] ComputeTangents(ReadOnlySpan<DataPoint3D> pts)
    {
        int m = pts.Length;
        Vector3[] tangents = new Vector3[m];

        for (int i = 0; i < m; i++)
        {
            Vector3 t;

            if (i == 0)
            {
                t = ToVec3(pts[1]) - ToVec3(pts[0]);
            }
            else if (i == m - 1)
            {
                t = ToVec3(pts[m - 1]) - ToVec3(pts[m - 2]);
            }
            else
            {
                // Average of forward and backward segment directions for a smooth joint.
                Vector3 back = Vector3.Normalize(ToVec3(pts[i]) - ToVec3(pts[i - 1]));
                Vector3 fwd = Vector3.Normalize(ToVec3(pts[i + 1]) - ToVec3(pts[i]));
                t = back + fwd;
            }

            float len = t.Length();
            tangents[i] = len > 1e-6f ? t / len : Vector3.UnitZ;
        }

        return tangents;
    }

    private static (Vector3 u, Vector3 v) ComputeRingBasis(Vector3 tangent)
    {
        // Choose a helper vector not parallel to the tangent.
        Vector3 helper = MathF.Abs(tangent.Y) < 0.9f ? Vector3.UnitY : Vector3.UnitX;
        Vector3 u = Vector3.Normalize(Vector3.Cross(tangent, helper));
        Vector3 v = Vector3.Cross(u, tangent);   // unit since tangent ⊥ u and both unit
        return (u, v);
    }

    private static Vector3 ToVec3(DataPoint3D p) => new(p.X, p.Y, p.Z);

    // ── Nested types ──────────────────────────────────────────────────────────

    private sealed record GpuTubeData(VertexArrayObject Vao, VertexBuffer Vbo, VertexBuffer Ebo, int IndexCount);
}
