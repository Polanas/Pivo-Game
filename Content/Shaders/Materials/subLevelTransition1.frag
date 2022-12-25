#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 fTexCoords;
in flat float fDepth;
in flat vec4 fColor;

uniform vec2 quadSize;
uniform float tm;
uniform vec2 pos;
uniform float rand;
uniform bool reverse;

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

float substr(in float d1, in float d2)
{
    return max(-d1, d2);
}

mat2 r2(float t)
{
	return mat2(cos(t), sin(t), -sin(t), cos(t));
}

float hash21(vec2 p)
{
    p = fract(p*vec2(356.466,1134.3455));
    p += dot(p,p+234.46);
    return fract(p.x*p.y);
}

vec2 voronoiRandomVec(vec2 uv, float offset)
{
    mat2 m = mat2(15.27, 47.63, 99.41, 89.98);
    uv = fract(sin(uv*m)*46839.32);
    return vec2(sin(uv.y*offset)*.5+.5, cos(uv.x*offset)*.5+.5);
}

void voronoi(vec2 uv, float angleOffset, float cellDensity, out float noise, out vec2 cellID)
{
    vec2 g = floor(uv * cellDensity);
    vec2 f = fract(uv * cellDensity);
    float t = 8.0;
    vec3 res = vec3(8.0, 0.0, 0.0);

    for(int y=-1; y<=1; y++)
    {
        for(int x=-1; x<=1; x++)
        {
            vec2 lattice = vec2(x,y);
            vec2 offset = voronoiRandomVec(lattice + g, angleOffset);
            float d = distance(lattice + offset, f);
            if(d < res.x)
            {
                res = vec3(d, offset.x, offset.y);
                noise = res.x;
                //cells = res.y;
                cellID = g + lattice;
            }
        }
    }
}

void main()
{
    vec2 fragCoords = floor(fTexCoords * quadSize);
    vec2 uv = (floor(fTexCoords * quadSize) - quadSize/2.) / max(quadSize.x, quadSize.y);

    float n;
    vec2 c;
    voronoi(uv-vec2(3.), rand*5. + 10., 5., n, c);

    float d = n;
    d = step(d - tm*1.2 - .08, -.05);

    fragColor = vec4(vec3(0), !reverse ? d : 1.-d);
    
    tryDiscard(fragColor.a);
	setDepth();
} 