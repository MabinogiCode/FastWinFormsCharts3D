// Copyright (c) 2026 MabinogiCode. All rights reserved.

namespace FastWinFormsCharts3D.DataModels;

/// <summary>
/// Holds a 2-D heightmap for use with a surface chart.
/// The grid spans [<see cref="XMin"/>, <see cref="XMax"/>] in X
/// and [<see cref="ZMin"/>, <see cref="ZMax"/>] in Z.
/// </summary>
public sealed class SurfaceData
{
    private float[,] _heights;

    /// <summary>
    /// Initialises a new <see cref="SurfaceData"/> with the given heightmap and world-space extents.
    /// </summary>
    /// <param name="heights">A <c>rows × cols</c> array of Y values.</param>
    /// <param name="xMin">Left boundary in world space.</param>
    /// <param name="xMax">Right boundary in world space.</param>
    /// <param name="zMin">Front boundary in world space.</param>
    /// <param name="zMax">Back boundary in world space.</param>
    public SurfaceData(float[,] heights, float xMin = -1f, float xMax = 1f, float zMin = -1f, float zMax = 1f)
    {
        _heights = heights;
        XMin = xMin;
        XMax = xMax;
        ZMin = zMin;
        ZMax = zMax;
    }

    /// <summary>Gets the number of rows in the heightmap.</summary>
    public int Rows => _heights.GetLength(0);

    /// <summary>Gets the number of columns in the heightmap.</summary>
    public int Cols => _heights.GetLength(1);

    /// <summary>Gets the left world-space boundary.</summary>
    public float XMin { get; init; }

    /// <summary>Gets the right world-space boundary.</summary>
    public float XMax { get; init; }

    /// <summary>Gets the front world-space boundary.</summary>
    public float ZMin { get; init; }

    /// <summary>Gets the back world-space boundary.</summary>
    public float ZMax { get; init; }

    /// <summary>Gets the height at the given grid cell.</summary>
    /// <param name="row">Row index (0 … <see cref="Rows"/> − 1).</param>
    /// <param name="col">Column index (0 … <see cref="Cols"/> − 1).</param>
    public float this[int row, int col] => _heights[row, col];

    /// <summary>Raised whenever the height data changes, triggering a GPU mesh rebuild.</summary>
    public event EventHandler? DataChanged;

    /// <summary>
    /// Replaces the heightmap and raises <see cref="DataChanged"/>.
    /// </summary>
    /// <param name="heights">The replacement heightmap; must have the same row/column counts.</param>
    /// <exception cref="ArgumentException">Thrown when the new array dimensions differ.</exception>
    public void SetHeights(float[,] heights)
    {
        if (heights.GetLength(0) != Rows || heights.GetLength(1) != Cols)
        {
            throw new ArgumentException(
                $"New heightmap must be {Rows}×{Cols}, but was {heights.GetLength(0)}×{heights.GetLength(1)}.",
                nameof(heights));
        }

        _heights = heights;
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
}
