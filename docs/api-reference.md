# API Reference

> Full XML documentation is enforced via StyleCop and `GenerateDocumentationFile`.
> This page summarises the public surface. See IntelliSense for parameter details.

## Controls

### `Chart3DControl : OpenGLControl`

The primary WinForms control. Drop this on your form.

| Member | Description |
|---|---|
| `Chart3DControl.Chart` | Gets/sets the active `IChart3D`. Setting disposes the previous chart. |
| `Chart3DControl.Camera` | The orbital `Camera3D` instance. |

### `OpenGLControl : UserControl`

Base control that owns the WGL OpenGL context.

| Member | Description |
|---|---|
| `OpenGLControl.GL` | The active `Silk.NET.OpenGL.GL` API (null before handle creation). |
| `OpenGLControl.Render` | Event raised each frame with the active GL context. |

---

## Charts

### `IChart3D`

| Member | Description |
|---|---|
| `Initialize(GL gl)` | Called once after context creation. Compile shaders, upload initial GPU data. |
| `Render(GL gl, Camera3D camera)` | Called every frame. Must not allocate. |
| `Resize(int width, int height)` | Called on control resize. |
| `Title` | Human-readable label. |
| `IsInitialized` | True after `Initialize` succeeds. |

### `ScatterChart3D : IChart3D`

| Member | Description |
|---|---|
| `Series` | Read-only list of `DataSeries3D`. |
| `AddSeries(DataSeries3D)` | Registers a new series. |
| `RemoveSeries(string name)` | Removes a series by name. |

---

## DataModels

### `DataPoint3D` *(readonly record struct)*

```csharp
public readonly record struct DataPoint3D(float X, float Y, float Z)
```

| Member | Description |
|---|---|
| `FromVector3(Vector3)` | Factory from `System.Numerics.Vector3`. |
| `ToVector3()` | Converts to `System.Numerics.Vector3`. |

### `DataSeries3D`

| Member | Description |
|---|---|
| `Name` | Unique series identifier. |
| `Points` | Read-only list of `DataPoint3D`. |
| `Color` | `System.Drawing.Color` for rendering. |
| `MarkerSize` | Point size in pixels. |
| `IsVisible` | Whether the series is rendered. |
| `AddPoint(DataPoint3D)` | Appends a point and raises `DataChanged`. |
| `SetPoints(DataPoint3D[])` | Bulk replace, raises `DataChanged`. |
| `DataChanged` | Event raised on any mutation. |

---

## Camera

### `Camera3D`

Orbital (azimuth/elevation/radius) camera. Target = orbit centre.

| Member | Description |
|---|---|
| `Target` | The point the camera orbits around. |
| `Azimuth` | Horizontal angle (degrees). |
| `Elevation` | Vertical angle (degrees), clamped [-89, 89]. |
| `Radius` | Distance from camera to target. |
| `FieldOfView` | Vertical FOV (degrees). |
| `ViewMatrix` | Computed view matrix (`System.Numerics.Matrix4x4`). |
| `Orbit(float dAz, float dEl)` | Orbits by delta angles. |
| `Zoom(float delta)` | Adjusts radius. |
| `Pan(Vector2 delta)` | Pans target along camera XY plane. |

### `Projection` *(static)*

| Member | Description |
|---|---|
| `Perspective(fov, aspect, near, far)` | Perspective projection matrix. |
| `Orthographic(l, r, b, t, near, far)` | Orthographic projection matrix. |

### `Viewport` *(readonly record struct)*

| Member | Description |
|---|---|
| `AspectRatio` | `Width / Height`. Returns 1 when height is 0. |

---

## Exceptions

| Type | When thrown |
|---|---|
| `ShaderCompilationException` | GLSL compile/link failure. Message includes the driver info log. |
| `OpenGLContextException` | WGL context creation failure. `Win32ErrorCode` property available. |
