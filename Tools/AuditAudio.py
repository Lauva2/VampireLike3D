"""Technical checks for generated PCM assets; this does not claim human listening."""
import array
import json
import math
import sys
import wave
from pathlib import Path

results = []
for path in sorted(Path(sys.argv[1]).glob('*.wav')):
    with wave.open(str(path), 'rb') as source:
        if source.getsampwidth() != 2 or source.getnchannels() != 1:
            raise ValueError(f'Unsupported format: {path}')
        samples = array.array('h', source.readframes(source.getnframes()))
        values = [sample / 32768 for sample in samples]
        peak = max(map(abs, values))
        rms = math.sqrt(sum(v * v for v in values) / len(values))
        dc = sum(values) / len(values)
        tail = abs(values[-1])
        passed = 0.005 < peak < 0.95 and rms > 0.002 and abs(dc) < 0.015 and tail < 0.02
        results.append(dict(file=path.name, duration=len(values) / source.getframerate(),
                            peak=round(peak, 5), rms=round(rms, 5), dc=round(dc, 6),
                            end_amplitude=round(tail, 5), passed=passed))
print(json.dumps(dict(passed=bool(results) and all(r['passed'] for r in results),
                     scope='PCM levels, silence, DC, clipping and loop endpoint only; no listening claim',
                     files=results), indent=2))
