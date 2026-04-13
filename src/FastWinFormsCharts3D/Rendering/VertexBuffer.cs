// Copyright (c) 2026 MabinogiCode. All rights reserved.

using Silk.NET.OpenGL;

namespace FastWinFormsCharts3D.Rendering;

/// <summary>
/// Wraps an OpenGL Vertex Buffer Object (VBO).
/// </summary>
public sealed class VertexBuffer : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly BufferTargetARB _target;
    private bool _disposed;

    private VertexBuffer(GL gl, uint handle, BufferTargetARB target)
    {
        _gl = gl;
        _handle = handle;
        _target = target;
    }

    /// <summary>
    /// Creates a new VBO and uploads the provided data.
    /// </summary>
    /// <typeparam name="T">An unmanaged value type whose layout matches the shader attribute.</typeparam>
    /// <param name="gl">The active OpenGL API.</param>
    /// <param name="data">The data to upload.</param>
    /// <param name="target">The buffer binding target (default: <c>ArrayBuffer</c>).</param>
    /// <returns>A bound and populated <see cref="VertexBuffer"/>.</returns>
    public static unsafe VertexBuffer Create<T>(
        GL gl,
        T[] data,
        BufferTargetARB target = BufferTargetARB.ArrayBuffer)
        where T : unmanaged
    {
        uint handle = gl.GenBuffer();
        var vbo = new VertexBuffer(gl, handle, target);
        vbo.Bind();
        vbo.UploadData(data);
        return vbo;
    }

    /// <summary>Binds this buffer to its target.</summary>
    public void Bind() => _gl.BindBuffer(_target, _handle);

    /// <summary>Unbinds the current buffer from the target.</summary>
    public void Unbind() => _gl.BindBuffer(_target, 0);

    /// <summary>
    /// Orphans the current storage and re-uploads new data (streaming update pattern).
    /// The buffer must be bound before calling this method.
    /// </summary>
    /// <typeparam name="T">An unmanaged value type.</typeparam>
    /// <param name="data">The replacement data.</param>
    public unsafe void UpdateData<T>(T[] data) where T : unmanaged
    {
        UploadData(data);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _gl.DeleteBuffer(_handle);
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    private unsafe void UploadData<T>(T[] data) where T : unmanaged
    {
        fixed (T* ptr = data)
        {
            _gl.BufferData(_target, (nuint)(data.Length * sizeof(T)), ptr, BufferUsageARB.DynamicDraw);
        }
    }
}
