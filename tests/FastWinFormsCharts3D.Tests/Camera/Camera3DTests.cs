// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Numerics;

namespace FastWinFormsCharts3D.Tests.Camera;

/// <summary>Unit tests for <see cref="Camera3D"/>.</summary>
public sealed class Camera3DTests
{
    [Fact]
    public void DefaultCamera_HasReasonableValues()
    {
        Camera3D camera = new();

        Assert.Equal(Vector3.Zero, camera.Target);
        Assert.True(camera.Radius > 0);
        Assert.True(camera.FieldOfView > 0);
    }

    [Fact]
    public void Orbit_ChangesAzimuth()
    {
        Camera3D camera = new();
        float initialAzimuth = camera.Azimuth;

        camera.Orbit(30f, 0f);

        Assert.NotEqual(initialAzimuth, camera.Azimuth);
    }

    [Fact]
    public void Orbit_ElevationIsClamped()
    {
        Camera3D camera = new();

        camera.Orbit(0f, 9999f);

        Assert.True(camera.Elevation <= 89f);
    }

    [Fact]
    public void Zoom_DecreasesRadius()
    {
        Camera3D camera = new();
        float initial = camera.Radius;

        camera.Zoom(1f);

        Assert.True(camera.Radius < initial);
    }

    [Fact]
    public void Zoom_RadiusNeverGoesNegative()
    {
        Camera3D camera = new();

        camera.Zoom(float.MaxValue);

        Assert.True(camera.Radius > 0);
    }

    [Fact]
    public void ViewMatrix_IsNotIdentityAfterOrbit()
    {
        Camera3D camera = new();
        camera.Orbit(90f, 0f);

        Matrix4x4 view = camera.ViewMatrix;

        Assert.NotEqual(Matrix4x4.Identity, view);
    }
}
