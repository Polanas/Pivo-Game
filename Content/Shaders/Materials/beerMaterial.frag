#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 fTexCoords;

uniform sampler2D image;
uniform vec2 lightPos;
uniform float time;

in flat ivec3 fFrameData;
in flat float fDepth;
in flat vec4 fColor;

vec4 texCol(vec2 uv) { return texture(image, uv); }

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
    gl_FragDepth = fragColor.a != 0. ? 1. - fDepth : 1.;
}

void tryDiscard(float alpha)
{
    if (alpha <= 0)
     discard;
}

void main()
{
	ivec2 fFramesCount = fFrameData.xy;
    int fCurrentFrame = fFrameData.z;

	vec2 uv = floor(fTexCoords * textureSize(image, 0)) / textureSize(image, 0);
	uv = frameUV(fFramesCount, fCurrentFrame);

	vec4 col = texture(image, uv);

	tryDiscard(col.a);

	vec3 col1 = vec3(255,225,21)/255, col2 = vec3(232,255,76)/255;

	if (texCol(uv).xyz == vec3(1,0,0))
	{
		col = vec4(mix(col1, col2, sin(time*1.5)*.5+.5),1);
		col.a *= (4-fCurrentFrame)/4.;
	}

	fragColor = col * vec4(fColor.rgb/255, fColor.a);
	setDepth();
}