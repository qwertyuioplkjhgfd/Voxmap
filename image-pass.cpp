#include "MagicaVoxel_File_Writer/VoxWriter.h"

#include <string>
#include <cmath>
#include <fstream>
#include <iostream>
#include <sstream>

using namespace std;

enum paints {
    Concrete,
    Roof,
    Wall,
    Steel,
    a,
    b,
    c,
    Bush,
    Grass,
};

enum colors {
    LushGreen = 153,
    ColdGray = 159,
    WarmGray = 106,
    White = 1,
    Cardinal = 30,
};

int colorOf(int paint){
    switch(paint){
        case Concrete:
            return WarmGray;
        case Steel:
            return ColdGray;
        case Roof:
            return Cardinal;
        case Wall:
            return White;
        case Bush:
            return LushGreen;
        case Grass:
            return LushGreen;
        default:
            return Concrete;
    }
}

int main()
{
    int width = 0;    //Number of columns
    int height = 0;   //Number of rows
    int maxColor = 0;
    char type[3]; // pgm

    vox::VoxWriter vox;

    ifstream f0("out-pgm/layer0.pgm");
    ifstream f1("out-pgm/layer1.pgm");
    ifstream f2("out-pgm/layer2.pgm");


    f0 >> type >> width >> height >> maxColor;
    f1 >> type >> width >> height >> maxColor;
    f2 >> type >> width >> height >> maxColor;
    

    for (int y = 0; y < height; y++) {
        for (int x = 0; x < width; x++) {
            int depth;
            int structure;
            int paint;

            f0 >> depth;
            f1 >> structure;
            f2 >> paint;

            depth /= 15;
            structure /= 15;
            paint /= 15;

            int color = colorOf(paint);
            if(depth > 0) {
                vox.AddVoxel(x, y, 0, colorOf(Concrete));
            }
            if (structure > 13) { // pole or wall

                vox.AddVoxel(x, y, depth, color);

                if (structure > 15) { // if wall, two-tone
                    color = colorOf(Wall);
                }

                for (int z = 1; z < depth; z++) {
                    vox.AddVoxel(x, y, z, color);
                }

            } else {
                vox.AddVoxel(x, y, depth, color);
            }
        }
    }

    vox.SaveToFile("out.vox");
}

