#version 430 core

layout (location = 0) in vec4 model1;
layout (location = 1) in vec2 model2;
layout (location = 2) in int textureId;
layout (location = 3) in float depth;
layout (location = 4) in vec4 color;
layout (location = 5) in ivec3 frameData;

out vec2 fTexCoords;
out flat int fTextureId;
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

//  public static void CreateTranslation(float x, float y, float z, out Matrix4 result)
//        {
//            result = Identity;
//            result.Row3.X = x;
//            result.Row3.Y = y;
//            result.Row3.Z = z;
//        }

/// [column][entry]
mat4 translation(in vec3 translation)
{
    mat4 result = identity;
    result[3][0] = translation.x;
    result[3][1] = translation.y;
    result[3][2] = translation.z;
    
    return result;
}

void main()
{
    mat4 projection = mat4(
    model1.x, model1.y, 0, model1.z,
    model1.w, model2.x, 0, model2.y,
    0,0,0,0,
    0,0,0,1
    );

    mat4 transl = translation(vec3(.1));

    projection *= cameraMatrix;

    fTextureId = textureId;
    fTexCoords = vertices[gl_VertexID % 4];
    fDepth = depth;
    fColor = color;
    fFrameData = frameData;
   
    vec4 vertex = vec4(vertices[gl_VertexID % 4], 0, 1.0) * projection;

    gl_Position = vertex;
}