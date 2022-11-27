#version 430 core
#define PI 3.14

out vec4 fragColor;

in vec2 texCoords;

uniform sampler2D samplerInput;

const float THRESHOLD = 0.75;

void main(void) 
{
  const vec2 resolution = vec2(512);
  float dist = 1.0;
   
   vec2 uv = texCoords;
   uv.y *= -1;
   uv.y += 1;
  
  for (float y=0.0; y<resolution.y; y+=1.0) {
  		//rectangular to polar filter
		vec2 norm = vec2(texCoords.s, y/resolution.y) * 2.0 - 1.0;
		float theta = PI*1.5 + norm.x * PI; 
		float r = (1.0 + norm.y) * 0.5;
		
		//coord which we will sample from occlude map
		vec2 coord = vec2(-r * sin(theta), -r * cos(theta))/2.0 + 0.5;
		
		//sample the occlusion map
		vec4 data = texture(samplerInput, coord);
		
		//the current dist is how far from the top we've come
		float dst = y/resolution.y;
		
		//if we've hit an opaque fragment (occluder), then get new dist
		//if the new dist is below the current, then we'll use that for our ray
		if (data.a > 0.5) {
			dist = min(dist, dst);
			//NOTE: we could probably use "break" or "return" here
  		}
  } 

  fragColor = vec4(vec3(dist), 1.0);
}