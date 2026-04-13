// Copyright (c) 2026 MabinogiCode. All rights reserved.

using FastWinFormsCharts3D.Camera;
using Silk.NET.OpenGL;

namespace FastWinFormsCharts3D.Charts.Abstractions;

/// <summary>
/// Defines the contract for a 3D chart that can be hosted inside a <c>Chart3DControl</c>.
/// </summary>
public interface IChart3D : IDisposable
{
    /// <summary>
    /// Called once after the OpenGL context is created and made current.
    /// Use this to compile shaders and upload initial GPU resources.
    /// </summary>
    /// <param name="gl">The active OpenGL API.</param>
    void Initialize(GL gl);

    /// <summary>
    /// Called every frame. Must not allocate managed memory.
    /// </summary>
    /// <param name="gl">The active OpenGL API.</param>
    /// <param name="camera">The current camera state for MVP computation.</param>
    void Render(GL gl, Camera3D camera);

    /// <summary>Called when the control is resized.</summary>
    /// <param name="width">The new width in pixels.</param>
    /// <param name="height">The new height in pixels.</param>
    void Resize(int width, int height);

    /// <summary>Gets or sets the human-readable title displayed as a chart overlay.</summary>
    string Title { get; set; }

    /// <summary>Gets a value indicating whether <see cref="Initialize"/> has been called successfully.</summary>
    bool IsInitialized { get; }
}
