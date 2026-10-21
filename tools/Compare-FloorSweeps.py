"""Compose les balayages Unity en comparaison vidéo ; dépendances locales Pillow/ffmpeg.

Usage: python tools/Compare-FloorSweeps.py before after output
Les images sources proviennent de FloorRenderCapture.Capture(), hors Play.
"""
import json
import subprocess
import sys
from pathlib import Path

from PIL import Image, ImageDraw
import imageio_ffmpeg


def main():
    before, after, output = map(Path, sys.argv[1:4])
    output.mkdir(parents=True, exist_ok=True)
    frames = output / "frames"
    frames.mkdir(exist_ok=True)
    labels = {"cour": "Cour", "sas": "Sas", "salle": "Salle", "rasant": "Salle - angle rasant"}
    index = 0
    for zone, label in labels.items():
        for step in range(31):
            frame = Image.new("RGB", (1280, 400), (18, 23, 31))
            draw = ImageDraw.Draw(frame)
            draw.text((12, 8), f"AVANT | {label} | yaw {-30 + step * 2} deg", fill="white")
            draw.text((652, 8), "APRES | memes camera, eclairage et exposition", fill="white")
            for column, folder in enumerate((before, after)):
                with Image.open(folder / zone / f"{step:02}.png") as image:
                    frame.paste(image.resize((640, 360), Image.Resampling.LANCZOS), (column * 640, 28))
            frame.save(frames / f"{index:03}.png")
            if zone == "salle" and step == 15:
                frame.save(output / "floor-before-after.png")
            index += 1
    subprocess.run([
        imageio_ffmpeg.get_ffmpeg_exe(), "-hide_banner", "-loglevel", "error", "-y",
        "-framerate", "10", "-i", str(frames / "%03d.png"), "-c:v", "libx264",
        "-crf", "20", "-pix_fmt", "yuv420p", "-movflags", "+faststart",
        str(output / "floor-comparison.mp4"),
    ], check=True)
    (output / "comparison.json").write_text(json.dumps({
        "frames": index, "fpsPlayback": 10, "durationSeconds": index / 10,
        "zones": list(labels), "source": "Editor camera sweeps outside Play",
        "limitation": "Playback speed is synthetic, not a benchmark or headset validation",
    }, indent=2), encoding="utf-8")
    print(json.dumps({"frames": index, "output": str(output)}))


if __name__ == "__main__":
    main()
