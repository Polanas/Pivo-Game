#version 430 core

layout (location = 0) in vec2 vertex;

//uniform sprite[10000] spritesInfoU;
uniform mat4 projection;

out vec2 texCoords;
void main()
{
	//spritesInfo = spritesInfoU;
    texCoords = vertex;
    gl_Position = vec4(vertex, 0, 1.0) * projection;
}