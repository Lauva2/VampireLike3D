"""Summarize raw Player telemetry; never turn an unfinished run into a pass."""
import csv
import json
import statistics
import sys
from pathlib import Path

root = Path(sys.argv[1])
with (root / 'Samples.csv').open(encoding='utf-8-sig', newline='') as stream:
    rows = list(csv.DictReader(stream))
if not rows:
    raise SystemExit('No measured samples yet')

def values(key):
    return [float(row[key]) for row in rows if row.get(key) not in (None, '', 'NA')]

def metric(key):
    data = values(key)
    if not data:
        return dict(available=False)
    third = max(1, len(data) // 3)
    return dict(available=True, minimum=min(data), maximum=max(data),
                median=statistics.median(data), mean=statistics.mean(data),
                first_third_median=statistics.median(data[:third]),
                last_third_median=statistics.median(data[-third:]))

summary_path = root / 'Summary.json'
summary = json.loads(summary_path.read_text(encoding='utf-8-sig')) if summary_path.exists() else None
termination_path = next((root / name for name in ('Interrupted.json', 'Cancelled.json')
                         if (root / name).exists()), None)
termination = json.loads(termination_path.read_text(encoding='utf-8-sig')) if termination_path else None
errors_path = root / 'Errors.txt'
errors = errors_path.read_text(encoding='utf-8-sig').strip() if errors_path.exists() else ''
finished = bool(summary and summary.get('wallSeconds', 0) >= summary.get('requestedSeconds', 7200))
origins = {(int(row['originX']), int(row['originZ'])) for row in rows}
result = dict(
    status='complete_without_runtime_errors' if finished and not errors and summary.get('errorCount') == 0
        else 'failed_or_incomplete' if summary or termination else 'running',
    completion_is_not_a_performance_or_art_approval=True,
    sampled_wall_seconds=values('wallSeconds')[-1], samples=len(rows),
    actual_camera_renders=int(sum(values('cameraRenders'))),
    biomes=sorted({row['biome'] for row in rows}), sampled_origin_positions=len(origins),
    origin_extent_chunks=dict(x=[min(p[0] for p in origins), max(p[0] for p in origins)],
                              z=[min(p[1] for p in origins), max(p[1] for p in origins)]),
    runtime_errors=errors.splitlines() if errors else [],
    source_summary=summary, termination_record=termination,
    metrics={key: metric(key) for key in ('fps', 'p95FrameMs', 'maxFrameMs', 'unityAllocatedMB',
        'unityReservedMB', 'managedMB', 'privateMB', 'enemies', 'poolObjects', 'chunks',
        'prefabHandles', 'cpuFrameMs', 'gpuFrameMs', 'drawCalls', 'triangles',
        'experienceOrbs', 'unclaimedExperience', 'projectiles')})
(root / 'Analysis.json').write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps({key: result[key] for key in ('status', 'sampled_wall_seconds', 'samples',
    'actual_camera_renders', 'biomes', 'sampled_origin_positions')}, ensure_ascii=False))
