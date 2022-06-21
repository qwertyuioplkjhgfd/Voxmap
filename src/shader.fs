#version 330 core
precision highp float;

out vec4 FragColor;

in vec2 TexCoord;

uniform highp sampler2D texture1;
uniform vec2 iResolution;
uniform float iTime;
uniform vec3 camRot;
uniform vec3 camPos;

const int X = 1024;
const int Y = 256;
const int Z = 16;

const float Yf = 256.0;
const float Zf = 16.0;

const int MAX_RAY_STEPS = 128;
const int MAX_SUN_STEPS = 5;

ivec3 clampedPos(ivec3 c)
{
  //center stuff and project down to 2D
  return ivec3(
    clamp(c.x, 0, X-1),
    clamp(c.y, 0, Y-1),
    clamp(c.z, 0, Z-1)
  );
}
ivec2 projectedPos(ivec3 c) {
  return ivec2(c.x, c.y + Y*c.z);
}

int sdf(ivec3 c) {
  int clampedDist = int(Zf * texelFetch(texture1, projectedPos(clampedPos(c)), 0).r);
  return clampedDist + max(ivec3(0), c - clampedPos(c));
}

vec3 color(ivec3 c) {
  return texelFetch(texture1, projectedPos(clampedPos(c)), 0).rgb;
}

vec2 rotate2d(vec2 v, float a) {
  float sinA = sin(a);
  float cosA = cos(a);
  return vec2(v.x * cosA - v.y * sinA, v.y * cosA + v.x * sinA);
}

struct March {
  vec3 rayPos;
  ivec3 cellPos;
  vec3 normal;
  float minDist;
  int step;
};

March march(vec3 rayPos, vec3 rayDir, int MAX_STEPS) {

  March res;

  res.rayPos = rayPos;
  res.minDist = float(Z);
  res.step = 0;

  int dist = 3;

  // Start marchin'
  while(res.step < MAX_STEPS && dist != 0) {

    vec3 axisCellDist;
    vec3 axisRayDist;
    vec3 minAxis;

    for(int i = 0; i < max(1, dist-1); i++) {

      //Axis distance to nearest cell (with a small bias).
      axisCellDist = fract(-res.rayPos * sign(rayDir)) + 1e-4;

      //Raytraced distance to each axis.
      axisRayDist = axisCellDist / abs(rayDir);

      //Nearest axis' raytrace distance (as a vec3).
      minAxis = vec3(lessThanEqual(
	axisRayDist.xyz, min(
	axisRayDist.yzx,
	axisRayDist.zxy
      )));

      //Step to the nearest voxel cell.
      res.rayPos += rayDir * length(minAxis * axisRayDist);
    }

    //Get the cell position (center of the voxel).
    res.cellPos = ivec3(res.rayPos);
    dist = sdf(res.cellPos);

    res.minDist = min(float(dist), res.minDist);
    res.normal = -minAxis * sign(rayDir);
    res.step++;
  }

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
  
  res.cellPos -= max(ivec3(0), ivec3(res.normal));
  vec3 baseCol = color(res.cellPos);

  vec3 heightCol = (vec3(clamp(res.rayPos.z, 0., Zf))/Zf + 5.)/6.;

  vec3 shadeCol = mat3x3(
    0.6, 0.7, 0.8,
    0.7, 0.8, 1.0,
    1.0, 1.0, 1.0
  ) * abs(res.normal);

  vec3 skyCol = mix(
    vec3(0.8, 0.9, 1.0),
    vec3(0.5, 0.8, 0.9),
    float(res.rayPos.z)/100.
  );

  vec3 ambCol = vec3(1);//mix(vec3(1), vec3(0.1, 0.2, 0.4), float(res.step)/MAX_RAY_STEPS);

  float sunFactor = 0.;
  if(dot(res.normal, sunDir) > 0.){
    March sun = march(res.rayPos, sunDir, MAX_SUN_STEPS);
    sunFactor = clamp(sun.minDist, 0., 1.);
  }
  vec3 sunCol = mix(vec3(0.3, 0.5, 0.7), vec3(1), sunFactor);

  vec3 objCol = baseCol * shadeCol * heightCol * ambCol * sunCol;

  float dist = length(res.rayPos - camPos);
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
