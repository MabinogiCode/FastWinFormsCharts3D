// Copyright (c) 2026 MabinogiCode. All rights reserved.

using FastWinFormsCharts3D.Camera;
using FastWinFormsCharts3D.Charts.Abstractions;

namespace FastWinFormsCharts3D.Controls;

/// <summary>
/// A <see cref="UserControl"/> that bridges the WinForms designer and the 3D chart domain.
/// Hosts any <see cref="IChart3D"/> implementation and handles mouse-driven camera interaction.
/// </summary>
/// <remarks>
/// Interaction model (implemented in v0.2):
/// <list type="bullet">
///   <item>Left-drag → orbit (azimuth + elevation)</item>
///   <item>Mouse wheel → zoom</item>
///   <item>Right-drag → pan target</item>
/// </list>
/// </remarks>
public class Chart3DControl : OpenGLControl
{
    private IChart3D? _chart;
    private Point _lastMousePosition;
    private bool _isOrbiting;
    private bool _isPanning;

    /// <summary>Gets the orbital camera used by this control.</summary>
    public Camera3D Camera { get; } = new Camera3D();

    /// <summary>
    /// Gets or sets the 3D chart rendered by this control.
    /// Setting a new chart disposes the previous one and triggers re-initialization
    /// on the next frame.
    /// </summary>
    public IChart3D? Chart
    {
        get => _chart;
        set
        {
            if (ReferenceEquals(_chart, value))
            {
                return;
            }

            _chart?.Dispose();
            _chart = value;
            Invalidate();
        }
    }

    /// <inheritdoc />
    protected override void OnRender(RenderEventArgs e)
    {
        if (_chart is null)
        {
            return;
        }

        if (!_chart.IsInitialized)
        {
            _chart.Initialize(e.GL);
        }

        _chart.Render(e.GL, Camera);
    }

    /// <inheritdoc />
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        _chart?.Resize(Width, Height);
    }

    /// <inheritdoc />
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _lastMousePosition = e.Location;

        if (e.Button == MouseButtons.Left)
        {
            _isOrbiting = true;
        }
        else if (e.Button == MouseButtons.Right)
        {
            _isPanning = true;
        }
    }

    /// <inheritdoc />
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        int dx = e.X - _lastMousePosition.X;
        int dy = e.Y - _lastMousePosition.Y;

        if (_isOrbiting)
        {
            // TODO (v0.2): scale factors should be configurable.
            Camera.Orbit(dx * 0.5f, -dy * 0.5f);
            Invalidate();
        }
        else if (_isPanning)
        {
            Camera.Pan(new System.Numerics.Vector2(dx, dy));
            Invalidate();
        }

        _lastMousePosition = e.Location;
    }

    /// <inheritdoc />
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _isOrbiting = false;
        _isPanning = false;
    }

    /// <inheritdoc />
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        Camera.Zoom(e.Delta * 0.01f);
        Invalidate();
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _chart?.Dispose();
        }

        base.Dispose(disposing);
    }
}
