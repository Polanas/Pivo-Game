#version 430 core


out vec2 fTexCoords;

uniform mat4 cameraMatrix;
uniform mat4 modelMatrix;
uniform ivec3 frameData;

const vec2[] vertices = vec2[](
    vec2(0,0),
    vec2(1,0),
    vec2(1,1),
    vec2(0,1));

void main()
{
    mat4 finalMatrix = modelMatrix;
    finalMatrix *= cameraMatrix;

    fTexCoords = vertices[gl_VertexID % 4];

    vec4 vertex = vec4(vertices[gl_VertexID % 4], 0, 1.0) * finalMatrix;

    gl_Position = vertex;
}