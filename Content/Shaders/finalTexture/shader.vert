#version 430 core

layout (location = 0) in vec2 vertex;

out vec2 texCoords;

const vec2 vertices[] = vec2[] (
    vec2(-1,1),
    vec2(1,1),
    vec2(1,-1),
    vec2(-1,-1));

void main()
{
    texCoords = vertex;
    gl_Position = vec4(vertices[gl_VertexID], 0, 1.0);
}