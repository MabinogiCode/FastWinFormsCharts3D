#version 330 core

layout(location = 0) in vec3 aPosition;

uniform mat4 uMVP;
uniform float uYMin;
uniform float uYMax;

out float vNormalizedY;

void main()
{
    gl_Position = uMVP * vec4(aPosition, 1.0);

    float range = uYMax - uYMin;
    vNormalizedY = (range == 0.0) ? 0.5 : (aPosition.y - uYMin) / range;
}
