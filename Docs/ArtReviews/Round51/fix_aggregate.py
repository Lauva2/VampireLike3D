from pathlib import Path
p=Path('Docs/ArtReviews/Round51/aggregate_observation.py')
s=p.read_text(encoding='utf-8-sig').replace('import json, hashlib','import json, hashlib, csv')
s=s.replace("  if seq in u['sequenceFrames']:","  tp=p.parent/'timestamps.csv'\n  if tp.exists():\n   ts={int(t['frame']):float(t['wallSeconds']) for t in csv.DictReader(tp.open(encoding='utf-8-sig'))}\n   x['relativeWallSeconds']=ts.get(frame);x['timestampsSidecar']={'path':str(tp).replace('\\\\','/'),'sha256':sha(tp)}\n  if seq in u['sequenceFrames']:")
s=s.replace('Five UI feedback sequences instead use actual wall-clock timestamps.','Realtime effects/death/feedback instead use actual wall-clock timestamps, not the requested interval as measured FPS.')
p.write_text(s,encoding='utf-8')
