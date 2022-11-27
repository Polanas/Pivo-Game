#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 fTexCoords;
in flat float fDepth;
in flat vec4 fColor;

uniform vec2 quadSize;
uniform float tm;
uniform vec2 pos;

#define Pi 3.14159265359
#define rep(p, c) mod(p, c) - c/2

void setDepth()
{
    gl_FragDepth = fragColor.a != 0. ? 1. - fDepth : 1.;
}

void tryDiscard(float alpha)
{
    if (alpha <= 0)
     discard;
}

float box(in vec2 p, in vec2 b)
{
    vec2 d = abs(p)-b;
    return length(max(d,0.0)) + min(max(d.x,d.y),0.0);
}

float substr(in float d1, in float d2)
{
    return max(-d1, d2);
}

mat2 r2(float t)
{
	return mat2(cos(t), sin(t), -sin(t), cos(t));
}

float sharpBox(in vec2 p, in float width, in float len, in float angle, in float offset)
{
    float d, d1;
    d = box(p, vec2(width,.05));
    d1 = box(r2(angle)*(p-vec2(offset, 0.)), vec2(5.,len));

    return substr(d1, d);
}

float part(in vec2 uv, in float xMult, in float angle, in float t)
{
    uv.y *= -1;
    uv *= 1./3.;
    //uv = rep(uv, vec2(.4, .2));
    uv.x *= xMult;
    uv.y -= .05;

    const float maxLen = .13;
    const float minLen = .32;
    float d, d1;

    d = sharpBox(uv, .2, mix(maxLen, minLen, sin(t*5.)*.5+.5), angle, -.2);
    d1 = sharpBox(uv-vec2(0., -.05*2.), .2, mix(maxLen, minLen, (sin(t*5.)*.5+.5)), angle, .2);

    d = min(d,d1);

    return d;
}


void main()
{
    vec2 fragCoords = floor(fTexCoords * quadSize);
    vec2 uv = (floor(fTexCoords * quadSize) - quadSize/2.) / max(quadSize.x, quadSize.y);

    float d, d1;

    float t = min(tm + .32, .85);

    d = part(uv, 1., -Pi/4., t);
    d1 = part(uv-vec2(0.,0.), -1., Pi/4., t);

    vec3 col;
   
    d = min(d, d1);
    d = step(d, 0.);
    
    fragColor = vec4(vec3(0), sign(d));
    
    tryDiscard(fragColor.a);
	setDepth();
} 