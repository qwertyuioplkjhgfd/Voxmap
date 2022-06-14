#include "pnm.hpp"
#include <iostream>

int main()
{
	using namespace pnm::literals;
	pnm::image<pnm::rgb_pixel> b = pnm::read("out.pbm");
	pnm::image<pnm::rgb_pixel> o = pnm::read("out.pgm");

	int width = b.width();
	int height = b.height();
	int depth = 16;
	height /= depth;

	auto w = [&](int x, int y, int z){
		return b[z*height + y][x].red;
	};

	for(int z = 0; z < depth; z++)
	for(int y = 0; y < height; y++)
	for(int x = 0; x < width; x++) {
		int d;
		int minD = depth;

		for(d = 0; !w(x, y, z - d) && d < z && d < minD; d++) continue;
		minD = d;

		for(d = 0; !w(x, y, z + d) && d < depth - z && d < minD; d++) continue;
		minD = d;

		for(d = 0; !w(x, y - d, z) && d < y && d < minD; d++) continue;
		minD = d;

		for(d = 0; !w(x, y + d, z) && d < height - y && d < minD; d++) continue;
		minD = d;

		for(d = 0; !w(x - d, y, z) && d < x && d < minD; d++) continue;
		minD = d;

		for(d = 0; !w(x + d, y, z) && d < width - x && d < minD; d++) continue;
		minD = d;

		pnm::rgb_pixel i = b[z*height + y][x];
		i.red = minD;
		i.green = minD;
		i.blue = minD;
		o[z*height + y][x] = i;
	}
	pnm::write("out.pgm", o, pnm::format::binary);
	return 0;
}

