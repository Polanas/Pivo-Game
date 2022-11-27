#version 430 core

layout (location = 0) in vec2 vertex;

uniform float matrix[6];

void main()
{
     mat4 projection = mat4(
    matrix[0], matrix[1], 0, matrix[2],
    matrix[3], matrix[4], 0, matrix[5],
    0,0,0,0,
    0,0,0,1
    );

    gl_Position = vec4(vertex,0,1) * projection;
}