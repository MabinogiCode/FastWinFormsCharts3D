// Copyright (c) 2026 MabinogiCode. All rights reserved.

namespace FastWinFormsCharts3D.DataModels;

/// <summary>
/// Holds a 2-D grid of bar heights for a
/// <see cref="FastWinFormsCharts3D.Charts.Bar.BarChart3D"/>.
/// The grid is arranged as <c>rows × cols</c> bars spanning the world-space cube [−1,1]³.
/// </summary>
public sealed class BarSeries3D
{
    private float[,] _values;

    /// <summary>
    /// Initialises a new series with the given height grid.
    /// </summary>
    /// <param name="name">Unique name identifying this series.</param>
    /// <param name="values">A <c>rows × cols</c> array of bar heights (negative values render as zero).</param>
    public BarSeries3D(string name, float[,] values)
    {
        Name = name;
        _values = values;
    }

    /// <summary>Gets the unique name of this series.</summary>
    public string Name { get; init; }

    /// <summary>Gets the number of bar rows.</summary>
    public int Rows => _values.GetLength(0);

    /// <summary>Gets the number of bar columns.</summary>
    public int Cols => _values.GetLength(1);

    /// <summary>Gets the height of the bar at the given grid cell.</summary>
    /// <param name="row">Row index.</param>
    /// <param name="col">Column index.</param>
    public float this[int row, int col] => _values[row, col];

    /// <summary>
    /// Gets or sets the fraction of each cell's width occupied by the bar (0 &lt; value ≤ 1).
    /// Default is <c>0.8</c>.
    /// </summary>
    public float BarWidthFraction { get; set; } = 0.8f;

    /// <summary>
    /// Gets or sets the fraction of each cell's depth occupied by the bar (0 &lt; value ≤ 1).
    /// Default is <c>0.8</c>.
    /// </summary>
    public float BarDepthFraction { get; set; } = 0.8f;

    /// <summary>Raised whenever the height data changes, triggering a GPU instance-buffer rebuild.</summary>
    public event EventHandler? DataChanged;

    /// <summary>
    /// Replaces the height grid and raises <see cref="DataChanged"/>.
    /// The new array may have different dimensions; all GPU buffers are rebuilt.
    /// </summary>
    /// <param name="values">The replacement height grid.</param>
    public void SetValues(float[,] values)
    {
        _values = values;
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
}
