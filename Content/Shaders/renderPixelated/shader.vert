#version 430 core

layout (location = 1) in vec4 color;
layout (location = 2) in vec4 model1;
layout (location = 3) in vec2 model2;
layout (location = 4) in float depth;
layout (location = 5) in ivec3 framesInfo;

out vec2 fTexCoords;
out vec4 fColor;
out float fDepth;
flat out ivec2 fFramesCount;
flat out int fCurrentFrame;

uniform mat4 cameraMatrix;

const vec2[] vertices = vec2[](
    vec2(0.0, 0.0), 
    vec2(1.0, 0.0),
    vec2(1.0, 1.0), 
    vec2(0.0, 1.0)
);

void main()
{
    mat4 projection = mat4(
    model1.x, model1.y, 0, model1.z,
    model1.w, model2.x, 0, model2.y,
    0,0,0,0,
    0,0,0,1
    );

    projection *= cameraMatrix;

    fTexCoords = vertices[gl_VertexID];
    fColor = color;
    fDepth = depth;
    fFramesCount = framesInfo.xy;
    fCurrentFrame = framesInfo.z;

    gl_Position = vec4(vertices[gl_VertexID], 0, 1.0) * projection;
}