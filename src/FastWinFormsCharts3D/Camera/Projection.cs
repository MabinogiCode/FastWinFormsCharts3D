// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Numerics;

namespace FastWinFormsCharts3D.Camera;

/// <summary>
/// Provides factory methods for 3D projection matrices using <see cref="System.Numerics"/>.
/// </summary>
public static class Projection
{
    /// <summary>
    /// Creates a perspective projection matrix.
    /// </summary>
    /// <param name="fovDegrees">The vertical field of view in degrees.</param>
    /// <param name="aspectRatio">The width-to-height aspect ratio.</param>
    /// <param name="nearPlane">The distance to the near clip plane (must be &gt; 0).</param>
    /// <param name="farPlane">The distance to the far clip plane (must be &gt; <paramref name="nearPlane"/>).</param>
    /// <returns>A column-major perspective projection matrix.</returns>
    public static Matrix4x4 Perspective(float fovDegrees, float aspectRatio, float nearPlane, float farPlane)
    {
        float fovRadians = fovDegrees * (MathF.PI / 180f);
        return Matrix4x4.CreatePerspectiveFieldOfView(fovRadians, aspectRatio, nearPlane, farPlane);
    }

    /// <summary>
    /// Creates an orthographic projection matrix.
    /// </summary>
    /// <param name="left">The left boundary of the view volume.</param>
    /// <param name="right">The right boundary of the view volume.</param>
    /// <param name="bottom">The bottom boundary of the view volume.</param>
    /// <param name="top">The top boundary of the view volume.</param>
    /// <param name="near">The distance to the near clip plane.</param>
    /// <param name="far">The distance to the far clip plane.</param>
    /// <returns>A column-major orthographic projection matrix.</returns>
    public static Matrix4x4 Orthographic(float left, float right, float bottom, float top, float near, float far)
    {
        return Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, near, far);
    }
}
