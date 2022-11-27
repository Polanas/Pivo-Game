#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 texCoords;

uniform sampler2D spritesTexture;

void main()
{	
	vec2 uv = texCoords;

    uv.y *= -1;
	uv.y += 1;

	fragColor = texture(spritesTexture, uv);
}