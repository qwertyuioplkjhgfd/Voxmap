import png
import numpy

image = png.Reader('out.sdf')
pngdata = image.asDirect()[2]

(X, Y, Z, C) = (1024, 256, 16, 1)

xc = numpy.vstack(map(numpy.uint16, pngdata))
xyzc = numpy.reshape(xc, (X,Y,Z,C))
print(xyzc)

def horizontal_pass(single_row):
    hull_vertices = []
    hull_intersections = []
    find_hull_parabolas(single_row, hull_vertices, hull_intersections)
    march_parabolas(single_row, hull_vertices, hull_intersections)

def march_parabolas(single_row, hull_vertices, hull_intersections):
    d = single_row
    v = hull_vertices
    z = hull_intersections
    k = 0
    for q in range(len(d)):
        while z[k + 1].x < q:
            k = k + 1
            dx = q - v[k].x
            d[q] = dx * dx + v[k].y

print(xyzc)


