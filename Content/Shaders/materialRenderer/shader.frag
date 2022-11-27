#version 430 core

layout (location = 0) out vec4 fragColor;

uniform float depth;

void main()
{	
	fragColor = vec4(1);
	gl_FragDepth = depth;
}