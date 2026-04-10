// Copyright (c) 2026 MabinogiCode. All rights reserved.

using Silk.NET.OpenGL;

namespace FastWinFormsCharts3D.Rendering.Abstractions;

/// <summary>
/// Defines a rendering strategy for submitting draw calls to OpenGL.
/// Implementations can vary the draw method (direct, instanced, indirect).
/// </summary>
public interface IRenderer : IDisposable
{
    /// <summary>Signals the start of a render pass. Bind shader and set global state here.</summary>
    /// <param name="gl">The active OpenGL API.</param>
    void Begin(GL gl);

    /// <summary>Submits a single draw call for a <see cref="VertexArrayObject"/>.</summary>
    /// <param name="vao">The vertex array object to draw.</param>
    /// <param name="vertexCount">The number of vertices to draw.</param>
    /// <param name="primitiveType">The OpenGL primitive topology.</param>
    void Submit(VertexArrayObject vao, int vertexCount, PrimitiveType primitiveType);

    /// <summary>Signals the end of a render pass. Unbind resources here.</summary>
    void End();
}
