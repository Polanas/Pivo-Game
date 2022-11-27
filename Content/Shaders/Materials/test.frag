#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 fTexCoords;
in flat ivec3 fFrameData;
in flat float fDepth;
in flat vec4 fColor;

uniform sampler2D tex;
uniform sampler2D tex1;
uniform vec2 quadSize;
uniform float time;

#define myTexUV(tex) (fTexCoords / (textureSize(tex, 0) / quadSize))

void setDepth()
{
    gl_FragDepth = fragColor.a != 0. ? 1. - fDepth : 1.;
}

vec4 myTexture(sampler2D tex, vec2 uv)
{
    vec2 texSize = textureSize(tex, 0);
    bool isOutside = false;
    vec2 fragCoord = uv * texSize;
    isOutside = fragCoord.x > texSize.x || fragColor.x < -uv.x || fragCoord.y > texSize.y || fragCoord.y < -uv.y;

	vec4 col = texture(tex, uv);
    col.a *= float(!isOutside);

    return col;
}

void moveMyTex(inout vec2 uv, vec2 p, sampler2D myTex)
{
    uv -= (1. / textureSize(myTex, 0) * p);
}

//c1 is on top of c2
vec4 colUnion(vec4 c1, vec4 c2)
{
    return c1.a <= 0 ? c2 : c1;
}

void main()
{
    vec2 uv = myTexUV(tex);
    vec2 uv2 = myTexUV(tex1);
    vec2 fragCoord = floor(fTexCoords * quadSize);

   // uv.x += mod(time/4., 2.);
    moveMyTex(uv2, vec2(0,2), tex1);
    moveMyTex(uv, vec2(-mod(floor(time*10), 100),0), tex);
   
    vec4 col1 = myTexture(tex1, uv2);
    vec4 col2 = myTexture(tex, uv);
 
    col2 *= float((fragCoord.x >= 2 && fragCoord.x <= 17));
    col1 = colUnion(col2, col1);

    fragColor = col1;

    setDepth();
} 