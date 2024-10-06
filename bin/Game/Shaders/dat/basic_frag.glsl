#version 330 core

in vec2 texCoord;
in vec4 color;
in vec3 normal;

layout(location = 0) out vec4 fragColor;

uniform vec3 modulate_color;
uniform vec3 cam_dir;
uniform int shade;

void main() {
    if(shade==1)
    {
		vec3 norm = normalize(normal);
	    vec3 camDirection = normalize(cam_dir);
	    float intensity = max(dot(norm, camDirection), 0.0);

	    vec3 shaded_color = modulate_color * intensity;
	    fragColor = color * vec4(shaded_color, 1.0f);
    }
	else
	{
	    fragColor = color * vec4(modulate_color, 1.0f);
	}
}
