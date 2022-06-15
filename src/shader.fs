#version 330 core
out vec4 FragColor;

in vec2 TexCoord;

// texture samplers
uniform sampler3D texture1;
uniform sampler3D texture2;
uniform float iTime;

//The raycasting code is somewhat based around a 2D raycasting toutorial found here:
//http://lodev.org/cgtutor/raycasting.html


const int X = 1024;
const int Y = 256;
const int Z = 16;

const int MAX_RAY_STEPS = X;

ivec3 project(ivec3 c){
  //center stuff and project down to 2D
  return ivec3(
	 clamp(c.x + X/2, 0, X-1),
	 clamp(c.y + Y/2, 0, Y-1),
	 clamp(c.z, 0, Z-1)
  );
}

int sdf(ivec3 c) {
//  return int(texelFetch(texture1, project(c), 0).a > 0 ? 0 : 1);
  return int(texelFetch(texture2, project(c), 0).r*16);
}

vec3 color(ivec3 c) {
  return texelFetch(texture1, project(c), 0).rgb;
}

vec2 rotate2d(vec2 v, float a) {
  float sinA = sin(a);
  float cosA = cos(a);
  return vec2(v.x * cosA - v.y * sinA, v.y * cosA + v.x * sinA);
}

void main() {
  vec2 screenPos = TexCoord;
  vec3 cameraDir = vec3(0, 1, -0.5);
  vec3 cameraPlaneU = vec3(1, 0, 0);
  vec3 cameraPlaneV = vec3(0, 0, 1) * 9 / 16;
  vec3 rayDir = cameraDir + screenPos.x * cameraPlaneU + screenPos.y * cameraPlaneV;
  vec3 rayPos = vec3(0.0, -12.0, 32.0);

  rayPos.xy = rotate2d(rayPos.xy, iTime/10);
  rayDir.xy = rotate2d(rayDir.xy, iTime/10);

  ivec3 mapPos = ivec3(floor(rayPos + 0.));

  vec3 deltaDist = abs(vec3(length(rayDir)) / rayDir);

  ivec3 rayStep = ivec3(sign(rayDir));

  vec3 sideDist = (sign(rayDir) * (vec3(mapPos) - rayPos) + (sign(rayDir) * 0.5) + 0.5) * deltaDist;

  bvec3 mask;

  for (int i = 0; i < MAX_RAY_STEPS; i++) {
	 int d = sdf(mapPos);
	 if (d == 0) break;
	 if (i == MAX_RAY_STEPS - 1 ) {
		FragColor = vec4(1);
		return;
	 }

	 mask = lessThanEqual(sideDist.xyz, min(sideDist.yzx, sideDist.zxy));

	 d = max(1, d/2);
	 sideDist += d * vec3(mask) * deltaDist;
	 mapPos += d * ivec3(mask) * rayStep;
  }

  vec3 baseCol = color(mapPos);
  vec3 heightCol = (vec3(clamp(mapPos.z, 0, Z))/Z + 2)/3;
  vec3 shadeCol = mask.x ? vec3(0.6, 0.7, 0.8)
    : mask.y ? vec3(0.7, 0.8, 1.0)
    : mask.z ? vec3(1.0)
    : vec3(0);

  FragColor.rgb = baseCol * shadeCol * heightCol;
}