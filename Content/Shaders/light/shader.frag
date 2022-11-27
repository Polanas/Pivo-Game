#version 430 core
#define PI 3.14

layout(location = 0) out vec4 fragColorLight;
layout(location = 1) out vec4 fragColorMask;

in vec2 texCoords;

float radius = 1; //TODO: make this a uniform

uniform sampler2D samplerInput;

float samp(vec2 coord, float r) {
	return step(r, texture2D(samplerInput, coord).r);
}

void main(void) 
{
	const vec2 resolution = vec2(512);
	//rectangular to polar
	vec2 norm = texCoords.st * 2.0 - 1.0;
	float theta = atan(norm.y, norm.x);
	float r = length(norm);	
	float coord = (theta + PI) / (2.0*PI);
	
	//the tex coord to sample our 1D lookup texture	
	//always 0.0 on y axis
	vec2 tc = vec2(coord, 0.0);
	
	//the center tex coord, which gives us hard shadows
	float center = samp(tc, r);        
	
	//we multiply the blur amount by our distance from center
	//this leads to more blurriness as the shadow "fades away"
//	float blur = (1./resolution.x)  * smoothstep(0., 1., r); 
	
	//now we use a simple gaussian blur
	float sum = 0.0;
	
//	sum += samp(vec2(tc.x - 4.0*blur, tc.y), r) * 0.05;
//	sum += samp(vec2(tc.x - 3.0*blur, tc.y), r) * 0.09;
//	sum += samp(vec2(tc.x - 2.0*blur, tc.y), r) * 0.12;
//	sum += samp(vec2(tc.x - 1.0*blur, tc.y), r) * 0.15;
	
	sum += center * 0.9;
	
////	sum += samp(vec2(tc.x + 1.0*blur, tc.y), r) * 0.15;
////	sum += samp(vec2(tc.x + 2.0*blur, tc.y), r) * 0.12;
////	sum += samp(vec2(tc.x + 3.0*blur, tc.y), r) * 0.09;
////	sum += samp(vec2(tc.x + 4.0*blur, tc.y), r) * 0.05;
	
	//sum of 1.0 -> in light, 0.0 -> in shadow
 	
 	//multiply the summed amount by our distance, which gives us a radial falloff
 	//then multiply by vertex (light) color  
	//vec3 col = vec3(sum * smoothstep(1.,0., r);
	float a =  mix(1.0, 0.0, r*(1./radius));
	float a1 = round(a * 255*1.1);
	a1 = floor(a1/50.)*50. / 255;

 	fragColorMask = vec4(sum*a1 <= 0.01 ? vec3(1) : vec3(0),1);
	fragColorLight = vec4(vec3(1), a1);
}