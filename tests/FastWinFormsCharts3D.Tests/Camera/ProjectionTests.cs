// Copyright (c) 2026 MabinogiCode. All rights reserved.

using FastWinFormsCharts3D.Camera;

namespace FastWinFormsCharts3D.Tests.Camera;

/// <summary>Unit tests for <see cref="Projection"/>.</summary>
public sealed class ProjectionTests
{
    [Fact]
    public void Perspective_DoesNotReturnIdentity()
    {
        var matrix = Projection.Perspective(45f, 16f / 9f, 0.1f, 1000f);

        Assert.NotEqual(System.Numerics.Matrix4x4.Identity, matrix);
    }

    [Fact]
    public void Orthographic_DoesNotReturnIdentity()
    {
        var matrix = Projection.Orthographic(-1f, 1f, -1f, 1f, 0.1f, 100f);

        Assert.NotEqual(System.Numerics.Matrix4x4.Identity, matrix);
    }
}
