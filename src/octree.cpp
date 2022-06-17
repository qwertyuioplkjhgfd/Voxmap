#include "../libs/pnm.hpp"
#include <iostream>
#include <cassert>
#include <set>

const int L = 10;

const int X = 1024;
const int Y = 256;
const int Z = 16;

int map[L][Z][Y][X];

int main()
{
	using namespace pnm::literals;

	pnm::image<pnm::rgb_pixel> img = pnm::read("maps/map.pgm");

	std::set<int> pal;
	pnm::image<pnm::rgb_pixel> pal_img = pnm::read("src/palette.ppm");

	for(int l = 0; l < L; l++)
	for(int z = 0; z < Z; z++)
	for(int y = 0; y < Y; y++)
	for(int x = 0; x < X; x++)
	{
		if(l == 0) {
			int val = int(img[Y*z + y][x].red);
			// put image bits into 3D existence table
			map[l][z][y][x] = val;
			// add to palette
			pal.insert(val);
		} else {
			int m = l-1;
			int val = map[m][z][y][x]
			for(int i = 0; i<1; i++)
			for(int j = 0; j<1; j++)
			for(int k = 0; k<1; k++)
			{
				map[m]
			}
		}
	}
	
	std::cout << "return ";
	for (int i : pal) {
		pnm::rgb_pixel p = pal_img[0][255-i];
		std::cout << "p==" << i << "?";
		std::cout << "vec3(";
		std::cout << float(p.red)/255.0 << ",";
		std::cout << float(p.green)/255.0 << ",";
		std::cout << float(p.blue)/255.0 << "):";
	}
	std::cout << "vec3(1);" << std::endl;

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

		if(bin[z][y][x] > 0){
			// no cube inside blocks
			sdf[z][y][x] = 0;
			continue;
		}

		for(int r = 1; r < Z; r++){
			// compute volume with summed volume table
			int vol = 0

				- csum(z-r, y+r, x+r)
				- csum(z+r, y-r, x+r)
				- csum(z+r, y+r, x-r)

				+ csum(z+r, y+r, x+r)

				+ csum(z+r, y-r, x-r)
				+ csum(z-r, y+r, x-r)
				+ csum(z-r, y-r, x+r)

				- csum(z-r, y-r, x-r)
				;

			// stop if there exists a block
			if(vol > 0) {
				sdf[z][y][x] = r;
				break;
			}
		}
	}

	FOR_XYZ {
		// put sdf into file
		img[Y*z + y][x].red = sdf[z][y][x]*Z;
	}

	pnm::write("maps/texture.ppm", img, pnm::format::binary);
	return 0;
}

