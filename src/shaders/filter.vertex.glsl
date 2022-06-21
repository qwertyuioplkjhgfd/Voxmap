#version 330 core
precision highp float;

in vec3 aPos;

out vec2 TexCoord;
 
void main()
{
	gl_Position = vec4(aPos, 1.0);
	TexCoord = aPos.xy*0.5 + 0.5;
}
