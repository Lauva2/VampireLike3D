"""Encode complete, unretouched Player frame sequences as reviewable GIFs.
Usage: python Tools/EncodeEvidence.py Artifacts/Round5
Requires Pillow; source JPEGs and capture metadata remain the authoritative evidence.
"""
import json
import csv
import statistics
import sys
from pathlib import Path
from PIL import Image

root = Path(sys.argv[1]).resolve()
destination = root / "Animations"
destination.mkdir(exist_ok=True)
for sequence in sorted((root / "Sequences").iterdir()):
    if not sequence.is_dir():
        continue
    metadata = json.loads((sequence / "metadata.json").read_text(encoding="utf-8"))
    frames = []
    for path in sorted(sequence.glob("*.jpg")):
        with Image.open(path) as image:
            frames.append(image.resize((960, 540), Image.Resampling.LANCZOS).convert("P", palette=Image.Palette.ADAPTIVE, colors=128))
    if len(frames) != metadata["frames"]:
        raise RuntimeError(f"Incomplete sequence: {sequence.name}")
    fps = metadata.get("fixedSimulationFps")
    if fps:
        durations = round(1000 / fps)
    else:
        with (sequence / 'timestamps.csv').open(newline='', encoding='utf-8') as source:
            times = [float(row['wallSeconds']) for row in csv.DictReader(source)]
        durations = [max(10, round((b-a)*1000)) for a, b in zip(times, times[1:])]
        durations.append(round(statistics.median(durations)) if durations else 33)
    frames[0].save(destination / (sequence.name + ".gif"), save_all=True,
                   append_images=frames[1:], duration=durations,
                   loop=0, optimize=False)
    print(sequence.name, len(frames), "frames")
