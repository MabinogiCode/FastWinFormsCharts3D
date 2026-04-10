// Copyright (c) 2026 MabinogiCode. All rights reserved.

using Silk.NET.Core.Contexts;

namespace FastWinFormsCharts3D.Controls.Native;

/// <summary>
/// Implements <see cref="INativeContext"/> for a WGL (Windows OpenGL) rendering context,
/// providing Silk.NET with a proc-address loader backed by <c>wglGetProcAddress</c>
/// and <c>GetProcAddress</c> from <c>opengl32.dll</c>.
/// </summary>
/// <remarks>
/// <para>
/// <c>wglGetProcAddress</c> returns null for core OpenGL functions on some drivers.
/// The fallback to <c>GetProcAddress(opengl32Handle, name)</c> covers those cases.
/// </para>
/// </remarks>
internal sealed class WglNativeContext : INativeContext
{
    private readonly nint _opengl32Handle;

    /// <summary>
    /// Initialises the context with the handle to the loaded <c>opengl32.dll</c> module.
    /// </summary>
    /// <param name="opengl32Handle">Module handle obtained via <c>LoadLibraryW("opengl32.dll")</c>.</param>
    internal WglNativeContext(nint opengl32Handle)
    {
        _opengl32Handle = opengl32Handle;
    }

    /// <inheritdoc />
    public nint GetProcAddress(string proc, int? slot = default)
    {
        nint addr = NativeMethods.WglGetProcAddress(proc);
        if (addr == 0)
        {
            addr = NativeMethods.GetProcAddress(_opengl32Handle, proc);
        }

        return addr;
    }

    /// <inheritdoc />
    public bool TryGetProcAddress(string proc, out nint addr, int? slot = default)
    {
        addr = GetProcAddress(proc, slot);
        return addr != 0;
    }

    /// <inheritdoc />
    public bool IsExtensionPresent(string extension) => false;

    /// <inheritdoc />
    /// <remarks>
    /// The <c>opengl32.dll</c> module is a system library owned by the OS loader.
    /// Do not call <c>FreeLibrary</c> on it here — the control's teardown manages lifetime.
    /// </remarks>
    public void Dispose()
    {
        // opengl32.dll lifetime is managed by OpenGLControl.TearDownGLContext.
        GC.SuppressFinalize(this);
    }
}
