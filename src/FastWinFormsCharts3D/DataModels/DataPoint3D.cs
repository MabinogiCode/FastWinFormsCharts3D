// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Numerics;

namespace FastWinFormsCharts3D.DataModels;

/// <summary>
/// Represents an immutable point in 3D space.
/// Using <c>readonly record struct</c> ensures zero heap allocation per point
/// and makes the type directly pinnable for GPU buffer uploads.
/// </summary>
/// <param name="X">The X coordinate.</param>
/// <param name="Y">The Y coordinate.</param>
/// <param name="Z">The Z coordinate.</param>
public readonly record struct DataPoint3D(float X, float Y, float Z)
{
    /// <summary>Creates a <see cref="DataPoint3D"/> from a <see cref="Vector3"/>.</summary>
    /// <param name="v">The source vector.</param>
    /// <returns>A new <see cref="DataPoint3D"/> with the same component values.</returns>
    public static DataPoint3D FromVector3(Vector3 v) => new(v.X, v.Y, v.Z);

    /// <summary>Converts this point to a <see cref="Vector3"/>.</summary>
    /// <returns>A <see cref="Vector3"/> with the same component values.</returns>
    public Vector3 ToVector3() => new(X, Y, Z);
}
