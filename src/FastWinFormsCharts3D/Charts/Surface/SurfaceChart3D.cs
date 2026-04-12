// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Numerics;
using FastWinFormsCharts3D.Camera;
using FastWinFormsCharts3D.Charts.Abstractions;
using FastWinFormsCharts3D.DataModels;
using FastWinFormsCharts3D.Rendering;
using Silk.NET.OpenGL;

namespace FastWinFormsCharts3D.Charts.Surface;

/// <summary>
/// A 3D surface chart that renders a <see cref="SurfaceData"/> heightmap as a shaded triangle mesh
/// coloured with the Viridis palette.
/// </summary>
/// <remarks>
/// <para>Rendering pipeline:</para>
/// <list type="number">
///   <item>Shaders compiled from embedded resources on <see cref="Initialize"/>.</item>
///   <item>Heightmap baked into a VBO (tightly-packed XYZ floats) + EBO (uint triangle indices).</item>
///   <item>MVP uploaded per frame; model matrix is Identity.</item>
///   <item>Fragment shader maps normalised Y height to Viridis via a degree-6 polynomial.</item>
///   <item>X/Y/Z axis lines rendered via <see cref="AxisRenderer"/> (flat-colour shader).</item>
/// </list>
/// <para>
/// <see cref="SurfaceData.DataChanged"/> triggers a full GPU mesh rebuild on the UI thread.
/// </para>
/// </remarks>
public sealed class SurfaceChart3D : IChart3D
{
    // ── Embedded resource names ───────────────────────────────────────────────
    private const string VertResourceName = "FastWinFormsCharts3D.Rendering.Shaders.surface.vert";
    private const string FragResourceName = "FastWinFormsCharts3D.Rendering.Shaders.surface.frag";

    // ── GPU state ─────────────────────────────────────────────────────────────
    private GL? _gl;
    private ShaderProgram? _shaderProgram;
    private VertexArrayObject? _meshVao;
    private VertexBuffer? _meshVbo;
    private VertexBuffer? _meshEbo;
    private int _indexCount;
    private float _yMin;
    private float _yMax;
    private AxisRenderer? _axisRenderer;

    // ── Domain state ──────────────────────────────────────────────────────────
    private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;
    private int _viewportWidth = 800;
    private int _viewportHeight = 600;
    private bool _disposed;

    /// <summary>Initialises a new chart bound to the given surface data.</summary>
    /// <param name="data">The heightmap to render.</param>
    public SurfaceChart3D(SurfaceData data)
    {
        Data = data;
    }

    // ── IChart3D ──────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public string Title { get; set; } = "Surface 3D";

    /// <inheritdoc />
    public bool IsInitialized { get; private set; }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Gets the heightmap rendered by this chart.</summary>
    public SurfaceData Data { get; }

    // ── IChart3D implementation ───────────────────────────────────────────────

    /// <inheritdoc />
    public void Initialize(GL gl)
    {
        _gl = gl;
        _shaderProgram = ShaderProgram.FromEmbeddedResource(gl, VertResourceName, FragResourceName);

        Data.DataChanged += OnDataChanged;
        UploadMesh(gl);

        _axisRenderer = new AxisRenderer();
        _axisRenderer.Initialize(gl);

        UpdateProjection();
        IsInitialized = true;
    }

    /// <inheritdoc />
    public void Render(GL gl, Camera3D camera)
    {
        if (_shaderProgram is null || _meshVao is null)
        {
            return;
        }

        Matrix4x4 mvp = camera.ViewMatrix * _projectionMatrix;

        _shaderProgram.Use();
        _shaderProgram.SetUniform("uMVP", mvp);
        _shaderProgram.SetUniform("uYMin", _yMin);
        _shaderProgram.SetUniform("uYMax", _yMax);

        _meshVao.Bind();
        gl.DrawElements(PrimitiveType.Triangles, (uint)_indexCount, DrawElementsType.UnsignedInt, 0);
        _meshVao.Unbind();

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
            Data.DataChanged -= OnDataChanged;
            _shaderProgram?.Dispose();
            FreeMeshBuffers();
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

    private void UploadMesh(GL gl)
    {
        FreeMeshBuffers();

        (float[] vertices, uint[] indices) = BuildMesh(Data);

        if (indices.Length == 0)
        {
            return;
        }

        (_yMin, _yMax) = FindYRange(Data);
        _indexCount = indices.Length;

        _meshVbo = VertexBuffer.Create(gl, vertices);
        _meshEbo = VertexBuffer.Create(gl, indices, BufferTargetARB.ElementArrayBuffer);

        // VAO constructor calls Bind() — VAO is active from this point.
        _meshVao = new VertexArrayObject(gl);

        // EBO binding must happen while VAO is active — it is stored in VAO state.
        _meshEbo.Bind();
        _meshVbo.Bind();
        _meshVao.AddVertexAttributePointer(0, 3, VertexAttribPointerType.Float, false, (uint)(3 * sizeof(float)), 0);
        _meshVao.Unbind();

        // Unbind VBO after VAO is inactive (safe). Do not unbind EBO while VAO was bound above.
        _meshVbo.Unbind();
    }

    private void FreeMeshBuffers()
    {
        _meshVao?.Dispose();
        _meshVbo?.Dispose();
        _meshEbo?.Dispose();
        _meshVao = null;
        _meshVbo = null;
        _meshEbo = null;
        _indexCount = 0;
    }

    private void OnDataChanged(object? sender, EventArgs e)
    {
        if (_gl is not null)
        {
            UploadMesh(_gl);
        }
    }

    // ── Static mesh builders ──────────────────────────────────────────────────

    private static (float[] vertices, uint[] indices) BuildMesh(SurfaceData data)
    {
        int rows = data.Rows;
        int cols = data.Cols;

        if (rows < 2 || cols < 2)
        {
            return ([], []);
        }

        float[] vertices = new float[rows * cols * 3];
        int v = 0;

        for (int r = 0; r < rows; r++)
        {
            float z = data.ZMin + ((float)r / (rows - 1)) * (data.ZMax - data.ZMin);

            for (int c = 0; c < cols; c++)
            {
                float x = data.XMin + ((float)c / (cols - 1)) * (data.XMax - data.XMin);
                vertices[v++] = x;
                vertices[v++] = data[r, c];
                vertices[v++] = z;
            }
        }

        uint[] indices = new uint[(rows - 1) * (cols - 1) * 6];
        int idx = 0;

        for (int r = 0; r < rows - 1; r++)
        {
            for (int c = 0; c < cols - 1; c++)
            {
                uint v0 = (uint)(r * cols + c);
                uint v1 = (uint)(r * cols + c + 1);
                uint v2 = (uint)((r + 1) * cols + c);
                uint v3 = (uint)((r + 1) * cols + c + 1);

                indices[idx++] = v0; indices[idx++] = v2; indices[idx++] = v1;
                indices[idx++] = v1; indices[idx++] = v2; indices[idx++] = v3;
            }
        }

        return (vertices, indices);
    }

    private static (float yMin, float yMax) FindYRange(SurfaceData data)
    {
        float min = float.MaxValue;
        float max = float.MinValue;

        for (int r = 0; r < data.Rows; r++)
        {
            for (int c = 0; c < data.Cols; c++)
            {
                float h = data[r, c];

                if (h < min)
                {
                    min = h;
                }

                if (h > max)
                {
                    max = h;
                }
            }
        }

        return (min, max);
    }
}
