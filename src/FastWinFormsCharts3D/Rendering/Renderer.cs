// Copyright (c) 2026 MabinogiCode. All rights reserved.

using FastWinFormsCharts3D.Rendering.Abstractions;
using Silk.NET.OpenGL;

namespace FastWinFormsCharts3D.Rendering;

/// <summary>
/// A straightforward immediate-mode renderer that issues direct <c>glDrawArrays</c> calls.
/// </summary>
/// <remarks>
/// This is the default renderer for v0.1/v0.2. Future versions will provide an
/// instanced renderer for large datasets.
/// </remarks>
public sealed class Renderer : IRenderer
{
    private GL? _gl;
    private bool _disposed;

    /// <inheritdoc />
    public void Begin(GL gl)
    {
        _gl = gl;
    }

    /// <inheritdoc />
    public void Submit(VertexArrayObject vao, int vertexCount, PrimitiveType primitiveType)
    {
        if (_gl is null)
        {
            return;
        }

        vao.Bind();
        _gl.DrawArrays(primitiveType, 0, (uint)vertexCount);
        vao.Unbind();
    }

    /// <inheritdoc />
    public void End()
    {
        _gl = null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _gl = null;
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
