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
uniform vec3 col;

in flat ivec3 fFrameData;
in flat float fDepth;
in flat vec4 fColor;
in flat vec2 fAtlasPos;

#define vTexture(uv) (texture(atlasTexture, uv))

vec2 vTexUV(vec2 uv, vTex virtTex)
{
    vec2 pixelOffset = 1. / vec2(fFrameData.xy);
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
    vec2 pixelOffset = 1. / vec2(textureSize(atlasTexture, 0));
    vTex image = vTex(fAtlasPos, fFrameData.xy);

	vec4 texCol = vTexture(vTexUV(fTexCoords, image));
	float alphaLeft = vTexture(vTexUV(fTexCoords, image) - vec2(pixelOffset.x, 0)).a;
	float alphaRight = vTexture(vTexUV(fTexCoords, image) + vec2(pixelOffset.x, 0)).a;
	float alphaBottom = vTexture(vTexUV(fTexCoords, image) - vec2(0, pixelOffset.y)).a;
	float alphaTop = vTexture(vTexUV(fTexCoords, image) + vec2(0, pixelOffset.y)).a;

    vec3 outLineCol = vec3((alphaLeft + alphaRight + alphaTop + alphaBottom + texCol.a));
    
    texCol = texCol.a > 0 ? texCol : vec4(outLineCol*col, outLineCol.r > 0 ? 1 : 0);
    
	fragColor = vec4(fColor.rgb/255, fColor.a) * texCol;

    finish();
}