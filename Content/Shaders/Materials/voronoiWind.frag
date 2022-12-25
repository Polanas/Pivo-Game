#version 430 core

layout (location = 0) out vec4 fragColor;

struct vTex
{
	vec2 atlasPos;
	vec2 size;
};

in vec2 fTexCoords;
in flat float fDepth;
in flat vec2 fAtlasPos;
in flat ivec3 fFrameData;

uniform float time;
uniform float seed;
uniform float pixelRatio;
uniform sampler2D atlasTexture;

#define vTexCol(uv) (texture(atlasTexture, uv))

vec2 vTexUV(vec2 uv, vTex virtTex)
{
    vec2 atlasSize = textureSize(atlasTexture, 0);
    uv /= atlasSize;
    vec2 atlasUVPos = virtTex.atlasPos / atlasSize;
    uv *= virtTex.size;
    uv += atlasUVPos;

    return uv;
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

void tryDiscardAndSetDepth(float alpha)
{
     if (alpha <= 0)
        discard;

     gl_FragDepth = 1. - fDepth;
}

void main() 
{
    vec2 uv = vTexUV(fTexCoords, vTex(fAtlasPos, fFrameData.xy));
    vec4 treeCol = vTexCol(uv);
    tryDiscardAndSetDepth(treeCol.a);

     bool leafs = treeCol.a < 1 && treeCol.a > 0;

    float noise, cells = 0;
	voronoi(fTexCoords, (time+seed)*3, 10,noise, cells);

    vec2 uv2 = mix(fTexCoords, fTexCoords+noise, .1);
    vec4 voronoiCol = vTexCol(vTexUV(uv2, vTex(fAtlasPos, fFrameData.xy)));
    fragColor = voronoiCol;

    vec4 finalLeafsCol = voronoiCol;
    finalLeafsCol.a = 1;

    fragColor = leafs && voronoiCol.a < 1 && voronoiCol.a > 0 ? finalLeafsCol : treeCol;
    
   
}
