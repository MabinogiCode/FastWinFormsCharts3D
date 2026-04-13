// Copyright (c) 2026 MabinogiCode. All rights reserved.

namespace FastWinFormsCharts3D.Tests.DataModels;

/// <summary>Unit tests for <see cref="DataSeries3D"/>.</summary>
public sealed class DataSeries3DTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        DataSeries3D series = new("Test", [new(0, 0, 0)]);

        Assert.Equal("Test", series.Name);
    }

    [Fact]
    public void AddPoint_IncreasesCount()
    {
        DataSeries3D series = new("S", []);
        series.AddPoint(new DataPoint3D(1, 2, 3));

        Assert.Single(series.Points);
    }

    [Fact]
    public void AddPoint_RaisesDataChanged()
    {
        DataSeries3D series = new("S", []);
        bool raised = false;
        series.DataChanged += (_, _) => raised = true;

        series.AddPoint(new DataPoint3D(0, 0, 0));

        Assert.True(raised);
    }

    [Fact]
    public void SetPoints_ReplacesAll()
    {
        DataSeries3D series = new("S", [new(1, 1, 1), new(2, 2, 2)]);
        DataPoint3D[] replacement = [new(9, 9, 9)];

        series.SetPoints(replacement);

        Assert.Single(series.Points);
        Assert.Equal(new DataPoint3D(9, 9, 9), series.Points[0]);
    }
}
