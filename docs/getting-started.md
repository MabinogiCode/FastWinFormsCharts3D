# Getting Started

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10 / 11
- GPU driver with OpenGL 3.3+ support

## Installation

```shell
dotnet add package FastWinFormsCharts3D
```

Or via the NuGet Package Manager in Visual Studio: search for `FastWinFormsCharts3D`.

## Minimal Example (code-only)

```csharp
using FastWinFormsCharts3D.Charts.Scatter;
using FastWinFormsCharts3D.Controls;
using FastWinFormsCharts3D.DataModels;
using System.Drawing;

var form = new Form { Text = "My 3D Chart", Size = new Size(1024, 768) };

var chartControl = new Chart3DControl { Dock = DockStyle.Fill };
form.Controls.Add(chartControl);

var scatter = new ScatterChart3D { Title = "My Scatter" };
scatter.AddSeries(new DataSeries3D("Series 1", GeneratePoints())
{
    Color = Color.DodgerBlue,
    MarkerSize = 4f,
});

chartControl.Chart = scatter;

Application.Run(form);

static IEnumerable<DataPoint3D> GeneratePoints()
{
    var rng = new Random(42);
    for (int i = 0; i < 10_000; i++)
        yield return new DataPoint3D(
            (float)(rng.NextDouble() * 2 - 1),
            (float)(rng.NextDouble() * 2 - 1),
            (float)(rng.NextDouble() * 2 - 1));
}
```

## Camera Interaction

| Gesture | Action |
|---|---|
| Left-drag | Orbit (rotate around target) |
| Mouse wheel | Zoom in / out |
| Right-drag | Pan target point |

## Adding a Second Series

```csharp
scatter.AddSeries(new DataSeries3D("Series 2", otherPoints)
{
    Color = Color.OrangeRed,
    MarkerSize = 6f,
});
```

## Dynamic Data Updates

```csharp
// Updating data triggers an automatic GPU buffer re-upload
series.SetPoints(newPoints);

// Or append one point at a time
series.AddPoint(new DataPoint3D(x, y, z));
```
