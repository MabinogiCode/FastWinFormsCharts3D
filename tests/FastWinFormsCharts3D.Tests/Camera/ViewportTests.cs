// Copyright (c) 2026 MabinogiCode. All rights reserved.

using FastWinFormsCharts3D.Camera;

namespace FastWinFormsCharts3D.Tests.Camera;

/// <summary>Unit tests for <see cref="Viewport"/>.</summary>
public sealed class ViewportTests
{
    [Fact]
    public void AspectRatio_IsWidthOverHeight()
    {
        Viewport vp = new(0, 0, 1920, 1080);

        float expected = 1920f / 1080f;

        Assert.Equal(expected, vp.AspectRatio, precision: 4);
    }

    [Fact]
    public void AspectRatio_ReturnsOneWhenHeightIsZero()
    {
        Viewport vp = new(0, 0, 800, 0);

        Assert.Equal(1f, vp.AspectRatio);
    }
}
