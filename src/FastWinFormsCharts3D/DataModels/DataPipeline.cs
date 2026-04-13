// Copyright (c) 2026 MabinogiCode. All rights reserved.

namespace FastWinFormsCharts3D.DataModels;

/// <summary>
/// Utility for computing new chart data on a background thread and delivering the result
/// back to the UI thread via the ambient <see cref="SynchronizationContext"/>.
/// </summary>
/// <remarks>
/// <para>Pattern:</para>
/// <list type="number">
///   <item>Call <c>PostAsync</c> on the UI thread — the current
///     <see cref="SynchronizationContext"/> is captured at that point.</item>
///   <item>The supplied <paramref name="compute"/> delegate runs on a <see cref="Task.Run"/>
///     thread pool thread (no UI-thread overhead during heavy data generation).</item>
///   <item>When the computation finishes, the result is marshalled back via
///     <see cref="SynchronizationContext.Post"/> so that the chart data update happens
///     on the UI thread (safe for GPU buffer re-uploads).</item>
/// </list>
/// <para>
/// If there is no ambient <see cref="SynchronizationContext"/> (e.g., in unit tests or
/// benchmarks), the update is applied synchronously on the thread-pool thread.
/// </para>
/// </remarks>
public static class DataPipeline
{
    /// <summary>
    /// Computes a new array of <see cref="DataPoint3D"/> on a background thread,
    /// then calls <see cref="DataSeries3D.SetPoints"/> on the captured UI context.
    /// </summary>
    /// <param name="series">The target series whose points will be replaced.</param>
    /// <param name="compute">
    /// A pure function that generates the replacement point array.
    /// Must be thread-safe; do not capture UI-thread state.
    /// </param>
    /// <returns>A <see cref="Task"/> that completes after the update has been posted.</returns>
    public static async Task PostAsync(DataSeries3D series, Func<DataPoint3D[]> compute)
    {
        // Capture before the first await so we hold the UI-thread context.
        SynchronizationContext? ctx = SynchronizationContext.Current;

        DataPoint3D[] result = await Task.Run(compute).ConfigureAwait(false);

        if (ctx is not null)
        {
            ctx.Post(_ => series.SetPoints(result), null);
        }
        else
        {
            series.SetPoints(result);
        }
    }

    /// <summary>
    /// Computes a new heightmap on a background thread,
    /// then calls <see cref="SurfaceData.SetHeights"/> on the captured UI context.
    /// </summary>
    /// <param name="surfaceData">The target surface whose heightmap will be replaced.</param>
    /// <param name="compute">
    /// A pure function that generates the replacement heightmap.
    /// Must be thread-safe; do not capture UI-thread state.
    /// </param>
    /// <returns>A <see cref="Task"/> that completes after the update has been posted.</returns>
    public static async Task PostAsync(SurfaceData surfaceData, Func<float[,]> compute)
    {
        SynchronizationContext? ctx = SynchronizationContext.Current;

        float[,] result = await Task.Run(compute).ConfigureAwait(false);

        if (ctx is not null)
        {
            ctx.Post(_ => surfaceData.SetHeights(result), null);
        }
        else
        {
            surfaceData.SetHeights(result);
        }
    }
}
