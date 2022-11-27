#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 fTexCoords;
in flat ivec3 fFrameData;
in flat float fDepth;
in flat vec4 fColor;

uniform vec2 quadSize;
uniform float angle;
uniform vec3 color;
uniform float len;

#define Pi 3.14159265359


bool lineSegment(vec2 p, vec2 a, vec2 b, float thickness)
{
    vec2 pa = p - a;
    vec2 ba = b - a;
    float len = length(ba);

    vec2 dir = ba / len;
    float t = dot(pa, dir);
    vec2 n = vec2(-dir.y, dir.x);
    float factor = max(abs(n.x), abs(n.y));
    float distThreshold = (thickness - 1.0 + factor)*0.5;
    float proj = dot(n, pa);

    return (t > 0.0) && (t < len) && (proj <= distThreshold) && (proj > -distThreshold);
}

mat2 r2(float theta)
{
    return mat2(
        cos(theta), sin(theta),
        -sin(theta), cos(theta));
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
	vec2 uv = floor(fTexCoords * quadSize);
	//uv = frameUV(fFramesCount, fCurrentFrame);

    vec3 tCol = color / 255.;
    vec2 pos1, pos2, position = quadSize/2;

    pos1 = vec2(0, position.y);

    pos2.x = pos1.x + len * cos(angle+Pi);
    pos2.y = pos1.y + len * sin(angle+Pi);

    bool line1 = lineSegment(uv, pos1, pos2, 1);

    vec4 col = vec4(0);
    if (line1)
        col = vec4(tCol,1);

    fragColor = col;

    tryDiscard(fragColor.a);
	setDepth();
} 