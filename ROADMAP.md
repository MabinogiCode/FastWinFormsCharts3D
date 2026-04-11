# Roadmap

## v0.1 — Project Setup + Base Architecture *(current)*

- [x] Repository structure, solution file, folder layout
- [x] Root config files: `.editorconfig`, `stylecop.json`, `global.json`, `.gitignore`, `.gitattributes`
- [x] `Directory.Build.props` — `TreatWarningsAsErrors`, `Nullable`, `ImplicitUsings`, analyzers
- [x] `Directory.Packages.props` — Central Package Management
- [x] Analyzer toolchain: `Microsoft.CodeAnalysis.NetAnalyzers`, `StyleCop.Analyzers`, `Roslynator`, `SonarAnalyzer.CSharp`
- [x] SourceLink + embedded PDB for NuGet debugging
- [x] Namespace skeleton: `Controls`, `Charts`, `Rendering`, `Camera`, `DataModels`, `Exceptions`
- [x] `DataPoint3D` (`readonly record struct`), `DataSeries3D`
- [x] `Camera3D` orbital model, `Viewport`, `Projection`
- [x] `IChart3D` interface, `IRenderer` interface
- [x] `ScatterChart3D`, `ShaderProgram`, `VertexBuffer`, `VertexArrayObject` stubs
- [x] xUnit test project skeleton
- [x] BenchmarkDotNet project skeleton
- [x] Demo WinForms app skeleton
- [x] GitHub Actions CI/CD workflows (ci, release, benchmarks)

## v0.2 — OpenGL Context in WinForms + Basic Rendering Pipeline *(done)*

- [x] `OpenGLControl` — WGL context via `NativeMethods` P/Invoke (`gdi32`, `opengl32`, `user32`)
- [x] `WglNativeContext` — Silk.NET `INativeContext` backed by `wglGetProcAddress` + `GetProcAddress` fallback
- [x] `PixelFormatDescriptor` — Win32 struct (32-bit RGBA, 24-bit depth, 8-bit stencil, double-buffer)
- [x] SwapBuffers, `glViewport` on resize, `Application.Idle` continuous render loop
- [x] `glClear` (color + depth) each frame, `GL_PROGRAM_POINT_SIZE` enabled
- [x] `ShaderProgram` — compile + link GLSL from embedded `Assembly.GetManifestResourceStream`
- [x] `VertexBuffer` + `VertexArrayObject` — full GPU buffer lifecycle (create, bind, upload, dispose)
- [x] `Renderer` — immediate-mode draw-call submission via `glDrawArrays`
- [x] `Camera3D` — orbital `ViewMatrix` (`Matrix4x4.CreateLookAt`) + `Projection.Perspective`
- [x] `Chart3DControl` — left-drag orbit, scroll zoom, right-drag pan, `IChart3D` lifecycle management

## v0.3 — Scatter 3D Chart *(done)*

- [x] `ScatterChart3D.Initialize` — VBO upload from `DataSeries3D`
- [x] `scatter.vert` / `scatter.frag` — `gl_PointSize`, per-series color uniform
- [x] MVP uniform upload per frame (`System.Numerics` row-major → `transpose=true` fix)
- [x] `DataSeries3D.DataChanged` → incremental VBO update (free + re-upload)
- [x] Axis lines (X/Y/Z) — red/green/blue color-coded, rendered via shared shader
- [x] Demo app: animated scatter (sin/cos spiral, 100 k points, ~60 fps `Timer`)
- [ ] Axis labels (WinForms GDI+ overlay via `OnPaint`) — deferred to v0.8
- [ ] Legend overlay — deferred to v0.8
- [ ] Benchmark: 1 000 000 points, measure sustained FPS — deferred to v0.7

## v0.4 — Surface Chart 3D *(done)*

- [x] `SurfaceChart3D : IChart3D`
- [x] Grid mesh generation from `float[rows, cols]` heightmap (VBO + EBO, indexed triangle list)
- [x] `SurfaceData` — rows × cols heightmap with world-space extents + `DataChanged` event
- [x] Viridis colour mapping — degree-6 polynomial in `surface.frag`, range driven by `uYMin`/`uYMax`
- [x] `surface.vert` / `surface.frag` — embedded GLSL, height-to-Viridis pipeline
- [x] Demo: ripple heightmap (sin(dist × 15) × 0.25, 100 × 100 grid) — Surface tab in demo app

## v0.5 — Bar Chart 3D

- [ ] `BarChart3D : IChart3D`
- [ ] Instanced rendering (`glDrawArraysInstanced`) for performance
- [ ] Per-instance color + height buffer

## v0.6 — Line Chart 3D

- [ ] `LineChart3D : IChart3D`
- [ ] Tube geometry for thick lines
- [ ] Dashed line shader variant

## v0.7 — Performance Pass

- [ ] Frustum culling (discard points outside camera view)
- [ ] LOD for scatter (reduce point count at distance)
- [ ] Compute shaders for large dataset transforms (`Silk.NET.OpenGL` compute)
- [ ] Async data loading pipeline (background thread → GPU upload on UI thread)
- [ ] Memory-mapped data source for streaming datasets

## v0.8 — UX Polish

- [ ] Tooltip on hover (ray-cast from mouse → nearest point in 3D)
- [ ] Click-to-select data point — fires `PointSelected` event
- [ ] Configurable axes: labels, tick marks, grid planes
- [ ] Theme system: Dark / Light / Custom (`IChartTheme`)
- [ ] Full WinForms Designer support (`TypeConverter`, `UITypeEditor`, `DesignerVerb`)
- [ ] Smooth camera animations (lerp/slerp transitions)

## v0.9 — Documentation + Samples

- [ ] Full XML doc on all public API members (enforced by `CS1591`)
- [ ] DocFX site published to GitHub Pages
- [ ] 10 demo scenarios covering all chart types
- [ ] Contributing guide + code of conduct

## v1.0 — Release

- [ ] API freeze + semantic versioning commitment
- [ ] ≥ 90 % code coverage (enforced in CI)
- [ ] Benchmarks published in docs
- [ ] Published to **NuGet.org**
- [ ] GitHub Release with `CHANGELOG.md`
- [ ] Icon + full NuGet metadata (description, tags, screenshots)
