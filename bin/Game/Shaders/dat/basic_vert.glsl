#version 330 core

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec3 in_normal;
layout(location = 3) in vec4 in_color;

out vec2 texCoord;
out vec4 color;
out vec3 normal;
out vec3 fragPosition;

void main() {
    // Apply model, view, projection transformations
    fragPosition = (model * vec4(in_position, 1.0)).xyz;
    gl_Position = projection * view * model * vec4(in_position, 1.0);
    
    // Pass texture coordinates
    texCoord = in_texCoord;
    
    // Transform normal, tangent, and bitangent by the model matrix
    mat3 normalMatrix = mat3(transpose(inverse(model)));
    normal = normalMatrix * in_normal;

    // Pass vertex color
    color = in_color;
}
