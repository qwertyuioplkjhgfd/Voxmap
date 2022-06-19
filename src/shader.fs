#version 330 core
precision highp float;

out vec4 FragColor;

in vec2 TexCoord;

uniform highp usampler2D texture1;
uniform vec2 iResolution;
uniform float iTime;
uniform vec3 iCamRot;
uniform vec3 iCamPos;

const int N = 3;
const float Nf = 3.;

const int X = 1024*N;
const int Y = 256*N;
const int Z = 16*N;

const float Xf = 1024.0*Nf;
const float Yf = 256.0*Nf;
const float Zf = 16.0*Nf;


const int MAX_RAY_STEPS = 200*N;
const int MAX_SUN_STEPS = Z;

ivec2 offset(ivec3 c) {
  //center stuff and project down to 2D
  return ivec2(
      clamp(c.x + X/2, 0, X-1),
      clamp(c.y + Y/2, 0, Y-1) + Y * clamp(c.z, 0, Z-1)
      );
}

ivec3 tex(ivec3 c) {
<<<<<<<< HEAD:src/shader.fs
  return ivec3(texelFetch(texture1, offset(c), 0));
========
  c.x = clamp(c.x + X/2, 0, X-1);
  c.y = clamp(c.y + Y/2, 0, Y-1);
  c.z = clamp(c.z, 0, Z-1);
  c /= N;
  ivec3 v = ivec3(texelFetch(mapTexture, ivec2(c.x, c.y + Y/N*c.z), 0));
  return v;
>>>>>>>> 9fc30ad... Upsample integers (inefficient), and smoother sky:src/shaders/march.fragment.glsl
}

int sdf(ivec3 c) {
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
  vec3 rayPos;
  vec3 normal;
  int minDist;
  int step;
};

March march( vec3 rayPos, vec3 rayDir, int MAX_STEPS ) {
  March res;

  res.cellPos = ivec3(floor(rayPos + 0.));
  res.rayPos = rayPos;
  res.normal;
  res.minDist = Z;
  res.step = 0;

<<<<<<< HEAD:src/shader.fs
  int dist = 4*16;

=======
  int dist = 16;

//#define MARCH_INTS
>>>>>>> 0e15432... Fix camera rotation:src/shaders/march.fragment.glsl
#ifdef MARCH_INTS
  bvec3 mask = bvec3(0);
  vec3 deltaDist = abs(vec3(length(rayDir)) / rayDir);
  ivec3 rayStep = ivec3(sign(rayDir));
  vec3 sideDist = (sign(rayDir) * (vec3(res.cellPos) - rayPos) + (sign(rayDir) * 0.5) + 0.5) * deltaDist;
#endif

  // Start marchin'
  while(res.step < MAX_STEPS && dist != 0) {
<<<<<<<< HEAD:src/shader.fs
========
    int safeDist = max(1, dist-N); // works for some reason
>>>>>>>> 9fc30ad... Upsample integers (inefficient), and smoother sky:src/shaders/march.fragment.glsl

#ifdef MARCH_INTS
    for(int j = 0; j < max(1, dist/16 - 1); j++) {
      mask = lessThanEqual(sideDist.xyz, min(sideDist.yzx, sideDist.zxy));
      sideDist += vec3(mask) * deltaDist;
      res.cellPos += ivec3(mask) * rayStep;
    }
#else
    //Axis distance to nearest cell (with a small bias).
<<<<<<< HEAD:src/shader.fs
    vec3 axisCellDist = fract(-res.rayPos * sign(rayDir)) + 1e-4;
=======
    axisCellDist = fract(-res.rayPos * sign(rayDir)) + 1e-4;
>>>>>>> 0e15432... Fix camera rotation:src/shaders/march.fragment.glsl
    //Raytraced distance to each axis.
    vec3 axisRayDist = axisCellDist / abs(rayDir);
    //Nearest axis' raytrace distance (as a vec3).
    vec3 minAxis = vec3(lessThanEqual(
          axisRayDist.xyz, min(
            axisRayDist.yzx,
            axisRayDist.zxy
          )));
    float trueDist = max(1.,float(dist/16)-1.);
    //Step to the nearest voxel cell.
    res.rayPos += rayDir * trueDist * length(minAxis * axisRayDist);
    //Get the cell position (center of the voxel).
    res.cellPos = ivec3(res.rayPos);
    res.normal = -minAxis * sign(rayDir);
#endif

    dist = sdf(res.cellPos);
    res.minDist = min(dist, res.minDist);

    res.step++;
  }

#ifdef MARCH_INTS
  res.normal = -sign(rayDir) * vec3(mask);
  res.rayPos = vec3(res.cellPos);
#endif

  return res;
}

void main() {

  // Marching setup

  vec2 screenPos = TexCoord / 2.;

  vec3 camPos = iCamPos * float(N);
  vec3 camRot = iCamRot;
  vec3 rayDir;

  vec3 camDir = vec3(0, 1, 0);
  vec3 camPlaneU = vec3(1, 0, 0);
  vec3 camPlaneV = vec3(0, 0, 1) * iResolution.y / iResolution.x;

<<<<<<<< HEAD:src/shader.fs
  vec3 rayDir = normalize(
      camDir
      + screenPos.x * camPlaneU
      + screenPos.y * camPlaneV
      );

  rayDir.yz = rotate2d(rayDir.yz, camRot.x);
  rayDir.xy = rotate2d(rayDir.xy, camRot.z);

  vec3 sunDir = normalize(vec3(1,1,1));
  sunDir.xy = rotate2d(sunDir.xy, iTime);

  March res = march(camPos, rayDir, MAX_RAY_STEPS);
========
  if(TexCoord.x > 0.5 && TexCoord.y > 0.5) {
    camPos = vec3(rotate2d(TexCoord-.625, camRot.z)*Yf + iCamPos.xy, Z);
    rayDir = normalize(vec3(.01,.01,-1.));
  } else {
    rayDir = normalize(
      camDir
    + screenPos.x * camPlaneU
    + screenPos.y * camPlaneV
    );
    rayDir.yz = rotate2d(rayDir.yz, camRot.x);
    rayDir.xy = rotate2d(rayDir.xy, camRot.z);
  }
>>>>>>>> 9fc30ad... Upsample integers (inefficient), and smoother sky:src/shaders/march.fragment.glsl

  vec3 sunDir = normalize(vec3(1,1,0.5));
  sunDir.xy = rotate2d(sunDir.xy, iTime);

  March res = march(camPos, rayDir, MAX_RAY_STEPS);

  // Start coloring

<<<<<<<< HEAD:src/shader.fs
  FragColor = vec4(1);
========
  FragColor = vec4(0,0,0,1);
// FragColor.rgb = mod(res.rayPos, 8.)/8.;
>>>>>>>> 9fc30ad... Upsample integers (inefficient), and smoother sky:src/shaders/march.fragment.glsl

  vec3 baseCol = color(res.cellPos);
//  FragColor.rgb = baseCol;

  vec3 heightCol = vec3(clamp(res.rayPos.z/Zf, 0., 1.) + 5.)/6.;

  vec3 shadeCol = mat3x3(
<<<<<<<< HEAD:src/shader.fs
      0.6, 0.7, 0.8,
      0.7, 0.8, 1.0,
      1.0, 1.0, 1.0
========
      0.90, 0.95, 1.0,
      0.90, 0.90, 1.0,
      1.00, 1.00, 1.0
>>>>>>>> 9fc30ad... Upsample integers (inefficient), and smoother sky:src/shaders/march.fragment.glsl
      ) * abs(res.normal);

  vec3 skyCol = mix(
      vec3(0.8, 0.9, 1.0),
      vec3(0.5, 0.8, 0.9),
      float(res.cellPos.z)/100.
      );

  vec3 ambCol = vec3(1);//mix(vec3(1), vec3(0.1, 0.2, 0.4), float(res.step)/MAX_RAY_STEPS);

  float sunFactor = max(0., dot(res.normal, sunDir));
  if( sunFactor > 0.){
    March sun = march(vec3(res.rayPos) + vec3(0,0,1), sunDir, MAX_SUN_STEPS);
    sunFactor *= clamp(float(sun.minDist), 0., 1.);
  }
  vec3 sunCol = mix(vec3(0.3, 0.5, 0.7), vec3(1), sunFactor);

  vec3 objCol = baseCol * shadeCol * heightCol * ambCol * sunCol;

  float dist = length(vec3(res.cellPos) - camPos);
<<<<<<<< HEAD:src/shader.fs
  float skyFactor = (res.step == MAX_RAY_STEPS) ? 1.
    : clamp(pow(dist/Yf, 3.), 0., 1.);
========
  float skyFactor = (res.step == MAX_RAY_STEPS || res.minDist > Z) ? 1.
    : clamp(abs(dist/Yf), 0., 1.);
>>>>>>>> 9fc30ad... Upsample integers (inefficient), and smoother sky:src/shaders/march.fragment.glsl

  FragColor.rgb = mix( objCol, skyCol, skyFactor );

  /*
  //Compute cheap ambient occlusion from the SDF.
  float ao = smoothstep(-1.0, 1.0, sdf(rayPos)),
  //Fade out to black using the distance.
  fog = min(1.0, exp(1.0 - length(rayPos-camPos)/8.0));

   */
  //Output final color with ao and fog (sqrt for gamma correction).
}
