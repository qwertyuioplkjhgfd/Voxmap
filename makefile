.PHONY: all clean

all: out.vox vox

out.svg: in.csv
	python3 text-pass.py

out.ora: out.svg
	# rasterize depth map
	convert -flip +antialias out.svg out-ora/data/layer0.png
	# get edges
	convert -threshold 0 -edge 1 +antialias out-ora/data/layer0.png out-ora/data/layer1.png
	# zip into .ora
	cd out-ora && zip -r out.ora * && cd ..
	mv out-ora/out.ora out.ora

out.pgm: out.ora
	unzip -o out.ora -d out-ora
	mogrify -format pgm -compress none -flip out-ora/data/*.png
	mv out-ora/data/*.pgm out-pgm

out.vox: out.pgm
	./vox

clean:
	rm *.o
	rm vox

VoxWriter.o:
	clang++ -I./MagicaVoxel_File_Writer -Og -g -std=gnu++20  -o VoxWriter.o -c MagicaVoxel_File_Writer/VoxWriter.cpp

image-pass.o: image-pass.cpp
	clang++ -I./MagicaVoxel_File_Writer -Og -g -std=gnu++20  -o image-pass.o -c image-pass.cpp

vox: VoxWriter.o image-pass.o
	clang++ -I. -g -Og -std=gnu++20  -o vox image-pass.o VoxWriter.o
