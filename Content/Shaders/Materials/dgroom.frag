#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 fTexCoords;
in flat float fDepth;

uniform vec2 quadSize;
uniform float time;
uniform sampler2D crate;

void setDepth()
{
    gl_FragDepth = fragColor.a != 0. ? 1. - fDepth : 1.;
}

float bo( vec3 p, vec3 b )
{
  vec3 q = abs(p) - b;
  return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0);
}

float bof(vec3 p, vec3 b, float e )
{
       p = abs(p)-b;
  vec3 q = abs(p+e)-e;
  return min(min(
      length(max(vec3(p.x,q.y,q.z),0.0))+min(max(p.x,max(q.y,q.z)),0.0),
      length(max(vec3(q.x,p.y,q.z),0.0))+min(max(q.x,max(p.y,q.z)),0.0)),
      length(max(vec3(q.x,q.y,p.z),0.0))+min(max(q.x,max(q.y,p.z)),0.0));
}

mat2 r2(float t)
{
	return mat2(cos(t), sin(t), -sin(t), cos(t));
}

vec2 mp(vec3 p)
{
	vec2 h, t;
	
	//t = vec2(bo(abs(p)-vec3(0.,.5,0.), vec3(1.,.05,1.)),2.);

	vec3 p2=p; //p2.xz*=r2(time);
    h = vec2(bo(p2, vec3(.15)), 4.);
    t = t.x < h.x ? t : h;

//	h = vec2(bo(abs(p)-vec3(.8,0.,0.),vec3(.05,1.,1.)), 2.);
//    t = t.x < h.x ? t : h;
//
//	h = vec2(bo((p)-vec3(0.,0.,-1.),vec3(5.,5.,.01)), 2.);
//    t = t.x < h.x ? t : h;
//
//	h = vec2(length(p-vec3(.2,0.,.5))-.1, 6.);
//	t = t.x < h.x ? t : h;

	return h;
}

vec3 nm(vec3 p) 
{
  vec2 e = vec2(1.0, -1.0) * 0.0005;
  return normalize(
    e.xyy * mp(p + e.xyy).x +
    e.yyx * mp(p + e.yyx).x +
    e.yxy * mp(p + e.yxy).x +
    e.xxx * mp(p + e.xxx).x);
}


vec2 rm(vec3 ro, vec3 rd)
{
	vec2 d;
	float d0;

	for (int i = 0; i < 256; i++)
	{
		vec3 p = ro + d0*rd;
		p.xz *= r2(time);
		d = mp(p);
		d0 += d.x;

		if (d.x >= 1000. || d.x <= 0.0001)
			break;
	}

	d.x = d0;

	return d;
}

float lt(vec3 p, out vec3 normal)
{
    vec3 lightPos = vec3(-.2,-.2,1.);
  //  lightPos.z -= time*12;
    //lightPos.xz += vec2(sin(time), cos(time))*2;
    
    vec3 l = normalize(lightPos-p);
    vec3 n = nm(p);
    normal = n;
    
    float dif = clamp(dot(n, l), 0.3, 1.);
    float d = rm(p+n * 0.001*2, l).x;
    if (d<length(lightPos - p))
        dif *= .1;
    
    return dif;
}

void main()
{
	vec2 fragCoord = (fTexCoords * quadSize);
	vec2 uv = floor((fTexCoords * quadSize)-quadSize*.5) / quadSize.y;
    vec3 col = vec3(0.);

	vec3 ro = vec3(0.,0.,1.);
	vec3 rd = normalize(vec3(uv,-1.));
	vec2 s = rm(ro, rd);

	if (s.x > 1000.)
	{
		col = vec3(.6);
	}
	else
	{
		vec3 p = ro + rd * s.x;
	p.xz *= r2(time);

		vec3 n;
		float dif = lt(p, n);
	
		col = vec3(dif);
		//if (s.y < 4.) col *= vec3(sin(length(p)*60.+time*4.)*.5+.5,0.,0.);
	   // if (s.y == 2.) col *= vec3(.1,.17,.8) * 1.;
	    if (s.y == 4.) 
		{
			vec3 colXZ = pow(texture(crate, p.xz*(1/.15)*.5-vec2(-.5)).xyz, vec3(2.2));
			vec3 colYZ = pow(texture(crate, p.yz*(1/.15)*.5-vec2(-.5)).xyz, vec3(2.2));
			vec3 colXY = pow(texture(crate, p.xy*(1/.15)*.5-vec2(-.5)).xyz, vec3(2.2));

			n = abs(n);
			col = colYZ*n.x + colXZ*n.y + colXY*n.z;
			
		}
	    if (s.y == 6.) col *= vec3(1.,1.,0.);
	}

	fragColor = vec4(pow(col, vec3(1./2.2)),1.);

	setDepth();
} 