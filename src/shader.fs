#version 330 core
out vec4 FragColor;

in vec2 TexCoord;

uniform sampler3D texture1;
uniform sampler3D texture2;
uniform float iTime;

const int X = 1024;
const int Y = 256;
const int Z = 16;

const int MAX_RAY_STEPS = 128;

ivec3 offset(ivec3 c) {
  //center stuff and project down to 2D
  return ivec3(
      clamp(c.x + X/2, 0, X-1),
      clamp(c.y + Y/2, 0, Y-1),
      clamp(c.z, 0, Z-1)
  );
}

int sdf(ivec3 c) {
  return int(Z * texelFetch(texture2, offset(c), 0).r);
}

vec3 color(ivec3 c) {
  return texelFetch(texture1, offset(c), 0).rgb;
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
  vec3 rayPos = vec3(-300.0, -12.0, 32.0);

  //rayPos.xy = rotate2d(rayPos.xy, iTime/10);
  rayDir.xy = rotate2d(rayDir.xy, iTime/4);

  ivec3 mapPos = ivec3(floor(rayPos + 0.));

  vec3 deltaDist = abs(vec3(length(rayDir)) / rayDir);

  ivec3 rayStep = ivec3(sign(rayDir));

  vec3 sideDist = (sign(rayDir) * (vec3(mapPos) - rayPos) + (sign(rayDir) * 0.5) + 0.5) * deltaDist;

  bvec3 mask;

  int d = Z;
  int i = 0;
  for (; i < MAX_RAY_STEPS; i++) {
    d = min(d, sdf(mapPos));
    if (d == 0) break;
    for(int j = 0; j < max(1, d - 1); j++) {
      mask = lessThanEqual(sideDist.xyz, min(sideDist.yzx, sideDist.zxy));
      sideDist += vec3(mask) * deltaDist;
      mapPos += ivec3(mask) * rayStep;
    }
  }

  vec3 baseCol = color(mapPos);
  vec3 heightCol = (vec3(clamp(mapPos.z, 0, Z))/Z + 5)/6;
  vec3 shadeCol = mask.x ? vec3(0.6, 0.7, 0.8)
    : mask.y ? vec3(0.7, 0.8, 1.0)
    : mask.z ? vec3(1.0)
    : vec3(0);
  vec3 skyCol = mix(
    vec3(0.8, 0.9, 1.0), 
    vec3(0.5, 0.8, 0.9),
    float(mapPos.z)/100
  );
  vec3 ambCol = mix(vec3(1), vec3(0.1, 0.2, 0.4), float(i)/MAX_RAY_STEPS);

  vec3 objCol = baseCol * shadeCol * heightCol * ambCol;

  float dist = length(mapPos - rayPos);
  float skyFactor = (i == MAX_RAY_STEPS) ? 1 : clamp(pow(dist/Y, 3), 0, 1);

  FragColor.rgb = mix( objCol, skyCol, skyFactor );
}
