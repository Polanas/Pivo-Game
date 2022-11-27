#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 fTexCoords;
in flat float fDepth;
in flat vec4 fColor;

uniform vec2 quadSize;
uniform float time;
uniform float cover;
uniform vec2 coverPos;

void setDepth()
{
    gl_FragDepth = fragColor.a != 0. ? 1. - fDepth : 1.;
}

float cirlce(vec2 p, float r)
{
	return length(p) - r;
}

void tryDiscard(float alpha)
{
    if (alpha <= 0)
     discard;
}

void main()
{
	vec2 uv = floor((fTexCoords * quadSize-vec2(quadSize/2.))) / quadSize.x;
	
	float c = cirlce(uv, cover-0.01);
    vec4 col = vec4(0.,0.,0.,1.), backGround = vec4(0.); 

    fragColor = c <= 0. ? backGround : col;

	tryDiscard(fragColor.a);
	setDepth();
} 