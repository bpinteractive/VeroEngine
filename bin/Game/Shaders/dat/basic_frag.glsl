#version 330 core

in vec2 texCoord;
in vec4 color;
in vec3 normal;
in vec3 fragPosition;

layout(location = 0) out vec4 fragColor;

uniform vec3 modulate_color;
uniform int shade;
uniform int num_lights;
uniform sampler2D Albedo;

uniform int dirlight_enabled = 0;   // 1 if directional light is enabled, 0 otherwise
uniform vec3 dirlight_rot;          // Direction of the directional light
uniform vec3 dirlight_col;          // Color of the directional light

uniform vec3 ambient_col = vec3(0.1, 0.1, 0.1);

#define MAX_LIGHTS 256

uniform vec3 light_positions[MAX_LIGHTS];
uniform vec3 light_colors[MAX_LIGHTS];

// Attenuation factors for point lights
uniform float constant = 1.0;
uniform float linear = 0.09;
uniform float quadratic = 0.032;

void main() {
    if (shade == 1)
    {
        vec3 norm = normalize(normal);
        vec3 shaded_color = ambient_col;

        for (int i = 0; i < num_lights; ++i)
        {
            vec3 light_dir = light_positions[i] - fragPosition; // Direction from fragment to light (not normalized)
            float distance = length(light_dir); // Calculate distance to light
            light_dir = normalize(light_dir); // Normalize the light direction

            // Diffuse shading (dot product)
            float diffuse_intensity = max(dot(norm, light_dir), 0.0);

            // Attenuation calculation
            float attenuation = 1.0 / (constant + linear * distance + quadratic * (distance * distance));

            // Combine diffuse intensity with attenuation
            vec3 light_effect = light_colors[i] * diffuse_intensity * attenuation;

            // Accumulate the result
            shaded_color += light_effect;
        }

        // Directional light shading
        if (dirlight_enabled == 1)
        {
            vec3 dir_light_dir = normalize(-dirlight_rot); // Light direction (negated, as it points towards the object)
            float diffuse_intensity = max(dot(norm, dir_light_dir), 0.0);

            // No attenuation for directional light
            vec3 dir_light_effect = dirlight_col * diffuse_intensity;

            // Accumulate directional light contribution
            shaded_color += dir_light_effect;
        }

        vec4 albedo = texture(Albedo, texCoord);
        fragColor = vec4(modulate_color, 1.0f) * color * vec4(shaded_color, 1.0f) * albedo;
    }
    else
    {
        vec4 albedo = texture(Albedo, texCoord);
        fragColor = color * vec4(modulate_color, 1.0f) * albedo;
    }
}
