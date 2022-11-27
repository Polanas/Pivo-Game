#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 fTexCoords;

uniform sampler2D image;

uniform vec3 color;
uniform float mixAmount;

void main() 
{
//	frameUV.x = mod(frameUV.x, 1./framesAmount.x) * framesAmount.x;
//    frameUV.y = mod(frameUV.y, 1./framesAmount.y) * framesAmount.y;
	
    vec2 uv = fTexCoords;
	fragColor = vec4(mix(texture(image, uv).xyz, color/255, max(mixAmount, 0)), texture(image, uv).w);
}