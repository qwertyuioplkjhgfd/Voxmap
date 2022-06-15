#version 330 core
out vec4 FragColor;

in vec2 TexCoord;

// texture samplers
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform float iTime;

//The raycasting code is somewhat based around a 2D raycasting toutorial found here:
//http://lodev.org/cgtutor/raycasting.html

const int MAX_RAY_STEPS = 128;

float sdSphere(vec3 p, float d) { return length(p) - d; }

float sdBox( vec3 p, vec3 b )
{
  vec3 d = abs(p) - b;
  return min(max(d.x,max(d.y,d.z)),0.0) +
    length(max(d,0.0));
}

int sdf(ivec3 c) {
  // center stuff
  c.x += 512;
  c.y += 128;
  c.z += 32;

  if(    0 > c.x || c.x >= 1024 
		|| 0 > c.y || c.y >= 256 
		|| 0 > c.z || c.z >= 16
	 ) return max(c.z-1, 0); // floor

  ivec2 p = c.xy;
  p.y += c.z * 256;

  return int(texelFetch(texture2, p, 0).r * 256);
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
  vec3 cameraPlaneV = vec3(0, 0, 1) * 6 / 8;
  vec3 rayDir = cameraDir + screenPos.x * cameraPlaneU + screenPos.y * cameraPlaneV;
  vec3 rayPos = vec3(0.0, -12.0, 2.0 * sin(iTime * 2.7));

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
		FragColor = vec4(0);
		return;
	 }

    mask = lessThanEqual(sideDist.xyz, min(sideDist.yzx, sideDist.zxy));

	 d = clamp(1, d - 1, 15);
    sideDist += d * vec3(mask) * deltaDist;
    mapPos += d * ivec3(vec3(mask)) * rayStep;
  }

  vec3 color;
  if (mask.x) {
    color = vec3(1,0,0);
  }
  if (mask.y) {
    color = vec3(0,1,0);
  }
  if (mask.z) {
    color = vec3(0,0,1);
  }
  FragColor.rgb = color;
}
