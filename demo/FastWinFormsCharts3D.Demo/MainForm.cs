// Copyright (c) 2026 MabinogiCode. All rights reserved.

using FastWinFormsCharts3D.Charts.Scatter;
using FastWinFormsCharts3D.Controls;
using FastWinFormsCharts3D.DataModels;
using System.Drawing;
using System.Windows.Forms;

namespace FastWinFormsCharts3D.Demo;

/// <summary>
/// Main demo window. Demonstrates a basic <see cref="ScatterChart3D"/> with random data.
/// Full interactivity (orbit, zoom, pan) is available once v0.2 is complete.
/// </summary>
public partial class MainForm : Form
{
    private Chart3DControl _chartControl = null!;

    /// <summary>Initializes a new instance of <see cref="MainForm"/>.</summary>
    public MainForm()
    {
        InitializeComponent();
        SetupChart();
    }

    private void SetupChart()
    {
        ScatterChart3D scatter = new() { Title = "Random Scatter — 10 000 points" };
        scatter.AddSeries(new DataSeries3D("Series 1", GeneratePoints(10_000, 42))
        {
            Color = Color.DodgerBlue,
            MarkerSize = 4f,
        });
        scatter.AddSeries(new DataSeries3D("Series 2", GeneratePoints(5_000, 99))
        {
            Color = Color.OrangeRed,
            MarkerSize = 5f,
        });

        _chartControl.Chart = scatter;
    }

    private static IEnumerable<DataPoint3D> GeneratePoints(int count, int seed)
    {
        Random rng = new(seed);
        for (int i = 0; i < count; i++)
        {
            yield return new DataPoint3D(
                (float)(rng.NextDouble() * 2 - 1),
                (float)(rng.NextDouble() * 2 - 1),
                (float)(rng.NextDouble() * 2 - 1));
        }
    }
}
