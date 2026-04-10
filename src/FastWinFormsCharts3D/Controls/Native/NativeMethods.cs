// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Runtime.InteropServices;

namespace FastWinFormsCharts3D.Controls.Native;

/// <summary>
/// P/Invoke declarations for Win32 APIs required to create and manage a WGL OpenGL context.
/// </summary>
internal static class NativeMethods
{
    // ── gdi32.dll ─────────────────────────────────────────────────────────────

    /// <summary>Retrieves a handle to a device context for the specified window.</summary>
    /// <param name="hWnd">Handle to the window.</param>
    /// <returns>An HDC if successful; zero on failure.</returns>
    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    internal static extern nint GetDC(nint hWnd);

    /// <summary>Releases a device context previously obtained via <see cref="GetDC"/>.</summary>
    /// <param name="hWnd">Handle to the window whose DC is being released.</param>
    /// <param name="hdc">The HDC to release.</param>
    /// <returns>Nonzero on success.</returns>
    [DllImport("user32.dll", ExactSpelling = true)]
    internal static extern int ReleaseDC(nint hWnd, nint hdc);

    /// <summary>
    /// Attempts to match an appropriate pixel format supported by a device context
    /// against the given <see cref="PixelFormatDescriptor"/>.
    /// </summary>
    /// <param name="hdc">The HDC to query.</param>
    /// <param name="ppfd">A description of the requested pixel format.</param>
    /// <returns>The index of the closest matching pixel format, or 0 on failure.</returns>
    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    internal static extern int ChoosePixelFormat(nint hdc, ref PixelFormatDescriptor ppfd);

    /// <summary>Sets the pixel format of the specified device context.</summary>
    /// <param name="hdc">The target HDC.</param>
    /// <param name="iPixelFormat">The pixel format index from <see cref="ChoosePixelFormat"/>.</param>
    /// <param name="ppfd">The descriptor for the selected format.</param>
    /// <returns><see langword="true"/> on success.</returns>
    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetPixelFormat(nint hdc, int iPixelFormat, ref PixelFormatDescriptor ppfd);

    /// <summary>Exchanges the front and back buffers of a double-buffered surface.</summary>
    /// <param name="hdc">The HDC associated with the drawable surface.</param>
    /// <returns><see langword="true"/> on success.</returns>
    [DllImport("gdi32.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SwapBuffers(nint hdc);

    // ── opengl32.dll ──────────────────────────────────────────────────────────

    /// <summary>Creates a new WGL rendering context for drawing on the specified device.</summary>
    /// <param name="hdc">An HDC whose pixel format has already been set via <see cref="SetPixelFormat"/>.</param>
    /// <returns>An HGLRC on success; zero on failure.</returns>
    [DllImport("opengl32.dll", EntryPoint = "wglCreateContext", ExactSpelling = true, SetLastError = true)]
    internal static extern nint WglCreateContext(nint hdc);

    /// <summary>Makes the specified WGL context current on the calling thread.</summary>
    /// <param name="hdc">The HDC to draw on.</param>
    /// <param name="hglrc">The rendering context to activate, or zero to deactivate.</param>
    /// <returns><see langword="true"/> on success.</returns>
    [DllImport("opengl32.dll", EntryPoint = "wglMakeCurrent", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool WglMakeCurrent(nint hdc, nint hglrc);

    /// <summary>Deletes a WGL rendering context previously created with <see cref="WglCreateContext"/>.</summary>
    /// <param name="hglrc">The rendering context to delete.</param>
    /// <returns><see langword="true"/> on success.</returns>
    [DllImport("opengl32.dll", EntryPoint = "wglDeleteContext", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool WglDeleteContext(nint hglrc);

    /// <summary>
    /// Returns the address of an OpenGL extension function.
    /// Returns zero for core functions on some drivers — fall back to
    /// <see cref="GetProcAddress"/> in that case.
    /// </summary>
    /// <param name="lpszProc">The function name.</param>
    /// <returns>A function pointer, or zero if not found.</returns>
    [DllImport("opengl32.dll", EntryPoint = "wglGetProcAddress", ExactSpelling = true)]
    internal static extern nint WglGetProcAddress([MarshalAs(UnmanagedType.LPStr)] string lpszProc);

    // ── kernel32.dll ──────────────────────────────────────────────────────────

    /// <summary>Loads the specified module into the address space of the calling process.</summary>
    /// <param name="lpLibFileName">The DLL name or path.</param>
    /// <returns>A module handle, or zero on failure.</returns>
    [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern nint LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string lpLibFileName);

    /// <summary>Retrieves the address of an exported function or variable from a loaded module.</summary>
    /// <param name="hModule">A module handle from <see cref="LoadLibraryW"/>.</param>
    /// <param name="lpProcName">The function name.</param>
    /// <returns>A function pointer, or zero if not found.</returns>
    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    internal static extern nint GetProcAddress(nint hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);
}
