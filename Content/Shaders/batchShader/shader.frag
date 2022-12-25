#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 fTexCoords;
in flat float fDepth;
in flat vec4 fColor;
in flat ivec3 fFrameData;
in flat vec2 fAtlasPos;

uniform sampler2D textures[31];
uniform sampler2D atlasTexture;

void setDepth()
{
    gl_FragDepth = fragColor.a != 0 ? (100 - fDepth)/100 : 1;
}

void tryDiscard(float alpha)
{
    if (alpha <= 0)
     discard;
}

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
    vec2 uv = fTexCoords;
    vec2 atlasSize = textureSize(atlasTexture, 0);
    uv /= atlasSize;
    vec2 atlasUVPos = fAtlasPos / atlasSize;
    uv *= fFrameData.xy;
    uv += atlasUVPos;

	vec4 texCol =  texture(atlasTexture, uv);

	fragColor = vec4(fColor.rgb/255, fColor.a) * texCol;

    if (fAtlasPos.y < 0)
    {
        uv = frameUV(fFrameData.xy, fFrameData.z);

	    texCol =  texture(textures[int(fFrameData.x)], uv);
	    fragColor = vec4(fColor.rgb/255, fColor.a) * texCol;
    }

    tryDiscard(fragColor.a);
	setDepth();
}