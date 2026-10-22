from pathlib import Path
p=Path('Docs/ArtReviews/Round52/aggregate_observation.py')
s=Path('Docs/ArtReviews/Round51/aggregate_observation.py').read_text(encoding='utf-8-sig').replace('Round51','Round52').replace('UI51','UI52').replace('Probe51','Probe52')
s=s.replace("'terrainPixelReview':'Docs/ArtReviews/Round52/World/TerrainPixelReview.json'", "'terrainAssessment':'Artifacts/TerrainCoverage52Assessment.json'")
s=s.replace('Only listed51 observations; no older screenshots/scores/counts migrated.', 'Only listed52 observations; no older screenshots/scores/counts migrated.')
s=s.replace('Terrain audit one-color pixel failure retained; full-frame observation shows no visible continuous hole, but AA coincidence is inference.', 'Actual52 terrain-selected60-image assessment reports zero exact camera-clear pixels and no inactive chunks; broad194-image audit with intentional studio clear backgrounds is separately retained and not a world-hole verdict. Neither audit awards art points.')
p.write_text(s,encoding='utf-8')
print('Prepared aggregation code; no score/data carried, all inputs point to52; not executed before UI/Combat completion.')
