// Copyright (c) 2026 MabinogiCode. All rights reserved.

using Silk.NET.OpenGL;

namespace FastWinFormsCharts3D.Rendering;

/// <summary>
/// Wraps an OpenGL Vertex Array Object (VAO), which records vertex attribute layout bindings.
/// </summary>
public sealed class VertexArrayObject : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;
    private bool _disposed;

    /// <summary>Creates and binds a new <see cref="VertexArrayObject"/>.</summary>
    /// <param name="gl">The active OpenGL API.</param>
    public VertexArrayObject(GL gl)
    {
        _gl = gl;
        _handle = gl.GenVertexArray();
        Bind();
    }

    /// <summary>Binds this VAO, making it the active vertex array.</summary>
    public void Bind() => _gl.BindVertexArray(_handle);

    /// <summary>Unbinds the current VAO.</summary>
    public void Unbind() => _gl.BindVertexArray(0);

    /// <summary>
    /// Defines a vertex attribute pointer and enables the attribute.
    /// The relevant VBO must be bound before calling this method.
    /// </summary>
    /// <param name="index">The attribute location in the shader.</param>
    /// <param name="size">Number of components (1–4).</param>
    /// <param name="type">The component data type.</param>
    /// <param name="normalized">Whether to normalize integer values to [0,1] or [-1,1].</param>
    /// <param name="stride">Byte offset between consecutive vertex attributes.</param>
    /// <param name="offset">Byte offset of the first component in the buffer.</param>
    public void AddVertexAttributePointer(
        uint index,
        int size,
        VertexAttribPointerType type,
        bool normalized,
        uint stride,
        int offset)
    {
        _gl.EnableVertexAttribArray(index);
        _gl.VertexAttribPointer(index, size, type, normalized, stride, (nint)offset);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _gl.DeleteVertexArray(_handle);
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
