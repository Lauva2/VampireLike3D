import json
from pathlib import Path
b={'id':6,'scope':'赤晶峡谷三近广景、晶门/地标、晶卫与完成回访','sheets':[f'Docs/ArtReviews/Round52/World/biome6-{j:02}.jpg' for j in range(4)],'originals':['Artifacts/Round52/Captures/'+n+'.png' for n in ['boss-closeup-6','waterside-architecture-6','sanctuary-completed-6','geography-6-1']],'observation':'实际新看4表/15静态并补4原图。玫瑰色土底、赤粉棱晶大小簇、米白嵌晶门与碎晶石环形成第六单群系独立语言，三处广景和路径连接可辨；营地的青旗/橙火和蓝滴祭坛也有区分。门柱正侧面很简、宽阔地面留白多，晶簇同向重复仍有精修空间，归C2/C3局部问题。赤晶守卫有头顶高晶/两侧肩晶、大块石掌、浅色胸晶核和分离石足，主题色/轮廓与火山橙黑熔像不同，近景未见丢贴图或粉材，悬浮石足按该造型观察不径判缺挂点。正常战斗体量大于玩家，完整攻击死亡由Combat补。完成回访晶列保留，挑战细圈暗化，开启提示消失；玩家金色符环与召唤圈区分。六单群系静态全部实看已齐，Mixed与新怪实际战斗静态下一批。'}
Path('Docs/ArtReviews/Round52/World/batch6.json').write_text(json.dumps(b,ensure_ascii=False,indent=2),encoding='utf-8')
