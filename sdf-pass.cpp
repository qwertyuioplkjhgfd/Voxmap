#include "pnm.hpp"
#include <iostream>
#include <cassert>

const int X = 1024;
const int Y = 256;
const int Z = 16;

int bin[Z][Y][X]; // 1 if block, else 0
int sum[Z][Y][X]; // summed volume table
int sdf[Z][Y][X]; // radius of largest fittng cube centered at block

#define FOR_XYZ \
	for(int z = 0; z < Z; z++) \
	for(int y = 0; y < Y; y++) \
	for(int x = 0; x < X; x++) 

int main()
{
	using namespace pnm::literals;

	pnm::image<pnm::rgb_pixel> bin_img = pnm::read("out.pbm");
	pnm::image<pnm::rgb_pixel> sdf_img = pnm::read("out.ppm");

	assert( X == bin_img.width() );
	assert( Y*Z == bin_img.height() );

	// clamped sum access
	auto csum = [&](int _z, int _y, int _x){
		return sum
			[std::clamp(_z,0,Z)]
			[std::clamp(_y,0,Y)]
			[std::clamp(_x,0,X)]
		;
	};

	int H;
	FOR_XYZ {
		// put image bits into 3D existence table
		bin[z][y][x] = bin_img[Y*z + y][x].red ? 1 : 0;
	}

	FOR_XYZ {
		// compute a summed volume table
		// aka: the number of blocks in the cube 
		// with diagonal (0,0,0)---(z,y,x), inclusive
		sum[z][y][x] = bin[z][y][x]

			+ csum(z-1, y, x)
			+ csum(z, y-1, x)
			+ csum(z, y, x-1)

			- csum(z, y-1, x-1)
			- csum(z-1, y, x-1)
			- csum(z-1, y-1, x)

			+ csum(z-1, y-1, x-1)
		;
		//std::cout << sum[z][y][x] << " ";
	}

	FOR_XYZ {
		// find greatest allowable cube's radius as sdf
		for(int r = 1; r < X; r++){
			// compute volume with summed volume table
			int vol = csum(z+r, y+r, x+r) - csum(z-r, y-r, x-r);

			// stop if there exists another block in this cube besides the central one
			if(vol > 1) {
				sdf[z][y][x] = r-1;
				break;
			}
		}
	}

	FOR_XYZ {
		// put sdf into file
		sdf_img[Y*z + y][x].red = sum[z][y][x]/10;
		sdf_img[Y*z + y][x].blue = 0;
		sdf_img[Y*z + y][x].green = 0;
	}

	pnm::write("out.ppm", sdf_img, pnm::format::binary);
	return 0;
}

