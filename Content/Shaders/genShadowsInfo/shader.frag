#version 430 core

out vec4 fragColor;

in vec2 texCoords;

uniform sampler2D samplerInput;

vec4 GetDist(vec3 col, vec2 uv)
{
	float d = col.r == 0 ? length(uv-0.5) : 1;
	return vec4(vec3(d), 1);
}

vec3 getBlackAndWhite(vec2 uv)
{
	return texture(samplerInput, uv).a == 0 ? vec3(1) : vec3(0);
}

void main()
{
	vec2 spriteUv = texCoords;
	spriteUv.y *= -1;
	spriteUv.y += 1;
   fragColor = vec4(getBlackAndWhite(spriteUv), 1);

}