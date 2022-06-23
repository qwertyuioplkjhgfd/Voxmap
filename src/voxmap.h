const int X = 1024;
const int Y = 256;
const int Z = 16;

#define FOR_XYZ \
for(int z = 0; z < Z; z++) \
for(int y = 0; y < Y; y++) \
for(int x = 0; x < X; x++)

