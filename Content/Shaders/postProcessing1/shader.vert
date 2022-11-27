#version 430 core

layout (location = 0) in vec2 vertex;

uniform mat4 projection;

out vec2 texCoords;

void main()
{
    texCoords = vertex;
    gl_Position = vec4(vertex, 0, 1.0) * projection;
}