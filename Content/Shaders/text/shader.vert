#version 430 core

layout (location = 0) in vec2 vertex;

out vec2 TexCoords;

uniform mat4 projection;

const vec2[] texCoords = vec2[](
    vec2(0.0, 0.0), 
    vec2(0.0, 1.0),
    vec2(1.0, 1.0), 

    vec2(0.0, 0.0),
    vec2(1.0, 1.0),
    vec2(1.0, 0.0)
);

void main()
{
    gl_Position = vec4(vertex.xy, 0.0, 1.0) * projection;
    TexCoords = texCoords[gl_VertexID];
}  