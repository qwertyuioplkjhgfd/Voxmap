#version 330 core
precision highp float;

out vec4 FragColor;

in vec2 TexCoord;

uniform highp usampler2D texture1;
uniform vec2 iResolution;
uniform float iTime;
uniform vec3 camRot;
uniform vec3 camPos;

const int X = 1024;
const int Y = 256;
const int Z = 16;

const float Yf = 256.0;
const float Zf = 16.0;

const int MAX_RAY_STEPS = 200;
const int MAX_SUN_STEPS = 5;

ivec2 offset(ivec3 c) {
  //center stuff and project down to 2D
  return ivec2(
    clamp(c.x + X/2, 0, X-1),
    clamp(c.y + Y/2, 0, Y-1) + Y * clamp(c.z, 0, Z-1)
  );
}

ivec3 tex(ivec3 c) {
  return ivec3(texelFetch(texture1, offset(c), 0));
}

int sdf(ivec3 c) {
  //if(abs(c.x)>X/2 || abs(c.y)>Y/2 || c.z>Z) return -1;
  int d = tex(c).r;
  return d;
}

vec3 color(ivec3 c) {
  int p = tex(c).g-2;
  return p<=0?vec3(0):p<=17?vec3(0.4375,0.484375,0.453125):p<=97?vec3(0.523438,0.648438,0.589844):p<=103?vec3(0.132812,0.488281,0.316406):p<=225?vec3(0.843137,0.10196,0.12549):p<=255?vec3(0.996094,0.996094,0.996094):vec3(1);
}

vec2 rotate2d(vec2 v, float a) {
  float sinA = sin(a);
  float cosA = cos(a);
  return vec2(v.x * cosA - v.y * sinA, v.y * cosA + v.x * sinA);
}

struct March {
  ivec3 cellPos;
  vec3 normal;
  int minDist;
  int step;
};

March march( vec3 rayPos, vec3 rayDir, int MAX_STEPS ) {

  March res;

  res.cellPos = ivec3(floor(rayPos + 0.));
  res.minDist = Z;
  res.step = 0;
  
  bvec3 mask = bvec3(0);
  vec3 deltaDist = abs(vec3(length(rayDir)) / rayDir);
  ivec3 rayStep = ivec3(sign(rayDir));
  vec3 sideDist = (sign(rayDir) * (vec3(res.cellPos) - rayPos) + (sign(rayDir) * 0.5) + 0.5) * deltaDist;

  int dist = 0;
  while(res.step < MAX_STEPS) {
    for(int j = 0; j < max(1, dist/16 - 1); j++) {
      mask = lessThanEqual(sideDist.xyz, min(sideDist.yzx, sideDist.zxy));
      sideDist += vec3(mask) * deltaDist;
      res.cellPos += ivec3(mask) * rayStep;
    }
    dist = sdf(res.cellPos);
    res.minDist = min(dist, res.minDist);
    if(dist < 1) {
      break;
    }
    res.step++;
  }
  res.normal = -sign(rayDir) * vec3(mask);

  return res;
}

void main() {

  // Marching setup
  
  vec2 screenPos = TexCoord;

  vec3 camDir = vec3(0, 1, 0);
  vec3 camPlaneU = vec3(1, 0, 0);
  vec3 camPlaneV = vec3(0, 0, 1) * iResolution.y / iResolution.x;

  vec3 rayDir = normalize(
      camDir
      + screenPos.x * camPlaneU
      + screenPos.y * camPlaneV
      );

  rayDir.xy = rotate2d(rayDir.xy, camRot.z);

  vec3 sunDir = normalize(vec3(1,1,1));

  March res = march(camPos, rayDir, MAX_RAY_STEPS);

  // Start coloring 
  
  FragColor = vec4(1);
  
  vec3 baseCol = color(res.cellPos);

  vec3 heightCol = (vec3(clamp(res.cellPos.z, 0, Z))/Zf + 5.)/6.;

  vec3 shadeCol = mat3x3(
    0.6, 0.7, 0.8,
    0.7, 0.8, 1.0,
    1.0, 1.0, 1.0
  ) * abs(res.normal);

  vec3 skyCol = mix(
    vec3(0.8, 0.9, 1.0),
    vec3(0.5, 0.8, 0.9),
    float(res.cellPos.z)/100.
  );

  vec3 ambCol = vec3(1);//mix(vec3(1), vec3(0.1, 0.2, 0.4), float(res.step)/MAX_RAY_STEPS);

  float sunFactor = 0.;
  if(dot(res.normal, sunDir) > 0.){
    March sun = march(vec3(res.cellPos), sunDir, MAX_SUN_STEPS);
    sunFactor = clamp(float(sun.minDist), 0., 1.);
  }
  vec3 sunCol = mix(vec3(0.3, 0.5, 0.7), vec3(1), sunFactor);

  vec3 objCol = baseCol * shadeCol * heightCol * ambCol * sunCol;

  float dist = length(vec3(res.cellPos) - camPos);
  float skyFactor = (res.step == MAX_RAY_STEPS) ? 1. 
    : clamp(pow(dist/Yf, 3.), 0., 1.);

  FragColor.rgb = mix( objCol, skyCol, skyFactor );

  /*
  //Compute cheap ambient occlusion from the SDF.
  float ao = smoothstep(-1.0, 1.0, sdf(rayPos)),
  //Fade out to black using the distance.
  fog = min(1.0, exp(1.0 - length(rayPos-camPos)/8.0));

   */
  //Output final color with ao and fog (sqrt for gamma correction).
}
