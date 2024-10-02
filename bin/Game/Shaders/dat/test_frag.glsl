#version 330 core

in vec2 texCoord;
in vec4 color;
in vec3 normal;
layout(location = 0) out vec4 fragColor;

uniform vec3 modulate_color;

void main() {
    fragColor = vec4(1.0f, 0, 0, 1.0f);
}