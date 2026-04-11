// Copyright (c) 2026 MabinogiCode. All rights reserved.

using FastWinFormsCharts3D.Charts.Scatter;
using FastWinFormsCharts3D.Controls;
using FastWinFormsCharts3D.DataModels;
using System.Drawing;
using System.Windows.Forms;

namespace FastWinFormsCharts3D.Demo;

/// <summary>
/// Main demo window. Hosts a <see cref="ScatterChart3D"/> with an animated sin/cos spiral
/// (100 000 points) spinning in real-time, plus a static random background cloud.
/// </summary>
public partial class MainForm : Form
{
    private Chart3DControl _chartControl = null!;
    private DataSeries3D? _spiralSeries;
    private float _animTime;

    /// <summary>Initializes a new instance of <see cref="MainForm"/>.</summary>
    public MainForm()
    {
        InitializeComponent();
        SetupChart();
    }

    private void SetupChart()
    {
        ScatterChart3D scatter = new() { Title = "Scatter 3D — Animated Spiral (100 k points)" };

        // Static background: random point cloud for depth reference.
        scatter.AddSeries(new DataSeries3D("Background", GenerateRandomPoints(20_000, 42))
        {
            Color = Color.FromArgb(180, 80, 140, 255),
            MarkerSize = 2f,
        });

        // Animated foreground: 100 000-point helical spiral driven by the application timer.
        _spiralSeries = new DataSeries3D("Spiral", GenerateSpiralPoints(100_000, 0f))
        {
            Color = Color.FromArgb(255, 60, 220, 110),
            MarkerSize = 3f,
        };
        scatter.AddSeries(_spiralSeries);

        _chartControl.Chart = scatter;
        _animTimer.Start();
    }

    private void OnAnimTimerTick(object? sender, EventArgs e)
    {
        _animTime += 0.025f;
        _spiralSeries?.SetPoints(GenerateSpiralPoints(100_000, _animTime));
    }

    private static DataPoint3D[] GenerateSpiralPoints(int count, float time)
    {
        const float turns = 8f;
        DataPoint3D[] pts = new DataPoint3D[count];

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float angle = (t * turns * MathF.Tau) + time;
            float radius = t;

            pts[i] = new DataPoint3D(
                radius * MathF.Cos(angle),
                (t * 2f) - 1f,
                radius * MathF.Sin(angle));
        }

        return pts;
    }

    private static IEnumerable<DataPoint3D> GenerateRandomPoints(int count, int seed)
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
