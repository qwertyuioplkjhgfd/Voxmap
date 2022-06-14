.PHONY: all clean

all: out.vox

out.svg: in.csv
	python3 csv-pass.py

out.old.ora: out.svg
	mkdir -p out-ora/data
	# rasterize depth map
	convert -flip +antialias out.svg out-ora/data/layer0.png
	# get edges
	convert -threshold 0 -edge 1 +antialias out-ora/data/layer0.png out-ora/data/layer1.png
	# zip into .ora
	cd out-ora && zip -r out.ora * && cd ..
	mv out-ora/out.ora out.ora

out.pgm: out.ora
	unzip -o out.ora -d out-ora
	mogrify -format pgm -flip out-ora/data/*.png
	mkdir -p out-pgm
	mv out-ora/data/*.pgm out-pgm
	touch out.pgm

out.vox: vox out.pgm
	./vox

out.pbm: out.png
	convert -threshold 0 out.png out.pbm

out.sdf: sdf out.pbm
	./sdf

final.png: out.png out.sdf
	echo 0

clean:
	rm -f *.o
	rm -f vox
	rm -rdf out-ora
	rm -rdf out-pgm
	rm -f out.ora

VoxWriter.o:
	clang++ -I./MagicaVoxel_File_Writer -Og -g -std=gnu++20  -o VoxWriter.o -c MagicaVoxel_File_Writer/VoxWriter.cpp

vox-pass.o: vox-pass.cpp
	clang++ -I./MagicaVoxel_File_Writer -Og -g -std=gnu++20  -o vox-pass.o -c vox-pass.cpp

vox: VoxWriter.o vox-pass.o
	clang++ -I. -g -Og -std=gnu++20 -o vox vox-pass.o VoxWriter.o

sdf: sdf-pass.cpp
	clang++ sdf-pass.cpp -g -Og -std=gnu++20 -o sdf
