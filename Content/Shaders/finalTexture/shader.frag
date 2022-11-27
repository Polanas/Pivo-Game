#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 texCoords;

uniform sampler2D spritesTexture;

uniform float scale;

void main()
{	
	vec2 uv = texCoords;

    uv.y *= -1;
	uv.y += 1;

	uv += scale-.5;
    uv *= 1/scale;
	uv -= .5;

	fragColor = texture(spritesTexture, uv);
}