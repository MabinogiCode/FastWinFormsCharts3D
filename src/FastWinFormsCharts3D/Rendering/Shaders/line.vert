#version 330 core

layout(location = 0) in vec3 aPosition;

// Cumulative arc-length along the line (world units) — used for dashed rendering.
layout(location = 1) in float aDistance;

uniform mat4 uMVP;
uniform vec4 uColor;

out vec4 vColor;
out float vDistance;

void main()
{
    gl_Position = uMVP * vec4(aPosition, 1.0);
    vColor = uColor;
    vDistance = aDistance;
}
