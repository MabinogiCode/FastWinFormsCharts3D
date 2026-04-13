// Copyright (c) 2026 MabinogiCode. All rights reserved.

namespace FastWinFormsCharts3D.Camera;

/// <summary>
/// Represents the rectangular region of the rendering surface used for output.
/// </summary>
/// <param name="X">The left edge of the viewport in pixels.</param>
/// <param name="Y">The bottom edge of the viewport in pixels.</param>
/// <param name="Width">The width of the viewport in pixels.</param>
/// <param name="Height">The height of the viewport in pixels.</param>
public readonly record struct Viewport(int X, int Y, int Width, int Height)
{
    /// <summary>Gets the aspect ratio (width / height). Returns 1 when height is zero.</summary>
    public float AspectRatio => Height == 0 ? 1f : (float)Width / Height;
}
