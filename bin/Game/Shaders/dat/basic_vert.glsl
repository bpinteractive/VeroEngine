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

void main() {
    // Apply model, view, projection transformations
    gl_Position = projection * view * model * vec4(in_position, 1.0);
    texCoord = in_texCoord;
    normal = mat3(transpose(inverse(model))) * in_normal; // Translate normals to clip space
    color = in_color;
}