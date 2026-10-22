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
elapsed = values('wallSeconds')
intervals = [elapsed[0]] + [right - left for left, right in zip(elapsed, elapsed[1:])]
full_windows = [row for row, duration in zip(rows, intervals) if 29.5 <= duration <= 30.5]
result['window_observations'] = dict(
    nominal_seconds=30, minimum_actual_seconds=min(intervals), maximum_actual_seconds=max(intervals),
    full_windows=len(full_windows), other_window_seconds=[duration for duration in intervals if not 29.5 <= duration <= 30.5],
    full_windows_below_60_fps=sum(float(row['fps']) < 60 for row in full_windows),
    windows_with_p95_above_16_67_ms=sum(float(row['p95FrameMs']) > 16.67 for row in rows),
    scope='Observed window throughput and tails, not a per-frame 60FPS or capped-pacing verdict.')

def segment_stats(selected, keys):
    return {key: dict(minimum=min(float(row[key]) for row in selected),
                      maximum=max(float(row[key]) for row in selected),
                      median=statistics.median(float(row[key]) for row in selected)) for key in keys}

result['fifteen_minute_segments'] = []
for segment in range(int(max(elapsed) - .001) // 900 + 1):
    selected = [row for row in rows if segment * 900 < float(row['wallSeconds']) <= (segment + 1) * 900]
    if selected:
        result['fifteen_minute_segments'].append(dict(
            start_seconds=segment * 900, end_seconds=(segment + 1) * 900, samples=len(selected),
            metrics=segment_stats(selected, ('fps', 'p95FrameMs', 'unityAllocatedMB', 'unityReservedMB',
                                            'managedMB', 'poolObjects', 'projectiles', 'chunks', 'prefabHandles'))))
build_path = root / 'BuildSamples.jsonl'
if build_path.exists():
    builds = [json.loads(line) for line in build_path.read_text(encoding='utf-8-sig').splitlines() if line.strip()]
    result['build_observations'] = dict(samples=len(builds),
        first=builds[0], last=builds[-1],
        all_six_abilities_every_sample=all(set(build['abilities']) == set(range(6)) for build in builds),
        any_fixed_imported_build=any(build['fixedImportedBuild'] for build in builds),
        quality_levels=sorted({build['quality'] for build in builds}),
        frame_rate_limits=sorted({build['frameRateLimit'] for build in builds}),
        upgrade_level_sum_first=sum(upgrade['level'] for upgrade in builds[0]['upgrades']),
        upgrade_level_sum_last=sum(upgrade['level'] for upgrade in builds[-1]['upgrades']))
external_path = root / 'ExternalMemory.csv'
if external_path.exists():
    with external_path.open(encoding='utf-8-sig', newline='') as stream:
        external_rows = list(csv.DictReader(stream))
    result['external_process_memory'] = dict(
        source='Windows Get-Process; separate observations, not replacement Player telemetry',
        samples=len(external_rows),
        process_elapsed_seconds=[float(external_rows[0]['processElapsedSeconds']),
                                 float(external_rows[-1]['processElapsedSeconds'])],
        metrics={key: dict(minimum=min(float(row[key]) for row in external_rows),
                           maximum=max(float(row[key]) for row in external_rows),
                           first=float(external_rows[0][key]), last=float(external_rows[-1][key]))
                 for key in ('privateMB', 'workingSetMB')})
    warm_rows = [row for row in external_rows if float(row['processElapsedSeconds']) >= 120]
    if warm_rows:
        third = max(1, len(warm_rows) // 3)
        result['external_process_memory']['after_120_second_startup'] = dict(
            samples=len(warm_rows),
            metrics={key: dict(
                minimum=min(float(row[key]) for row in warm_rows),
                maximum=max(float(row[key]) for row in warm_rows),
                first_third_median=statistics.median(float(row[key]) for row in warm_rows[:third]),
                last_third_median=statistics.median(float(row[key]) for row in warm_rows[-third:]))
                for key in ('privateMB', 'workingSetMB')})
    result['external_process_memory']['fifteen_minute_process_segments'] = []
    for segment in range(int(float(external_rows[-1]['processElapsedSeconds'])) // 900 + 1):
        selected = [row for row in external_rows
                    if max(120, segment * 900) <= float(row['processElapsedSeconds']) < (segment + 1) * 900]
        if selected:
            result['external_process_memory']['fifteen_minute_process_segments'].append(dict(
                start_process_seconds=max(120, segment * 900), end_process_seconds=(segment + 1) * 900,
                samples=len(selected), metrics=segment_stats(selected, ('privateMB', 'workingSetMB'))))
(root / 'Analysis.json').write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps({key: result[key] for key in ('status', 'sampled_wall_seconds', 'samples',
    'actual_camera_renders', 'biomes', 'sampled_origin_positions')}, ensure_ascii=False))
