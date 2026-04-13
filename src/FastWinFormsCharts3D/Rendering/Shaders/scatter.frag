#version 330 core

// ── Uniforms ──────────────────────────────────────────────────────────────────
uniform vec4 uColor;

// ── Outputs ───────────────────────────────────────────────────────────────────
out vec4 FragColor;

// ── Entry point ───────────────────────────────────────────────────────────────
void main()
{
    // Render circular points by discarding fragments outside the unit circle
    vec2 coord = gl_PointCoord - vec2(0.5);
    if (dot(coord, coord) > 0.25)
    {
        discard;
    }

    FragColor = uColor;
}
