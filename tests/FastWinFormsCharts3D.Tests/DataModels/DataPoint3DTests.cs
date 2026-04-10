// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Numerics;

namespace FastWinFormsCharts3D.Tests.DataModels;

/// <summary>Unit tests for <see cref="DataPoint3D"/>.</summary>
public sealed class DataPoint3DTests
{
    [Fact]
    public void Constructor_SetsComponents()
    {
        DataPoint3D point = new(1f, 2f, 3f);

        Assert.Equal(1f, point.X);
        Assert.Equal(2f, point.Y);
        Assert.Equal(3f, point.Z);
    }

    [Fact]
    public void FromVector3_RoundTrips()
    {
        Vector3 v = new(4f, 5f, 6f);
        DataPoint3D point = DataPoint3D.FromVector3(v);

        Assert.Equal(v, point.ToVector3());
    }

    [Fact]
    public void EqualityIsValueBased()
    {
        DataPoint3D a = new(1f, 2f, 3f);
        DataPoint3D b = new(1f, 2f, 3f);

        Assert.Equal(a, b);
    }
}
