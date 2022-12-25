#version 430 core

layout (location = 0) out vec4 fragColor;

struct vTex
{
	vec2 atlasPos;
	vec2 size;
};

in vec2 fTexCoords;

uniform float time;
uniform sampler2D atlasTexture;

in flat ivec3 fFrameData;
in flat float fDepth;
in flat vec4 fColor;
in flat vec2 fAtlasPos;

#define vTexture(uv) (texture(atlasTexture, uv))

vec2 vTexUV(vec2 uv, vTex virtTex)
{
    vec2 atlasSize = textureSize(atlasTexture, 0);
    uv /= atlasSize;
    vec2 atlasUVPos = virtTex.atlasPos / atlasSize;
    uv *= virtTex.size;
    uv += atlasUVPos;

    return uv;
}

void finish()
{
     if (fragColor.a <= 0)
        discard;

     gl_FragDepth = fragColor.a != 0. ? 1. - fDepth : 1.;
}


void main()
{
	vec4 texCol = vTexture(vTexUV(fTexCoords, vTex(fAtlasPos, fFrameData.xy)));

	fragColor = vec4(fColor.rgb/255, fColor.a) * texCol;

    finish();
}