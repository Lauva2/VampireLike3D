"""Encode complete, unretouched Player frame sequences as reviewable GIFs.
Usage: python Tools/EncodeEvidence.py Artifacts/Round5
Requires Pillow; source JPEGs and capture metadata remain the authoritative evidence.
"""
import json
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
    frames[0].save(destination / (sequence.name + ".gif"), save_all=True,
                   append_images=frames[1:], duration=round(1000 / metadata["fixedSimulationFps"]),
                   loop=0, optimize=False)
    print(sequence.name, len(frames), "frames")
