// Copyright (c) 2026 MabinogiCode. All rights reserved.

using Silk.NET.OpenGL;

namespace FastWinFormsCharts3D.Controls;

/// <summary>
/// A WinForms <see cref="UserControl"/> that owns a WGL OpenGL context.
/// Subclass this control to receive <see cref="Render"/> callbacks with an active GL context.
/// </summary>
/// <remarks>
/// <para>
/// The OpenGL context is created in <see cref="OnHandleCreated"/> and destroyed in
/// <see cref="OnHandleDestroyed"/>. The rendering loop is driven by <see cref="OnPaint"/>.
/// </para>
/// <para>
/// Full WGL bootstrapping (wglCreateContext, wglMakeCurrent, SwapBuffers) is implemented
/// in milestone v0.2. This stub provides the public API surface and event contract.
/// </para>
/// </remarks>
public class OpenGLControl : UserControl
{
    /// <summary>
    /// Raised each time a frame should be rendered.
    /// Subscribe to this event to issue OpenGL draw calls.
    /// </summary>
    public event EventHandler<RenderEventArgs>? Render;

    /// <summary>
    /// Gets the <see cref="Silk.NET.OpenGL.GL"/> API instance for the active context.
    /// Returns <see langword="null"/> before <see cref="OnHandleCreated"/> is called.
    /// </summary>
    public GL? GL { get; private set; }

    /// <inheritdoc />
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        // TODO (v0.2): Create the WGL pixel format, context, and initialise Silk.NET GL.
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
    }

    /// <inheritdoc />
    protected override void OnHandleDestroyed(EventArgs e)
    {
        // TODO (v0.2): Release WGL context and Silk.NET GL resources.
        GL?.Dispose();
        GL = null;
        base.OnHandleDestroyed(e);
    }

    /// <inheritdoc />
    protected override void OnPaint(PaintEventArgs e)
    {
        if (GL is null)
        {
            base.OnPaint(e);
            return;
        }

        OnRender(new RenderEventArgs(GL));

        // TODO (v0.2): Call SwapBuffers(hdc) here after Render completes.
    }

    /// <inheritdoc />
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (GL is not null)
        {
            // TODO (v0.2): Call glViewport(0, 0, Width, Height).
        }
    }

    /// <summary>Raises the <see cref="Render"/> event.</summary>
    /// <param name="e">Event arguments containing the active <see cref="Silk.NET.OpenGL.GL"/> API.</param>
    protected virtual void OnRender(RenderEventArgs e)
    {
        Render?.Invoke(this, e);
    }
}
