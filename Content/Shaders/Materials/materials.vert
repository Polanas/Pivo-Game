#version 430 core

out vec2 fTexCoords;
out vec2 fFramesAmount;

const vec2[] vertices = vec2[](
    vec2(-1.0, -1.0), 
    vec2(1.0, -1.0),
    vec2(1.0, 1.0), 
    vec2(-1.0, 1.0));

const vec2[] texCoords = vec2[](
    vec2(0,0),
    vec2(1,0),
    vec2(1,1),
    vec2(0,1));

void main()
{
    fTexCoords = texCoords[gl_VertexID];
    gl_Position = vec4(vertices[gl_VertexID], 0, 1.0);
}