"""Inspect original Player captures and chunk metadata without modifying images."""
import json
import sys
from pathlib import Path
from PIL import Image

folder = Path(sys.argv[1])
results = []
for path in sorted(folder.glob('*.png')):
    if not path.with_suffix('.world.json').exists():
        continue
    image = Image.open(path).convert('RGB')
    pixels = image.load()
    # The gameplay camera clear color, not a terrain palette color.
    background = (220, 232, 207)
    count = sum(1 for pixel in image.getdata() if pixel == background)
    largest_run = 0
    for y in range(image.height):
        run = 0
        for x in range(image.width):
            run = run + 1 if pixels[x, y] == background else 0
            largest_run = max(largest_run, run)
    world = json.loads(path.with_suffix('.world.json').read_text(encoding='utf-8-sig'))
    inactive = [c['key'] for c in world['chunks'] if not c['active'] or c['vertices'] < 24]
    results.append(dict(image=path.name, background_pixels=count,
                        longest_background_run=largest_run, inactive_chunks=inactive))
if not results:
    raise SystemExit('No paired PNG/.world.json captures found; pass the Captures directory')
print(json.dumps(results, indent=2))
if any(row['background_pixels'] or row['inactive_chunks'] for row in results):
    raise SystemExit('Terrain evidence contains camera background or invalid chunks')
