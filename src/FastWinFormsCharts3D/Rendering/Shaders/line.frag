#version 330 core

in vec4 vColor;
in float vDistance;

// 1.0 = dashed, 0.0 = solid.
uniform float uIsDashed;
uniform float uDashLength;   // world units per filled segment
uniform float uGapLength;    // world units per gap

out vec4 FragColor;

void main()
{
    if (uIsDashed > 0.5)
    {
        float period = uDashLength + uGapLength;
        if (mod(vDistance, period) > uDashLength)
        {
            discard;
        }
    }

    FragColor = vColor;
}
