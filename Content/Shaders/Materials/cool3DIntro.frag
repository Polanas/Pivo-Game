#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 fTexCoords;

uniform ivec2 texSize;
uniform float time;
uniform sampler2D sdfText1;
uniform sampler2D sdfText2;

float extrussion( in vec3 p, in float sdf, in float h )
{
    vec2 w = vec2( sdf, abs(p.z) - h );
  	return min(max(w.x,w.y),0.0) + length(max(w,0.0));
}


mat2 r2(float theta) {return mat2(cos(theta), sin(theta), -sin(theta), cos(theta));}

vec2 text(vec3 p, vec2 len, vec2 offset, sampler2D tex)
{
	p.xy -= vec2(.5);
	p.xy += offset;
	p.xy /= len;
	vec2 uv = p.xy;
	//uv -= vec2(1);
  //  uv /= 2;
  //  uv.y *= -1;
    
   uv.x = clamp(uv.x,-1,0);
   uv.y = clamp(uv.y,-1,0);

	float d2d = texture(tex, uv).x;
	d2d = .4-d2d;

	float d = extrussion(p, d2d, 0.2);
	//d -= .01;

	return vec2(d,0);

	//d *= 1/5;
}

vec2 map(vec3 p)
{
	vec2 d = text((p-vec3(0,0,0)), vec2(8,2), vec2(-3.5,-.5), sdfText1);
	d.y=0;

	vec3 p1 = p;
	p1.xz *= r2(3.141);
	p1 -= vec3(0,0,-20);
	p1 /= 2;
	vec2 d1 = text(p1, vec2(4,2), vec2(-1.5,-.5), sdfText2);
	d1.y=1;
	d = d1.x < d.x ? d1 : d;

	return d;
}

float march(vec3 ro, vec3 rd, out vec3 col)
{
	col = vec3(0);
	vec2 cd;
	float d;

	for (int i=0;i<256;i++)
	{
		vec3 p = ro + rd * d;
		cd = map(p);

		d += cd.x;

		if (d < 0.0001 || d > 100)
			break;
	}

	if (cd.y == 0)
		col = 0.5 + 0.5*cos(time+fTexCoords.xyx+vec3(0,2,4));
    if (cd.y == 1)
		col = vec3(1,1,0);

	return d;
}

void main()
{	
	vec2 fragCoords = fTexCoords*texSize;
	vec2 uv = (fragCoords - texSize*.5)/texSize.y;

	float gt = mod(time,20);
	
	vec3 ro = vec3(0,0,0);
	ro.z += gt;
	//else if (gt > 5.5) ro.z = 5.5;
	//ro.z += time/1.5 > mod(gt,5) ? (gt > 6 ? 5 : pow(1-((time*2) - 5), 4)) : mod(gt,5);
	vec3 rd = normalize(vec3(uv,-1));
	if (gt > 5) rd.xz *= r2( -clamp(gt-5, 0.,3.14));
	if (gt > 9) rd.xy *= r2(pow(gt-9,2));

	vec3 col;
	float d = march(ro, rd, col);
	//col = col * 1/d * 1/d * 1/d * 1/d * 64;
	if (gt > 11) col *= 1-((gt-13)/2.5);  
	
	if (d >= 100) col = vec3(0);

	fragColor=vec4(col,1);
}