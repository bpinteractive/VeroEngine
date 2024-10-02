#version 330 core

in vec2 texCoord;
in vec4 color;
in vec3 normal;
layout(location = 0) out vec4 fragColor;

void main() {
    fragColor = color;
}