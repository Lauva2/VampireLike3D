from pathlib import Path
import json
for group in ['Round54','UI54','Probe54']:
 print(group,'capture PNG',len(list((Path('Artifacts')/group/'Captures').glob('*.png'))))
