// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Numerics;

namespace FastWinFormsCharts3D.Camera;

/// <summary>
/// An orbital (azimuth/elevation/radius) camera that circles a fixed target point.
/// Matches the interaction model of Plotly 3D charts — drag to orbit, scroll to zoom.
/// </summary>
public sealed class Camera3D
{
    private const float MinElevation = -89f;
    private const float MaxElevation = 89f;
    private const float MinRadius = 0.1f;

    private Vector3 _position;

    /// <summary>Initializes a new <see cref="Camera3D"/> with default orbital parameters.</summary>
    public Camera3D()
    {
        Target = Vector3.Zero;
        Azimuth = 45f;
        Elevation = 30f;
        Radius = 5f;
        FieldOfView = 45f;
        RecalculatePosition();
    }

    /// <summary>Gets or sets the point the camera orbits around.</summary>
    public Vector3 Target { get; set; }

    /// <summary>Gets or sets the horizontal rotation angle in degrees.</summary>
    public float Azimuth { get; private set; }

    /// <summary>Gets or sets the vertical tilt angle in degrees, clamped to [-89, 89].</summary>
    public float Elevation { get; private set; }

    /// <summary>Gets or sets the distance from the camera to <see cref="Target"/>.</summary>
    public float Radius { get; private set; }

    /// <summary>Gets or sets the vertical field-of-view in degrees.</summary>
    public float FieldOfView { get; set; }

    /// <summary>Gets the view matrix for the current camera state.</summary>
    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(_position, Target, Vector3.UnitY);

    /// <summary>Orbits the camera by the given delta angles (in degrees).</summary>
    /// <param name="deltaAzimuth">Change in horizontal angle.</param>
    /// <param name="deltaElevation">Change in vertical angle.</param>
    public void Orbit(float deltaAzimuth, float deltaElevation)
    {
        Azimuth = (Azimuth + deltaAzimuth) % 360f;
        Elevation = Math.Clamp(Elevation + deltaElevation, MinElevation, MaxElevation);
        RecalculatePosition();
    }

    /// <summary>Adjusts the zoom by moving the camera closer to or farther from <see cref="Target"/>.</summary>
    /// <param name="delta">Positive values zoom in, negative values zoom out.</param>
    public void Zoom(float delta)
    {
        Radius = MathF.Max(MinRadius, Radius - delta);
        RecalculatePosition();
    }

    /// <summary>Pans the target point along the camera's local XY plane.</summary>
    /// <param name="delta">Screen-space delta in pixels (will be scaled internally).</param>
    public void Pan(Vector2 delta)
    {
        // Compute camera-local right and up vectors for panning
        Vector3 forward = Vector3.Normalize(Target - _position);
        Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
        Vector3 up = Vector3.Cross(right, forward);

        float scale = Radius * 0.001f;
        Target -= right * delta.X * scale;
        Target += up * delta.Y * scale;
        RecalculatePosition();
    }

    private void RecalculatePosition()
    {
        float azRad = Azimuth * (MathF.PI / 180f);
        float elRad = Elevation * (MathF.PI / 180f);

        _position = Target + new Vector3(
            Radius * MathF.Cos(elRad) * MathF.Sin(azRad),
            Radius * MathF.Sin(elRad),
            Radius * MathF.Cos(elRad) * MathF.Cos(azRad));
    }
}
