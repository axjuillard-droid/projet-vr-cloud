"""Rasteriser la couche Natural Earth Land 1:110m pour le player Unity."""

import argparse
from pathlib import Path
from zipfile import ZipFile
from io import BytesIO

from PIL import Image, ImageDraw
import shapefile


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("archive", type=Path, help="Archive officielle ne_110m_land.zip")
    parser.add_argument("output", type=Path, help="Chemin du PNG à produire")
    args = parser.parse_args()

    width, height = 4096, 2048
    sea, land, coast = (17, 28, 43), (43, 64, 77), (75, 100, 109)
    world = Image.new("RGB", (width, height), sea)
    draw = ImageDraw.Draw(world)

    def project(ring):
        return [((lon + 180) / 360 * (width - 1), (90 - lat) / 180 * (height - 1))
                for lon, lat in ring]

    with ZipFile(args.archive) as archive:
        with shapefile.Reader(shp=BytesIO(archive.read("ne_110m_land.shp"))) as reader:
            for shape in reader.iterShapes():
                geometry = shape.__geo_interface__
                polygons = ([geometry["coordinates"]] if geometry["type"] == "Polygon"
                            else geometry["coordinates"])
                for exterior, *holes in polygons:
                    draw.polygon(project(exterior), fill=land, outline=coast)
                    for hole in holes:
                        draw.polygon(project(hole), fill=sea, outline=coast)
    world.save(args.output, optimize=True)


if __name__ == "__main__":
    main()
