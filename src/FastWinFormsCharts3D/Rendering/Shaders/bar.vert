#version 330 core

// Per-vertex: unit cube position in [0, 1]³.
layout(location = 0) in vec3 aLocalPos;

// Per-instance (divisor = 1): (barX, barZ, barHeight).
//   barX   — world-space left edge of the bar footprint
//   barZ   — world-space front edge of the bar footprint
//   barHeight — actual height in world units (negative clamped to 0)
layout(location = 1) in vec3 aInstanceData;

uniform mat4  uMVP;
uniform float uBarWidth;    // world-space bar width  (X direction)
uniform float uBarDepth;    // world-space bar depth  (Z direction)
uniform float uYMin;        // minimum height in the series (for Viridis mapping)
uniform float uYMax;        // maximum height in the series

out float vNormalizedY;

void main()
{
    float height = max(aInstanceData.z, 0.0);

    vec3 world = vec3(
        aInstanceData.x + aLocalPos.x * uBarWidth,
        aLocalPos.y * height,
        aInstanceData.y + aLocalPos.z * uBarDepth
    );

    gl_Position = uMVP * vec4(world, 1.0);

    float range = uYMax - uYMin;
    vNormalizedY = (range == 0.0) ? 0.5 : (height - uYMin) / range;
}
