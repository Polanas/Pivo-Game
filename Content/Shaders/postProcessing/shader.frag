#version 430 core

out vec4 fragColor;

in vec2 texCoords;

uniform sampler2D spritesTexture;
uniform sampler2D lightTexture;
uniform sampler2D lightMaskTexture;
uniform sampler2D shadowCastersTexture;

void main()
{
	vec2 spriteUv = texCoords;
	spriteUv.y *= -1;
	spriteUv.y += 1;
	vec4 lightCol = texture(lightTexture, spriteUv);
	vec3 lightMaskCol = texture(lightMaskTexture, spriteUv).xyz;
	vec4 spriteCol = texture(spritesTexture, spriteUv);
	vec3 shadowCastersCol = texture(shadowCastersTexture, spriteUv).xyz;
//
    vec4 result = spriteCol * vec4(vec3(1) * (lightCol.a > 0.2 ? lightCol.a : 0.2), 1.);
	fragColor = lightMaskCol.r > 0 ? vec4(spriteCol.xyz * (shadowCastersCol.r == 1. ? (lightCol.a > 0.2 ? lightCol.a : 0.2) : 0.2), 1) : result;
	//fragColor = ;
	
}