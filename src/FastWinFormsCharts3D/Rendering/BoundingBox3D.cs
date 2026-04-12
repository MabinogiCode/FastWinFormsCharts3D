// Copyright (c) 2026 MabinogiCode. All rights reserved.

namespace FastWinFormsCharts3D.Rendering;

/// <summary>
/// Axis-aligned bounding box (AABB) in 3D world space.
/// Stored alongside each series' GPU buffer and tested each frame by
/// <see cref="FrustumCuller"/> to avoid submitting invisible draw calls.
/// </summary>
internal readonly record struct BoundingBox3D(
    float MinX,
    float MinY,
    float MinZ,
    float MaxX,
    float MaxY,
    float MaxZ);
