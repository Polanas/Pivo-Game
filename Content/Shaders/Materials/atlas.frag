#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 fTexCoords;
uniform sampler2D spriteTexture;
uniform ivec3 frameData;

vec2 getFramePos(int index, int width)
{
	 int x = index % width;
     int y = (index - x) / width;

     return vec2(x, y);
}

vec2 frameUV(ivec2 fFramesCount, int fCurrentFrame)
{   
    vec2 uv = fTexCoords;
    vec2 frameSize = 1. / fFramesCount;
   
    uv.x = fract(uv.x/fFramesCount.x);
    uv.y = fract(uv.y/fFramesCount.y);
   
    vec2 framePos = getFramePos(fCurrentFrame, fFramesCount.x);
    uv += framePos*frameSize;

    return uv;
}

void main()
{	
    vec2 texSize = textureSize(spriteTexture,0);
    vec2 uv = frameUV(frameData.xy, frameData.z);
   
	vec4 texCol = texture(spriteTexture, uv);
	fragColor = texCol;
}