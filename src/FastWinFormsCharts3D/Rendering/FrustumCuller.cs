// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Numerics;

namespace FastWinFormsCharts3D.Rendering;

/// <summary>
/// Conservative axis-aligned bounding box vs. view-frustum visibility test performed in clip space.
/// </summary>
internal static class FrustumCuller
{
    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="box"/> is at least partially inside the view frustum.
    /// </summary>
    /// <remarks>
    /// <para>Algorithm: transform all 8 AABB corners to clip space via <paramref name="mvp"/>.
    /// A box is provably invisible only when every corner lies beyond the same clip-space half-plane.
    /// The test is conservative — it never culls a visible box, but may retain a small number of
    /// boxes that straddle frustum edges.</para>
    /// <para>
    /// The clip-space planes in homogeneous coordinates are:
    /// <list type="bullet">
    ///   <item>Left:   <c>x ≥ −w</c></item>
    ///   <item>Right:  <c>x ≤  w</c></item>
    ///   <item>Bottom: <c>y ≥ −w</c></item>
    ///   <item>Top:    <c>y ≤  w</c></item>
    ///   <item>Near:   <c>z ≥ −w</c></item>
    ///   <item>Far:    <c>z ≤  w</c></item>
    /// </list>
    /// <c>System.Numerics.Vector4.Transform</c> uses row-vector × matrix convention, which matches
    /// the row-major MVP uploaded to GLSL with <c>transpose=true</c>.
    /// </para>
    /// </remarks>
    /// <param name="box">World-space axis-aligned bounding box.</param>
    /// <param name="mvp">Combined model-view-projection matrix (row-major, System.Numerics convention).</param>
    /// <returns>
    /// <see langword="false"/> when the box is provably outside the frustum; otherwise <see langword="true"/>.
    /// </returns>
    internal static bool IsVisible(in BoundingBox3D box, Matrix4x4 mvp)
    {
        // Transform all 8 AABB corners to clip space.
        Span<Vector4> c = stackalloc Vector4[8];
        c[0] = Vector4.Transform(new Vector4(box.MinX, box.MinY, box.MinZ, 1f), mvp);
        c[1] = Vector4.Transform(new Vector4(box.MaxX, box.MinY, box.MinZ, 1f), mvp);
        c[2] = Vector4.Transform(new Vector4(box.MinX, box.MaxY, box.MinZ, 1f), mvp);
        c[3] = Vector4.Transform(new Vector4(box.MaxX, box.MaxY, box.MinZ, 1f), mvp);
        c[4] = Vector4.Transform(new Vector4(box.MinX, box.MinY, box.MaxZ, 1f), mvp);
        c[5] = Vector4.Transform(new Vector4(box.MaxX, box.MinY, box.MaxZ, 1f), mvp);
        c[6] = Vector4.Transform(new Vector4(box.MinX, box.MaxY, box.MaxZ, 1f), mvp);
        c[7] = Vector4.Transform(new Vector4(box.MaxX, box.MaxY, box.MaxZ, 1f), mvp);

        // Per clip-plane: track whether ALL corners are outside that plane.
        // Initialise to true; clear when a corner is found inside the plane.
        bool outL = true, outR = true, outB = true, outT = true, outN = true, outF = true;

        for (int i = 0; i < 8; i++)
        {
            Vector4 v = c[i];

            if (v.X >= -v.W)
            {
                outL = false;
            }

            if (v.X <= v.W)
            {
                outR = false;
            }

            if (v.Y >= -v.W)
            {
                outB = false;
            }

            if (v.Y <= v.W)
            {
                outT = false;
            }

            if (v.Z >= -v.W)
            {
                outN = false;
            }

            if (v.Z <= v.W)
            {
                outF = false;
            }
        }

        // If every corner is outside any single plane, the box is provably invisible.
        return !(outL || outR || outB || outT || outN || outF);
    }
}
