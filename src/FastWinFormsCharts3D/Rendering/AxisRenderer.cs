// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Numerics;
using Silk.NET.OpenGL;

namespace FastWinFormsCharts3D.Rendering;

/// <summary>
/// Renders the three world-space orientation axes (X/Y/Z) using a flat-colour shader.
/// Each axis is drawn as a line from −1 to +1 with its canonical colour (red/green/blue).
/// </summary>
/// <remarks>
/// Shared by all <c>IChart3D</c> implementations so axis geometry and shader are not duplicated.
/// </remarks>
internal sealed class AxisRenderer : IDisposable
{
    private const string VertResource = "FastWinFormsCharts3D.Rendering.Shaders.flat.vert";
    private const string FragResource = "FastWinFormsCharts3D.Rendering.Shaders.flat.frag";

    private static readonly float[] Vertices =
    [
        -1f, 0f, 0f,   1f, 0f, 0f,   // X axis
         0f,-1f, 0f,   0f, 1f, 0f,   // Y axis
         0f, 0f,-1f,   0f, 0f, 1f,   // Z axis
    ];

    private static readonly (Vector4 Color, int First, int Count)[] Segments =
    [
        (new Vector4(0.9f, 0.25f, 0.25f, 1f), 0, 2),   // X — red
        (new Vector4(0.25f, 0.9f, 0.25f, 1f), 2, 2),   // Y — green
        (new Vector4(0.25f, 0.5f,  1.0f, 1f), 4, 2),   // Z — blue
    ];

    private ShaderProgram? _shader;
    private VertexArrayObject? _vao;
    private VertexBuffer? _vbo;
    private bool _disposed;

    /// <summary>Compiles the flat shader and uploads axis geometry to the GPU.</summary>
    /// <param name="gl">The active OpenGL API.</param>
    internal void Initialize(GL gl)
    {
        _shader = ShaderProgram.FromEmbeddedResource(gl, VertResource, FragResource);
        _vbo = VertexBuffer.Create(gl, Vertices);
        _vao = new VertexArrayObject(gl);
        _vbo.Bind();
        _vao.AddVertexAttributePointer(0, 3, VertexAttribPointerType.Float, false, (uint)(3 * sizeof(float)), 0);
        _vao.Unbind();
        _vbo.Unbind();
    }

    /// <summary>
    /// Draws the X/Y/Z axis lines using the provided MVP matrix.
    /// Must be called after <see cref="Initialize"/>.
    /// </summary>
    /// <param name="gl">The active OpenGL API.</param>
    /// <param name="mvp">The combined model-view-projection matrix.</param>
    internal void Render(GL gl, Matrix4x4 mvp)
    {
        if (_shader is null || _vao is null)
        {
            return;
        }

        _shader.Use();
        _shader.SetUniform("uMVP", mvp);

        foreach ((Vector4 color, int first, int count) in Segments)
        {
            _shader.SetUniform("uColor", color);
            _vao.Bind();
            gl.DrawArrays(PrimitiveType.Lines, first, (uint)count);
            _vao.Unbind();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _shader?.Dispose();
            _vao?.Dispose();
            _vbo?.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
