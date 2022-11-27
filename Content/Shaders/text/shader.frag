#version 430 core
in vec2 TexCoords;
layout (location=0) out vec4 color;

uniform sampler2D text;
uniform vec3 textColor;
uniform float alpha;

void tryDiscard(float alpha)
{
    if (alpha <= 0)
     discard;
}

void main()
{    
    vec2 texCoords = TexCoords;
    vec4 sampled = vec4(vec3(1.0), texture(text, texCoords).r);
    sampled.a = sampled.a > 0 ? 1 : sampled.a;
    color = vec4(textColor, alpha) * sampled;

    tryDiscard(color.a);
    gl_FragDepth = .05;
}  