# Contributing

## Requirements

- .NET 8 SDK
- Windows 10 / 11 (WinForms + WGL)
- Visual Studio 2022 17.8+ or Rider 2024+

## Build

```shell
git clone https://github.com/MabinogiCode/FastWinFormsCharts3D
cd FastWinFormsCharts3D
dotnet build FastWinFormsCharts3D.sln -c Release
```

The build **must produce zero warnings**. `TreatWarningsAsErrors = true` is enforced globally in `Directory.Build.props`.

## Tests

```shell
dotnet test --collect:"XPlat Code Coverage"
```

Tests that require an OpenGL context are skipped when no GPU is available (CI-safe).

## Code Style

Rules are enforced automatically by the compiler:

| Tool | What it enforces |
|---|---|
| `.editorconfig` | Naming, braces, var usage, file-scoped namespaces |
| `StyleCop.Analyzers` | Member ordering, documentation, using placement |
| `Roslynator.Analyzers` | Redundant code, simplifications |
| `SonarAnalyzer.CSharp` | Security, code smells |
| `Microsoft.CodeAnalysis.NetAnalyzers` | .NET API usage guidelines |

Key rules:
- Private fields: `_camelCase`
- All public/internal members must have XML doc comments
- `var` only when type is apparent
- File-scoped namespaces (`namespace Foo.Bar;`)
- Braces always required (no braceless `if`)

## Adding a New Chart Type

1. Create `src/FastWinFormsCharts3D/Charts/YourChart/YourChart3D.cs` implementing `IChart3D`.
2. Add GLSL shaders in `src/FastWinFormsCharts3D/Rendering/Shaders/` — they are automatically embedded as resources.
3. Add unit tests in `tests/FastWinFormsCharts3D.Tests/Charts/`.
4. Add a demo scenario in `demo/FastWinFormsCharts3D.Demo/`.
5. Update `ROADMAP.md` to mark the milestone done.

## Pull Request Checklist

- [ ] `dotnet build -c Release` → 0 errors, 0 warnings
- [ ] `dotnet test` → all green
- [ ] New public API has XML doc comments
- [ ] New feature has unit tests
- [ ] `ROADMAP.md` updated if applicable
