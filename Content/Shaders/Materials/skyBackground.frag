#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 fTexCoords;
in flat float fDepth;
in flat vec4 fColor;

uniform float camY;
uniform vec3 col1;
uniform vec3 col2;
uniform vec2 quadSize;

const mat4 ditherTable = mat4(
    -4.0, 0.0, -3.0, 1.0,
    2.0, -2.0, 3.0, -1.0,
    -3.0, 1.0, -4.0, 0.0,
    3.0, -1.0, 2.0, -2.0
);


void setDepth()
{
    gl_FragDepth = fragColor.a != 0. ? 1. - fDepth : 1.;
}

void main()
{
    vec2 fragCoords = floor(fTexCoords * quadSize);
    vec2 uv = floor(fTexCoords * quadSize) / quadSize;

    vec3 col1 = col1/255;
	vec3 col2 = col2/255;

	vec3 mixedCol = mix(vec3(0),vec3(1), uv.y+camY/quadSize.y);

    mixedCol += ditherTable[int( fragCoords.x ) % 4][int( fragCoords.y ) % 4]*0.02;//*0.1;
	mixedCol = floor(mixedCol*4.)/4.;

    vec3 col = mix(col1, col2, (cos(mixedCol.x*3)+1)/2);
    fragColor = vec4(col, 1);

	setDepth();
} 