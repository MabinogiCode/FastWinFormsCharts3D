// Copyright (c) 2026 MabinogiCode. All rights reserved.

using FastWinFormsCharts3D.Charts.Bar;
using FastWinFormsCharts3D.Charts.Line;
using FastWinFormsCharts3D.Charts.Scatter;
using FastWinFormsCharts3D.Charts.Surface;
using FastWinFormsCharts3D.Controls;
using FastWinFormsCharts3D.DataModels;
using System.Drawing;
using System.Windows.Forms;

namespace FastWinFormsCharts3D.Demo;

/// <summary>
/// Main demo window. Shows four tabs:
/// <list type="bullet">
///   <item><b>Scatter 3D</b> — animated sin/cos spiral (100 000 points, ~60 fps).</item>
///   <item><b>Surface 3D</b> — ripple heightmap (100 × 100 grid, Viridis palette).</item>
///   <item><b>Bar 3D</b> — wave-pattern bar grid (12 × 12, instanced rendering).</item>
///   <item><b>Line 3D</b> — Lissajous tube (solid) + helix tube (dashed).</item>
/// </list>
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
        SetupScatter();
        SetupSurface();
        SetupBar();
        SetupLine();
    }

    // ── Scatter tab ───────────────────────────────────────────────────────────

    private void SetupScatter()
    {
        ScatterChart3D scatter = new() { Title = "Scatter 3D — Animated Spiral (100 k points)" };

        scatter.AddSeries(new DataSeries3D("Background", GenerateRandomPoints(20_000, 42))
        {
            Color = Color.FromArgb(180, 80, 140, 255),
            MarkerSize = 2f,
        });

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

    // ── Surface tab ───────────────────────────────────────────────────────────

    private void SetupSurface()
    {
        const int size = 100;
        float[,] heights = BuildRippleHeightmap(size, size);

        SurfaceChart3D surface = new(new SurfaceData(heights))
        {
            Title = "Surface 3D — Ripple (100 × 100)",
        };

        _surfaceControl.Chart = surface;
    }

    private static float[,] BuildRippleHeightmap(int rows, int cols)
    {
        float[,] h = new float[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            float z = -1f + (2f * r / (rows - 1));

            for (int c = 0; c < cols; c++)
            {
                float x = -1f + (2f * c / (cols - 1));
                float dist = MathF.Sqrt((x * x) + (z * z));
                h[r, c] = MathF.Sin(dist * 15f) * 0.25f;
            }
        }

        return h;
    }

    // ── Bar tab ───────────────────────────────────────────────────────────────

    private void SetupBar()
    {
        const int rows = 12;
        const int cols = 12;
        float[,] values = BuildWaveGrid(rows, cols);

        BarChart3D bar = new(new BarSeries3D("Wave", values))
        {
            Title = "Bar 3D — Wave Grid (12 × 12, instanced)",
        };

        _barControl.Chart = bar;
    }

    private static float[,] BuildWaveGrid(int rows, int cols)
    {
        float[,] v = new float[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                v[r, c] = (MathF.Sin(r * 0.8f) * MathF.Cos(c * 0.8f) + 1f) * 0.5f;
            }
        }

        return v;
    }

    // ── Line tab ──────────────────────────────────────────────────────────────

    private void SetupLine()
    {
        LineChart3D line = new() { Title = "Line 3D — Lissajous (solid) + Helix (dashed)" };

        // Lissajous 3D knot: x=sin(3t+π/4), y=sin(2t), z=cos(t)
        line.AddSeries(new LineSeries3D("Lissajous", BuildLissajous(1_000))
        {
            Color = Color.FromArgb(255, 50, 210, 255),
            Radius = 0.018f,
            IsDashed = false,
        });

        // Helix with 4 turns — rendered dashed
        line.AddSeries(new LineSeries3D("Helix", BuildHelix(600, turns: 4))
        {
            Color = Color.FromArgb(255, 255, 160, 40),
            Radius = 0.022f,
            IsDashed = true,
            DashLength = 0.12f,
            GapLength = 0.06f,
        });

        _lineControl.Chart = line;
    }

    private static DataPoint3D[] BuildLissajous(int count)
    {
        DataPoint3D[] pts = new DataPoint3D[count];

        for (int i = 0; i < count; i++)
        {
            float t = i * MathF.Tau / (count - 1);
            pts[i] = new DataPoint3D(
                MathF.Sin((3 * t) + (MathF.PI / 4f)),
                MathF.Sin(2 * t),
                MathF.Cos(t));
        }

        return pts;
    }

    private static DataPoint3D[] BuildHelix(int count, int turns)
    {
        DataPoint3D[] pts = new DataPoint3D[count];

        for (int i = 0; i < count; i++)
        {
            float t = i * MathF.Tau * turns / (count - 1);
            pts[i] = new DataPoint3D(
                MathF.Cos(t) * 0.75f,
                ((float)i / (count - 1) * 2f) - 1f,
                MathF.Sin(t) * 0.75f);
        }

        return pts;
    }
}
