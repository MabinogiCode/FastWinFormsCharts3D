// Copyright (c) 2026 MabinogiCode. All rights reserved.

using Silk.NET.OpenGL;

namespace FastWinFormsCharts3D.Controls;

/// <summary>
/// Provides data for the <see cref="OpenGLControl.Render"/> event.
/// </summary>
public sealed class RenderEventArgs : EventArgs
{
    /// <summary>Initializes a new instance with the given OpenGL API.</summary>
    /// <param name="gl">The active <see cref="Silk.NET.OpenGL.GL"/> API for the current frame.</param>
    public RenderEventArgs(GL gl)
    {
        GL = gl;
    }

    /// <summary>Gets the active <see cref="Silk.NET.OpenGL.GL"/> API for the current frame.</summary>
    public GL GL { get; }
}
