# Architecture

## Overview

FastWinFormsCharts3D is structured as a layered library with strict unidirectional dependencies. No layer depends on the layer above it.

```
┌──────────────────────────────────────────────┐
│         Controls  (WinForms surface)         │
│   OpenGLControl  ·  Chart3DControl           │
└──────────────┬───────────────────────────────┘
               │  uses
┌──────────────▼───────────────────────────────┐
│         Charts  (domain)                     │
│   IChart3D  ·  ScatterChart3D                │
└──────────┬──────────────────┬────────────────┘
           │  uses            │  uses
┌──────────▼────────┐  ┌──────▼───────────────┐
│  Rendering        │  │  Camera               │
│  IRenderer        │  │  Camera3D             │
│  Renderer         │  │  Viewport             │
│  ShaderProgram    │  │  Projection           │
│  VertexBuffer     │  └──────────────────────-┘
│  VertexArrayObject│         │  uses
└──────────┬────────┘  ┌──────▼───────────────┐
           │           │  DataModels           │
           │           │  DataPoint3D          │
           │           │  DataSeries3D         │
           │           └──────────────────────-┘
           │
    Silk.NET.OpenGL          System.Numerics
```

## Namespaces

| Namespace | Responsibility | External dependencies |
|---|---|---|
| `FastWinFormsCharts3D.Controls` | WinForms UserControls, WGL context lifecycle | Silk.NET.OpenGL (via base) |
| `FastWinFormsCharts3D.Charts.Abstractions` | `IChart3D` contract | Silk.NET.OpenGL, Camera |
| `FastWinFormsCharts3D.Charts.Scatter` | Scatter 3D implementation | Rendering, Camera, DataModels |
| `FastWinFormsCharts3D.Rendering` | GPU buffer management, shader program | Silk.NET.OpenGL only |
| `FastWinFormsCharts3D.Camera` | Orbital camera, projection matrices | System.Numerics only |
| `FastWinFormsCharts3D.DataModels` | Value types for chart data | System.Numerics, System.Drawing |
| `FastWinFormsCharts3D.Exceptions` | Domain-specific exceptions | BCL only |

## Key Design Decisions

### `DataPoint3D` as `readonly record struct`

Zero heap allocation per point. The layout is `float X, float Y, float Z` = 12 bytes, directly pinnable for `glBufferData` uploads via `fixed (DataPoint3D* ptr = array)`.

### Shaders as Embedded Resources

GLSL files (`*.vert`, `*.frag`) are compiled into the assembly as `EmbeddedResource`. This makes the NuGet package self-contained — no file system dependency at runtime. `ShaderProgram.FromEmbeddedResource` loads them via `Assembly.GetManifestResourceStream`.

### WGL Context Strategy (v0.2)

`OpenGLControl` creates a Win32 WGL context from the control's HWND using `Silk.NET.OpenGL.Extensions.WGL`. This avoids any dependency on OpenTK while keeping Silk.NET as the sole 3D backend.

### Orbital Camera

`Camera3D` uses the azimuth/elevation/radius model matching Plotly's 3D UX. All math uses `System.Numerics.Matrix4x4` (BCL, no extra dependency). The conversion to Silk.NET math types for uniform upload is done at the shader boundary only.

## Adding a New Chart Type

1. Create `Charts/YourChart/YourChart3D.cs` implementing `IChart3D`.
2. Add vertex/fragment shaders under `Rendering/Shaders/` as `EmbeddedResource`.
3. Assign `chart3DControl.Chart = new YourChart3D()` — the control handles the lifecycle.
