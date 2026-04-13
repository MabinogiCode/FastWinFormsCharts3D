// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Numerics;
using FastWinFormsCharts3D.Camera;
using FastWinFormsCharts3D.Charts.Abstractions;
using FastWinFormsCharts3D.DataModels;
using FastWinFormsCharts3D.Rendering;
using Silk.NET.OpenGL;

namespace FastWinFormsCharts3D.Charts.Bar;

/// <summary>
/// A 3D bar chart that renders a <see cref="BarSeries3D"/> grid using instanced rendering
/// (<c>glDrawArraysInstanced</c>) — one draw call regardless of bar count.
/// </summary>
/// <remarks>
/// <para>Rendering pipeline:</para>
/// <list type="number">
///   <item>Shaders compiled from embedded resources on <see cref="Initialize"/>.</item>
///   <item>A shared unit-cube VBO (36 vertices, never rebuilt) provides base geometry.</item>
///   <item>A per-instance VBO stores <c>(barX, barZ, barHeight)</c> — 12 bytes per bar.</item>
///   <item>MVP + bar dimensions uploaded per frame; Viridis colours bars by normalised height.</item>
///   <item>X/Y/Z axes rendered via <see cref="AxisRenderer"/> with proper R/G/B colours.</item>
/// </list>
/// <para>
/// <see cref="BarSeries3D.DataChanged"/> triggers a rebuild of the instance VBO + VAO only;
/// the unit-cube VBO is reused.
/// </para>
/// </remarks>
public sealed class BarChart3D : IChart3D
{
    // ── Embedded resource names ───────────────────────────────────────────────
    private const string VertResourceName = "FastWinFormsCharts3D.Rendering.Shaders.bar.vert";
    private const string FragResourceName = "FastWinFormsCharts3D.Rendering.Shaders.bar.frag";

    // ── Unit cube: 6 faces × 2 triangles × 3 vertices × 3 floats = 108 floats ──
    private static readonly float[] UnitCubeVertices =
    [
        // Front (z = 1)
        0f,0f,1f,  1f,0f,1f,  1f,1f,1f,   0f,0f,1f,  1f,1f,1f,  0f,1f,1f,
        // Back (z = 0)
        1f,0f,0f,  0f,0f,0f,  0f,1f,0f,   1f,0f,0f,  0f,1f,0f,  1f,1f,0f,
        // Left (x = 0)
        0f,0f,0f,  0f,0f,1f,  0f,1f,1f,   0f,0f,0f,  0f,1f,1f,  0f,1f,0f,
        // Right (x = 1)
        1f,0f,1f,  1f,0f,0f,  1f,1f,0f,   1f,0f,1f,  1f,1f,0f,  1f,1f,1f,
        // Top (y = 1)
        0f,1f,0f,  0f,1f,1f,  1f,1f,1f,   0f,1f,0f,  1f,1f,1f,  1f,1f,0f,
        // Bottom (y = 0)
        0f,0f,1f,  0f,0f,0f,  1f,0f,0f,   0f,0f,1f,  1f,0f,0f,  1f,0f,1f,
    ];

    // ── GPU state ─────────────────────────────────────────────────────────────
    private GL? _gl;
    private ShaderProgram? _shaderProgram;
    private VertexBuffer? _cubeVbo;
    private VertexArrayObject? _vao;
    private VertexBuffer? _instanceVbo;
    private int _instanceCount;
    private float _yMin;
    private float _yMax;
    private float _barWorldWidth;
    private float _barWorldDepth;
    private AxisRenderer? _axisRenderer;

    // ── Domain state ──────────────────────────────────────────────────────────
    private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;
    private int _viewportWidth = 800;
    private int _viewportHeight = 600;
    private bool _disposed;

    /// <summary>Initialises a new chart bound to the given bar series.</summary>
    /// <param name="series">The bar height grid to render.</param>
    public BarChart3D(BarSeries3D series)
    {
        Series = series;
    }

    // ── IChart3D ──────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public string Title { get; set; } = "Bar 3D";

    /// <inheritdoc />
    public bool IsInitialized { get; private set; }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Gets the bar series rendered by this chart.</summary>
    public BarSeries3D Series { get; }

    // ── IChart3D implementation ───────────────────────────────────────────────

    /// <inheritdoc />
    public void Initialize(GL gl)
    {
        _gl = gl;
        _shaderProgram = ShaderProgram.FromEmbeddedResource(gl, VertResourceName, FragResourceName);

        // Unit cube is static — create once, never rebuild.
        _cubeVbo = VertexBuffer.Create(gl, UnitCubeVertices);

        Series.DataChanged += OnDataChanged;
        UploadInstanceData(gl);

        _axisRenderer = new AxisRenderer();
        _axisRenderer.Initialize(gl);

        UpdateProjection();
        IsInitialized = true;
    }

    /// <inheritdoc />
    public void Render(GL gl, Camera3D camera)
    {
        if (_shaderProgram is null || _vao is null || _instanceCount == 0)
        {
            return;
        }

        Matrix4x4 mvp = camera.ViewMatrix * _projectionMatrix;

        _shaderProgram.Use();
        _shaderProgram.SetUniform("uMVP", mvp);
        _shaderProgram.SetUniform("uBarWidth", _barWorldWidth);
        _shaderProgram.SetUniform("uBarDepth", _barWorldDepth);
        _shaderProgram.SetUniform("uYMin", _yMin);
        _shaderProgram.SetUniform("uYMax", _yMax);

        _vao.Bind();
        gl.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, _instanceCount);
        _vao.Unbind();

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
            Series.DataChanged -= OnDataChanged;
            _shaderProgram?.Dispose();
            _cubeVbo?.Dispose();
            FreeInstanceBuffers();
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

    private void UploadInstanceData(GL gl)
    {
        FreeInstanceBuffers();

        if (Series.Rows == 0 || Series.Cols == 0)
        {
            return;
        }

        float cellWidth = 2f / Series.Cols;
        float cellDepth = 2f / Series.Rows;
        _barWorldWidth = cellWidth * Series.BarWidthFraction;
        _barWorldDepth = cellDepth * Series.BarDepthFraction;

        float xPad = (cellWidth - _barWorldWidth) * 0.5f;
        float zPad = (cellDepth - _barWorldDepth) * 0.5f;

        float[] instanceData = BuildInstanceData(Series, cellWidth, cellDepth, xPad, zPad);
        (_yMin, _yMax) = FindValueRange(Series);
        _instanceCount = Series.Rows * Series.Cols;

        _instanceVbo = VertexBuffer.Create(gl, instanceData);

        // VAO constructor calls Bind() — VAO is active from here.
        _vao = new VertexArrayObject(gl);

        // Attrib 0: per-vertex unit-cube position (divisor = 0, advances each vertex).
        _cubeVbo!.Bind();
        _vao.AddVertexAttributePointer(0, 3, VertexAttribPointerType.Float, false, (uint)(3 * sizeof(float)), 0);

        // Attrib 1: per-instance transform (divisor = 1, advances once per instance).
        _instanceVbo.Bind();
        _vao.AddVertexAttributePointer(1, 3, VertexAttribPointerType.Float, false, (uint)(3 * sizeof(float)), 0);
        gl.VertexAttribDivisor(1, 1);

        _vao.Unbind();
        _cubeVbo.Unbind();
        _instanceVbo.Unbind();
    }

    private void FreeInstanceBuffers()
    {
        _vao?.Dispose();
        _instanceVbo?.Dispose();
        _vao = null;
        _instanceVbo = null;
        _instanceCount = 0;
    }

    private void OnDataChanged(object? sender, EventArgs e)
    {
        if (_gl is not null)
        {
            UploadInstanceData(_gl);
        }
    }

    private static float[] BuildInstanceData(
        BarSeries3D series,
        float cellWidth,
        float cellDepth,
        float xPad,
        float zPad)
    {
        int rows = series.Rows;
        int cols = series.Cols;
        float[] data = new float[rows * cols * 3];
        int idx = 0;

        for (int r = 0; r < rows; r++)
        {
            float zFront = -1f + (r * cellDepth) + zPad;

            for (int c = 0; c < cols; c++)
            {
                float xLeft = -1f + (c * cellWidth) + xPad;
                data[idx++] = xLeft;
                data[idx++] = zFront;
                data[idx++] = series[r, c];
            }
        }

        return data;
    }

    private static (float yMin, float yMax) FindValueRange(BarSeries3D series)
    {
        float min = float.MaxValue;
        float max = float.MinValue;

        for (int r = 0; r < series.Rows; r++)
        {
            for (int c = 0; c < series.Cols; c++)
            {
                float v = MathF.Max(series[r, c], 0f);

                if (v < min)
                {
                    min = v;
                }

                if (v > max)
                {
                    max = v;
                }
            }
        }

        return (min > max ? 0f : min, min > max ? 1f : max);
    }
}
