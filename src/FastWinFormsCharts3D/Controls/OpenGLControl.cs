// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Runtime.InteropServices;
using FastWinFormsCharts3D.Controls.Native;
using FastWinFormsCharts3D.Exceptions;
using Silk.NET.OpenGL;

namespace FastWinFormsCharts3D.Controls;

/// <summary>
/// A WinForms <see cref="UserControl"/> that creates and owns a WGL OpenGL context.
/// Subclass this control (or use <see cref="Chart3DControl"/>) to receive
/// <see cref="Render"/> callbacks with an active, current GL context.
/// </summary>
/// <remarks>
/// <para>
/// Context lifecycle:
/// <list type="bullet">
///   <item>Created in <see cref="OnHandleCreated"/> — after the Win32 HWND is available.</item>
///   <item>Destroyed in <see cref="OnHandleDestroyed"/> and <see cref="Dispose(bool)"/>.</item>
/// </list>
/// </para>
/// <para>
/// The render loop is driven by <see cref="Application.Idle"/>:
/// each idle cycle invalidates the control, causing a <c>WM_PAINT</c> → <see cref="OnPaint"/>.
/// </para>
/// </remarks>
public class OpenGLControl : UserControl
{
    // ── Win32 handles ─────────────────────────────────────────────────────────
    private nint _hwnd;
    private nint _hdc;
    private nint _hglrc;
    private nint _opengl32Handle;

    // ── GL_PROGRAM_POINT_SIZE — lets vertex shader control gl_PointSize ───────
    private const int GlProgramPointSize = 0x8642;

    private bool _contextCreated;

    /// <summary>
    /// Raised each frame after <c>glClear</c> and before <c>SwapBuffers</c>.
    /// Subscribe to this event to issue OpenGL draw calls.
    /// </summary>
    public event EventHandler<RenderEventArgs>? Render;

    /// <summary>
    /// Gets the <see cref="Silk.NET.OpenGL.GL"/> API bound to the active WGL context.
    /// Returns <see langword="null"/> before handle creation or after destruction.
    /// </summary>
    public GL? GL { get; private set; }

    // ── Handle lifecycle ──────────────────────────────────────────────────────

    /// <inheritdoc />
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        if (DesignMode)
        {
            return;
        }

        SetupGLContext();
        Application.Idle += OnApplicationIdle;
    }

    /// <inheritdoc />
    protected override void OnHandleDestroyed(EventArgs e)
    {
        Application.Idle -= OnApplicationIdle;
        TearDownGLContext();
        base.OnHandleDestroyed(e);
    }

    // ── Painting ──────────────────────────────────────────────────────────────

    /// <inheritdoc />
    protected override void OnPaint(PaintEventArgs e)
    {
        if (!_contextCreated || GL is null)
        {
            base.OnPaint(e);
            return;
        }

        // Ensure context is current on this thread (safe for single-threaded WinForms)
        NativeMethods.WglMakeCurrent(_hdc, _hglrc);

        GL.Viewport(0, 0, (uint)Width, (uint)Height);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        OnRender(new RenderEventArgs(GL));

        NativeMethods.SwapBuffers(_hdc);
    }

    /// <inheritdoc />
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (_contextCreated)
        {
            Invalidate();
        }
    }

    // ── Render event ──────────────────────────────────────────────────────────

    /// <summary>Raises the <see cref="Render"/> event.</summary>
    /// <param name="e">Arguments containing the active <see cref="Silk.NET.OpenGL.GL"/> API.</param>
    protected virtual void OnRender(RenderEventArgs e)
    {
        Render?.Invoke(this, e);
    }

    // ── Dispose ───────────────────────────────────────────────────────────────

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Application.Idle -= OnApplicationIdle;
            TearDownGLContext();
        }

        base.Dispose(disposing);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void OnApplicationIdle(object? sender, EventArgs e)
    {
        if (IsHandleCreated && !IsDisposed && _contextCreated)
        {
            Invalidate();
        }
    }

    private void SetupGLContext()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.Opaque
            | ControlStyles.UserPaint,
            value: true);
        UpdateStyles();

        _hwnd = Handle;
        _hdc = NativeMethods.GetDC(_hwnd);
        if (_hdc == 0)
        {
            throw new OpenGLContextException(
                "GetDC failed — cannot obtain device context for the control HWND.",
                Marshal.GetLastWin32Error());
        }

        PixelFormatDescriptor pfd = new()
        {
            nSize = (ushort)Marshal.SizeOf<PixelFormatDescriptor>(),
            nVersion = 1,
            dwFlags = PixelFormatDescriptor.DrawToWindow
                      | PixelFormatDescriptor.SupportOpenGl
                      | PixelFormatDescriptor.DoubleBuffer,
            iPixelType = PixelFormatDescriptor.TypeRgba,
            cColorBits = 32,
            cDepthBits = 24,
            cStencilBits = 8,
        };

        int pixelFormat = NativeMethods.ChoosePixelFormat(_hdc, ref pfd);
        if (pixelFormat == 0)
        {
            throw new OpenGLContextException(
                "ChoosePixelFormat failed — no matching pixel format found.",
                Marshal.GetLastWin32Error());
        }

        if (!NativeMethods.SetPixelFormat(_hdc, pixelFormat, ref pfd))
        {
            throw new OpenGLContextException(
                "SetPixelFormat failed.",
                Marshal.GetLastWin32Error());
        }

        _hglrc = NativeMethods.WglCreateContext(_hdc);
        if (_hglrc == 0)
        {
            throw new OpenGLContextException(
                "wglCreateContext failed — check that opengl32.dll is available.",
                Marshal.GetLastWin32Error());
        }

        if (!NativeMethods.WglMakeCurrent(_hdc, _hglrc))
        {
            throw new OpenGLContextException(
                "wglMakeCurrent failed.",
                Marshal.GetLastWin32Error());
        }

        _opengl32Handle = NativeMethods.LoadLibraryW("opengl32.dll");
        GL = Silk.NET.OpenGL.GL.GetApi(new WglNativeContext(_opengl32Handle));

        // Default render state
        GL.ClearColor(0.08f, 0.08f, 0.12f, 1f);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable((EnableCap)GlProgramPointSize); // gl_PointSize in vertex shaders

        _contextCreated = true;
    }

    private void TearDownGLContext()
    {
        _contextCreated = false;

        GL?.Dispose();
        GL = null;

        if (_hglrc != 0)
        {
            NativeMethods.WglMakeCurrent(0, 0);
            NativeMethods.WglDeleteContext(_hglrc);
            _hglrc = 0;
        }

        if (_hdc != 0 && _hwnd != 0)
        {
            NativeMethods.ReleaseDC(_hwnd, _hdc);
            _hdc = 0;
        }

        // opengl32.dll is a system library — do not FreeLibrary.
        _opengl32Handle = 0;
        _hwnd = 0;
    }
}
