#include "MagicaVoxel_File_Writer/VoxWriter.h"

#include <string>
#include <cmath>
#include <fstream>
#include <iostream>
#include <sstream>

using namespace std;

int main()
{
    int width = 0;    //Number of columns
    int height = 0;   //Number of rows
    int maxColor = 0;
    char type[3]; // pgm

    vox::VoxWriter vox;

    ifstream f0("out-pgm/layer0.pgm");
    ifstream f1("out-pgm/layer1.pgm");


    f0 >> type >> width >> height >> maxColor;
    f1 >> type >> width >> height >> maxColor;

    for (int y = 0; y < height; y++) {
        for (int x = 0; x < width; x++) {
            int depth = 1;
            int tag = 0;
            int misc = 0;

            f0 >> depth;
            f1 >> tag;

            depth /= 20;

            vox.AddVoxel(x, y, 0, 0); // ground
            if (tag == 0) { // ceiling
                vox.AddVoxel(x, y, depth, depth);
            } else { // wall
                for (int z = depth; z > 0; z--) {
                    vox.AddVoxel(x, y, z, depth);
                }
            }
        }
    }

    vox.SaveToFile("out.vox");
}

