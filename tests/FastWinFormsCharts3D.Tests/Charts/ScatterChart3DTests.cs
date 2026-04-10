// Copyright (c) 2026 MabinogiCode. All rights reserved.

namespace FastWinFormsCharts3D.Tests.Charts;

/// <summary>Unit tests for <see cref="ScatterChart3D"/>.</summary>
public sealed class ScatterChart3DTests
{
    [Fact]
    public void AddSeries_IncreasesSeriesCount()
    {
        ScatterChart3D chart = new();
        DataSeries3D series = new("S1", []);

        chart.AddSeries(series);

        Assert.Single(chart.Series);
    }

    [Fact]
    public void RemoveSeries_ByName_RemovesIt()
    {
        ScatterChart3D chart = new();
        chart.AddSeries(new DataSeries3D("A", []));
        chart.AddSeries(new DataSeries3D("B", []));

        chart.RemoveSeries("A");

        Assert.Single(chart.Series);
        Assert.Equal("B", chart.Series[0].Name);
    }

    [Fact]
    public void IsInitialized_FalseBeforeInitialize()
    {
        ScatterChart3D chart = new();

        Assert.False(chart.IsInitialized);
    }

    [Fact]
    public void Title_HasDefault()
    {
        ScatterChart3D chart = new();

        Assert.False(string.IsNullOrEmpty(chart.Title));
    }
}
