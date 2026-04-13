// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Drawing;

namespace FastWinFormsCharts3D.DataModels;

/// <summary>
/// Represents a named collection of <see cref="DataPoint3D"/> values to be rendered as a single series.
/// </summary>
public sealed class DataSeries3D
{
    private readonly List<DataPoint3D> _points;

    /// <summary>Initializes a new instance of <see cref="DataSeries3D"/>.</summary>
    /// <param name="name">The unique name identifying this series.</param>
    /// <param name="points">The initial set of data points.</param>
    public DataSeries3D(string name, IEnumerable<DataPoint3D> points)
    {
        Name = name;
        _points = new List<DataPoint3D>(points);
        Color = Color.DodgerBlue;
        MarkerSize = 4f;
        IsVisible = true;
    }

    /// <summary>Gets the unique name of this series.</summary>
    public string Name { get; init; }

    /// <summary>Gets a read-only view of the data points in this series.</summary>
    public IReadOnlyList<DataPoint3D> Points => _points;

    /// <summary>Gets or sets the render color of this series.</summary>
    public Color Color { get; set; }

    /// <summary>Gets or sets the point marker size in pixels.</summary>
    public float MarkerSize { get; set; }

    /// <summary>Gets or sets a value indicating whether this series is rendered.</summary>
    public bool IsVisible { get; set; }

    /// <summary>Raised whenever the point data changes, triggering a GPU buffer update.</summary>
    public event EventHandler? DataChanged;

    /// <summary>Appends a single point and raises <see cref="DataChanged"/>.</summary>
    /// <param name="point">The point to append.</param>
    public void AddPoint(DataPoint3D point)
    {
        _points.Add(point);
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Replaces all points with a new array and raises <see cref="DataChanged"/>.
    /// Prefer this over repeated <see cref="AddPoint"/> calls for bulk updates.
    /// </summary>
    /// <param name="points">The replacement array.</param>
    public void SetPoints(DataPoint3D[] points)
    {
        _points.Clear();
        _points.AddRange(points);
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
}
