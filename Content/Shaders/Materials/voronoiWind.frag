#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 fTexCoords;
in flat float fDepth;

uniform sampler2D image;
uniform float time;
uniform float seed;
uniform float pixelRatio;

float frac(float n)
{
    return n - floor(n);
}

vec2 voronoiRandomVec(vec2 uv, float offset)
{
    mat2 m = mat2(15.27, 47.63, 99.41, 89.98);
    uv = fract(sin(uv*m)*46839.32);
    return vec2(sin(uv.y*offset)*.5+.5, cos(uv.x*offset)*.5+.5);
}

void voronoi(vec2 uv, float angleOffset, float cellDensity, out float noise, out float cells)
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
                cells = res.y;
            }
        }
    }
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
    vec2 quadSize = textureSize(image, 0);
	vec2 uv = floor(fTexCoords * quadSize) / quadSize;

    vec4 tree = texture(image, uv);

    tryDiscard(tree.a);

    bool leafs = tree.a < 1 && tree.a > 0;

    float noise, cells = 0;
	voronoi(uv, (time+seed)*3, 10,noise, cells);
    vec4 voronoiCol = texture(image, mix(uv, uv+noise, .1));
    vec4 finalLeafsCol = voronoiCol;
    finalLeafsCol.a = 1;

    fragColor = leafs && voronoiCol.a < 1 && voronoiCol.a > 0 ? finalLeafsCol : tree;
    
    setDepth();
}
