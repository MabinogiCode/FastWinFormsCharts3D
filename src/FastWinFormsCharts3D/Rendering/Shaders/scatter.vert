#version 330 core

// ── Inputs ────────────────────────────────────────────────────────────────────
layout (location = 0) in vec3 aPosition;

// ── Uniforms ──────────────────────────────────────────────────────────────────
uniform mat4 uMVP;
uniform float uPointSize;

// ── Entry point ───────────────────────────────────────────────────────────────
void main()
{
    gl_Position  = uMVP * vec4(aPosition, 1.0);
    gl_PointSize = uPointSize;
}
