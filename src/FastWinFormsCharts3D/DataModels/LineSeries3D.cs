// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Drawing;

namespace FastWinFormsCharts3D.DataModels;

/// <summary>
/// Represents a named polyline to be rendered as a 3D tube by a
/// <see cref="FastWinFormsCharts3D.Charts.Line.LineChart3D"/>.
/// </summary>
public sealed class LineSeries3D
{
    private DataPoint3D[] _points;

    /// <summary>Initialises a new series with the given point sequence.</summary>
    /// <param name="name">Unique name identifying this series.</param>
    /// <param name="points">The ordered vertices of the polyline.</param>
    public LineSeries3D(string name, DataPoint3D[] points)
    {
        Name = name;
        _points = points;
    }

    /// <summary>Gets the unique name of this series.</summary>
    public string Name { get; init; }

    /// <summary>Gets a read-only view of the polyline vertices.</summary>
    public ReadOnlySpan<DataPoint3D> Points => _points;

    /// <summary>Gets or sets the tube render colour.</summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets the tube radius in world units.
    /// Default is <c>0.02</c>.
    /// </summary>
    public float Radius { get; set; } = 0.02f;

    /// <summary>Gets or sets a value indicating whether the line is rendered dashed.</summary>
    public bool IsDashed { get; set; }

    /// <summary>
    /// Gets or sets the filled segment length in world units when <see cref="IsDashed"/> is <see langword="true"/>.
    /// Default is <c>0.1</c>.
    /// </summary>
    public float DashLength { get; set; } = 0.1f;

    /// <summary>
    /// Gets or sets the gap length in world units when <see cref="IsDashed"/> is <see langword="true"/>.
    /// Default is <c>0.05</c>.
    /// </summary>
    public float GapLength { get; set; } = 0.05f;

    /// <summary>Gets or sets a value indicating whether this series is rendered.</summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>Raised whenever the point data changes, triggering a GPU tube rebuild.</summary>
    public event EventHandler? DataChanged;

    /// <summary>
    /// Replaces all points and raises <see cref="DataChanged"/>.
    /// </summary>
    /// <param name="points">The replacement vertex sequence.</param>
    public void SetPoints(DataPoint3D[] points)
    {
        _points = points;
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
}
