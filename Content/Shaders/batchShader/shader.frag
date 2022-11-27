#version 430 core

layout (location = 0) out vec4 fragColor;

in flat int fTextureId;
in vec2 fTexCoords;
in flat float fDepth;
in flat vec4 fColor;
in flat ivec3 fFrameData;

uniform sampler2D textures[32];

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

void setDepth()
{
    gl_FragDepth = fragColor.a != 0 ? (100 - fDepth)/100 : 1;
}

void tryDiscard(float alpha)
{
    if (alpha <= 0)
     discard;
}

void main()
{	
    vec2 texSize = textureSize(textures[fTextureId],0);
    vec2 uv = frameUV(fFrameData.xy, fFrameData.z);
   
	vec4 texCol =  texture(textures[fTextureId], uv);
	fragColor = vec4(fColor.rgb/255, fColor.a) * texCol;

    tryDiscard(fragColor.a);
	setDepth();
}