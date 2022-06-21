#version 330 core
precision highp float;

out vec4 FragColor;

in vec2 TexCoord;

uniform highp sampler2D marchTexture0;
uniform highp sampler2D marchTexture1;
uniform highp sampler2D marchTexture2;
uniform highp sampler2D marchTexture3;
uniform int iFrame;
uniform int iTAA;

void main()
{
	ivec2 m = ivec2(gl_FragCoord.xy) % iTAA;
	if(m ==ivec2(0,0)) {
		FragColor = texture(marchTexture0, TexCoord);
	} else if (m == ivec2(0,1)) {
		FragColor = texture(marchTexture1, TexCoord);
	} else if (m == ivec2(1,0)) {
		FragColor = texture(marchTexture2, TexCoord);
	} else if (m == ivec2(1,1)) {
		FragColor = texture(marchTexture3, TexCoord);
	}
}
