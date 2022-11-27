#version 430 core

layout (location = 0) out vec4 color;

in vec2 fTexCoords;
in vec4 fColor;
in float fDepth;
flat in ivec2 fFramesCount;
flat in int fCurrentFrame;

uniform sampler2D image;

vec2 getFramePos(int index, int width)
{
	 int x = index % width;
     int y = (index - x) / width;

     return vec2(x, y);
}

void main() 
{
   vec2 texSize = textureSize(image,0);
   vec2 fragCoord = fTexCoords * texSize;
   vec2 uv = (fragCoord) / texSize;

   vec2 frameSize = 1. / fFramesCount;

   uv.x = fract(uv.x/fFramesCount.x);
   uv.y = fract(uv.y/fFramesCount.y);

   vec2 framePos = getFramePos(fCurrentFrame, fFramesCount.x);
   uv += framePos*frameSize;

   color = vec4(fColor.rgb/255.0,fColor.a) * texture(image,uv);

   gl_FragDepth = color.a != 0 ? (100 - fDepth)/100 : 1;
}