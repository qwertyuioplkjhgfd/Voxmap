#include "../libs/pnm.hpp"
#include <iostream>
#include <cassert>
#include <set>

const int X = 1024;
const int Y = 256;
const int Z = 16;

int lum[Z][Y][X]; // color
int bin[Z][Y][X]; // 1 if block, else 0
int sum[Z][Y][X]; // summed volume table
int sdf[Z][Y][X]; // radius of largest fittng cube centered at block

#define KEY 69420

#define FOR_XYZ \
for(int z = 0; z < Z; z++) \
for(int y = 0; y < Y; y++) \
for(int x = 0; x < X; x++)

int hash(int a, int b){
	return (a + b)*(a + b + 1) + b*2;
}
int hash(int a, int b, int c, int d){
	return hash(hash(hash(hash(a,b),c),d),KEY) % 255;
}

int main()
{
	using namespace pnm::literals;

	pnm::image<pnm::rgb_pixel> img = pnm::read("maps/map.ppm");

	std::set<int> pal;
	pnm::image<pnm::rgb_pixel> pal_img = pnm::read("src/palette.ppm");

	assert( X == img.width() );
	assert( Y*Z == img.height() );

	// clamped sum access
	auto csum = [&](int _z, int _y, int _x){
		return sum
			[std::clamp(_z,0,Z-1)]
			[std::clamp(_y,0,Y-1)]
			[std::clamp(_x,0,X-1)]
		;
	};

	FOR_XYZ {
		// put image bits into 3D existence table
		lum[z][y][x] = int(img[Y*z + y][x].red);
		bin[z][y][x] = lum[z][y][x] ? 1 : 0;

		// check palette
		pal.insert(lum[z][y][x]);
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
		// bad encryption
		img[Y*z + y][x].red = sdf[z][y][x]*Z ^ hash(z,y,x,0);
		img[Y*z + y][x].green = lum[z][y][x] ^ hash(z,y,x,1);
		img[Y*z + y][x].blue = 0 ^ hash(z,y,x,2);
	}

	pnm::write("maps/texture.ppm", img, pnm::format::binary);
	pnm::image<pnm::rgb_pixel> dec = pnm::read("maps/texture.ppm");

	FOR_XYZ {
		// bad encryption
		dec[Y*z + y][x].red = img[Y*z+y][x].red ^ hash(z,y,x,0);
		dec[Y*z + y][x].green = img[Y*z+y][x].green ^ hash(z,y,x,1);
		dec[Y*z + y][x].blue = img[Y*z+y][x].blue ^ hash(z,y,x,2);
	}
	pnm::write("maps/decrypt.ppm", dec, pnm::format::binary);
	return 0;
}

