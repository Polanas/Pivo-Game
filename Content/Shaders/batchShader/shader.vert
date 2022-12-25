#version 430 core

layout (location = 0) in vec4 model1;
layout (location = 1) in vec2 model2;
layout (location = 2) in vec2 atlasPos;
layout (location = 3) in float depth;
layout (location = 4) in vec4 color;
layout (location = 5) in ivec3 frameData;

out vec2 fTexCoords;
out flat vec2 fAtlasPos;
out flat float fDepth;
out flat vec4 fColor;
out flat ivec3 fFrameData;

uniform mat4 cameraMatrix;

const vec2[] vertices = vec2[](
    vec2(0.0, 0.0), 
    vec2(1.0, 0.0),
    vec2(1.0, 1.0), 
    vec2(0.0, 1.0)
);

#define identity (mat4(1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0,0.0, 0.0, 0.0, 1.0))


void main()
{
    mat4 projection = mat4(
    model1.x, model1.y, 0, model1.z,
    model1.w, model2.x, 0, model2.y,
    0,0,0,0,
    0,0,0,1
    );

    projection *= cameraMatrix;

    fAtlasPos = atlasPos;
    fTexCoords = vertices[gl_VertexID % 4];
    fDepth = depth;
    fColor = color;
    fFrameData = frameData;
   
    vec4 vertex = vec4(vertices[gl_VertexID % 4], 0, 1.0) * projection;

    gl_Position = vertex;
}