// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Runtime.InteropServices;

namespace FastWinFormsCharts3D.Controls.Native;

/// <summary>
/// Mirrors the Win32 <c>PIXELFORMATDESCRIPTOR</c> structure used to describe
/// the pixel format of a drawing surface for WGL context creation.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct PixelFormatDescriptor
{
    // ── Flag constants ─────────────────────────────────────────────────────
    internal const uint DoubleBuffer = 0x00000001u;
    internal const uint DrawToWindow = 0x00000004u;
    internal const uint SupportOpenGl = 0x00000020u;
    internal const byte TypeRgba = 0;

    // ── Fields (must match Win32 layout exactly — 40 bytes total) ─────────
#pragma warning disable SA1307 // Field names follow Win32 naming convention for P/Invoke interop
    public ushort nSize;
    public ushort nVersion;
    public uint dwFlags;
    public byte iPixelType;
    public byte cColorBits;
    public byte cRedBits;
    public byte cRedShift;
    public byte cGreenBits;
    public byte cGreenShift;
    public byte cBlueBits;
    public byte cBlueShift;
    public byte cAlphaBits;
    public byte cAlphaShift;
    public byte cAccumBits;
    public byte cAccumRedBits;
    public byte cAccumGreenBits;
    public byte cAccumBlueBits;
    public byte cAccumAlphaBits;
    public byte cDepthBits;
    public byte cStencilBits;
    public byte cAuxBuffers;
    public byte iLayerType;
    public byte bReserved;
    public uint dwLayerMask;
    public uint dwVisibleMask;
    public uint dwDamageMask;
#pragma warning restore SA1307
}
