# FastWinFormsCharts3D

[![NuGet](https://img.shields.io/nuget/v/FastWinFormsCharts3D.svg)](https://www.nuget.org/packages/FastWinFormsCharts3D)
[![Build](https://github.com/MabinogiCode/FastWinFormsCharts3D/actions/workflows/ci.yml/badge.svg)](https://github.com/MabinogiCode/FastWinFormsCharts3D/actions/workflows/ci.yml)
[![Coverage](https://codecov.io/gh/MabinogiCode/FastWinFormsCharts3D/branch/main/graph/badge.svg)](https://codecov.io/gh/MabinogiCode/FastWinFormsCharts3D)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

High-performance 3D charting controls for WinForms, powered by **Silk.NET** and OpenGL. Drop a control onto your form and render millions of 3D data points at interactive framerates — as powerful as Helix Toolkit or Plotly, but native to Windows Forms and distributed as a single NuGet package.

---

## Features

- **Scatter 3D** — render millions of points with per-series color and marker size
- **Orbital camera** — drag to rotate, scroll to zoom, right-drag to pan (Plotly-style UX)
- **Embedded GLSL shaders** — no file dependencies, fully self-contained NuGet package
- **SourceLink** — step-through debugging directly from NuGet
- **WinForms Designer support** — drag & drop `Chart3DControl` from the toolbox
- **.NET 8 LTS** — modern C# 12, nullable reference types, file-scoped namespaces

**Roadmap:** Surface 3D, Bar Chart 3D, Line/Trajectory, instanced rendering, tooltips, themes. See [ROADMAP.md](ROADMAP.md).

---

## Requirements

- .NET 8 (Windows)
- Windows 10 / 11
- GPU driver with **OpenGL 3.3+** support

---

## Installation

```shell
dotnet add package FastWinFormsCharts3D
```

---

## Quick Start

### Via WinForms Designer

1. Build the project to register `Chart3DControl` in the VS toolbox.
2. Drag `Chart3DControl` onto your `Form`.
3. In code-behind:

```csharp
using FastWinFormsCharts3D.Charts.Scatter;
using FastWinFormsCharts3D.DataModels;
using System.Drawing;

// Generate sample data
var series = new DataSeries3D("MySeries", GeneratePoints())
{
    Color = Color.DodgerBlue,
    MarkerSize = 4f,
};

chart3DControl1.Chart = new ScatterChart3D();
((ScatterChart3D)chart3DControl1.Chart).AddSeries(series);

static IEnumerable<DataPoint3D> GeneratePoints()
{
    var rng = new Random(42);
    for (int i = 0; i < 10_000; i++)
    {
        yield return new DataPoint3D(
            (float)(rng.NextDouble() * 2 - 1),
            (float)(rng.NextDouble() * 2 - 1),
            (float)(rng.NextDouble() * 2 - 1));
    }
}
```

### Code-only (no Designer)

```csharp
using FastWinFormsCharts3D.Controls;
using FastWinFormsCharts3D.Charts.Scatter;
using FastWinFormsCharts3D.DataModels;
using System.Drawing;
using System.Windows.Forms;

var form = new Form { Text = "FastWinFormsCharts3D Demo", Size = new Size(1024, 768) };

var chart = new Chart3DControl { Dock = DockStyle.Fill };
chart.Chart = new ScatterChart3D();
((ScatterChart3D)chart.Chart).AddSeries(new DataSeries3D("Demo", [
    new(0f, 0f, 0f), new(1f, 0f, 0f), new(0f, 1f, 0f), new(0f, 0f, 1f)
]) { Color = Color.OrangeRed, MarkerSize = 6f });

form.Controls.Add(chart);
Application.Run(form);
```

---

## Architecture

See [docs/architecture.md](docs/architecture.md) for the full namespace and dependency graph.

```
Controls  →  Charts/Abstractions  →  Rendering (Silk.NET.OpenGL)
Controls  →  Camera
Charts/Scatter  →  Rendering + Camera + DataModels
DataModels  →  System.Numerics (no third-party dependencies)
Camera  →  System.Numerics (no third-party dependencies)
```

---

## Contributing

See [docs/contributing.md](docs/contributing.md). All contributions must pass:

```shell
dotnet build FastWinFormsCharts3D.sln -c Release   # 0 warnings (TreatWarningsAsErrors)
dotnet test                                          # all green
```

---

## License

MIT — Copyright (c) 2026 [MabinogiCode](https://github.com/MabinogiCode)
