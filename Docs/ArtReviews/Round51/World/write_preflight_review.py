import json,hashlib
from pathlib import Path
from datetime import datetime,timezone
from PIL import Image
out=Path('Docs/ArtReviews/Round51/World');root=Path('Artifacts/Soak51Preflight')
def read(p):return json.loads(Path(p).read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
a=read(root/'BuildIdentity.json');b=read('Artifacts/Round51/BuildIdentity.json')
index=lambda d:{x['file']:x['sha256'].lower() for x in d['files']}
assert index(a)==index(b)
cur=read(root/'CurrentBuild.json');summary=read(root/'Summary.json');limits=read(root/'Limitations.json')
image=root/'FinalFrame.png'
record={'candidate':'Round51','version':'1.2.0','reviewer':'/root/art_lead47','reviewedAt':datetime.now(timezone.utc).isoformat(),'kind':'Supplemental original static inspection only; fixed imported preflight not7200 continuous growth', 'source':{'path':image.as_posix(),'sha256':sha(image),'pixels':list(Image.open(image).size),'originalOpenedIndividually':True,'worldSidecar':None,'qualitySidecar':None},'sameCandidateIdentity':{'fileCount':len(index(a)),'exactFileNameShaMatchWithRound51':True,'buildIdentityPath':(root/'BuildIdentity.json').as_posix(),'buildIdentitySha256':sha(root/'BuildIdentity.json')},'context':{'source':(root/'CurrentBuild.json').as_posix(),'sourceSha256':sha(root/'CurrentBuild.json'),'wallSecondsAtSample':cur['wallSeconds'],'gameSecondsAtSample':cur['gameSeconds'],'seedAtSample':cur['seed'],'characterId':None,'characterIdReason':'No explicit current-character field in CurrentBuild or frame sidecar; not inferred from unlocked profile.','level':cur['level'],'abilityIds':cur['abilities'],'fixedImportedBuild':cur['fixedImportedBuild'],'qualityAtSample':cur['quality'],'renderResolutionFromSummary':summary['renderResolution'],'scope':'Nearby final build sample, not a per-PNG exact-timestamp world sidecar. HUD shows152:59 from imported game clock, not evidence of152min wall run.'},'sampling':{'newStaticSourcesPersonallyViewed':1,'newOriginalFilesOpened':1,'newDynamicFramesViewed':0,'newAuxiliarySheets':0,'coreCountsUnchanged':True},'observations':['完整原图为明亮沙海：金黄沙脊、路径阴影、绿掌/棕榈和散石可辨，未见连续地形洞、粉材或明显缺块。','中心玩家棕色头发/面部与青色足环仍可定位；周围青白灵体/飞剑弧线、橙焰、弹道等有分组，不能仅凭HUD能力9断言每一能力的全程发射。','屏幕两侧精英金冠密集重复，并与青蓝攻击轨迹/经验/冻结圈局部挤在一起；己方环轨和大橙色效果占据较大面积。既有E2局部拥挤4分扣分保留，不提高为5。','本帧没有出现持续玩家失踪或核心HUD/中文关键裁切的静态直接阻断。能力栏显示九能力摘要但只露初段六图标和滚动尾部，单帧不证明完整连续滚动。','单帧没有完整Boss预警/危险生效/闪避过程，不能补判全程H07/H08或F1/F2时序。'], 'scoreEffect':{'coreTotal':91,'changed':False,'existingDeductionsRetained':True,'newStaticArtBlockerObserved':False},'runtimeContextNonScoring':{'wallSeconds':summary['wallSeconds'],'actualCameraRenders':limits['actualCameraRenders'],'errorCount':summary['errorCount'],'originalExitCode':read(root/'ProcessExit.json')['exitCode'],'minimumWindowFps':limits['fps']['minimum'],'windowsBelow60':limits['windows']['full_windows_below_60_fps'],'performancePassClaimed':False},'limits':['Fixed imported520-level nine-ability diagnostic load, controlled healing,180.2sec only; not natural growth or actual7200sec run.','One full window59.42FPS and five windows p95>16.67ms retained. No60FPS/performance pass derived from art review.','One1920x1080 raw frame actually opened; no continuous footage or every preflight frame viewed.','CurrentBuild is a neighboring sample; no exact frame role/time/position sidecar. Seed28354315 recorded with that provenance.','Actual nonfixed7200sec run has started only; final frame not yet supplied/observed.','Shared ArtReview.md/ArtReviewResult.json and core observation counts unchanged pending final supplement.']}
(out/'DeepPreflightReview.json').write_text(json.dumps(record,ensure_ascii=False,indent=2),encoding='utf-8')
md=f'''# Round51 固定深构筑预检末帧补记

评审人：`/root/art_lead47`。只读另开原始 [FinalFrame.png](../../../../Artifacts/Soak51Preflight/FinalFrame.png)，1920×1080，SHA256 `{record['source']['sha256']}`。29条构建文件名/SHA与Round51精确相同；逐项记录见[DeepPreflightReview.json](DeepPreflightReview.json)。原图未改，没有制作新联系表或观看新动态。

本帧金黄沙脊/路径阴影、仙人掌/棕榈和散石有沙海特征，未见可见连续地形洞、粉材或明显缺块。中心玩家头脸和青色足环仍可定位，青白环轨、橙焰与弹道有形色分组。画面两侧金色精英冠、青蓝攻击/经验/冻结圈仍局部拥挤，环轨与橙色效果占据较大面积，**保留E2=4与核心91分，不因深等级加分**。核心HUD与中文字未见静态直接裁切；HUD能力9摘要与初段六图标可见，只证明该状态，不能证明HUD连续滚动或每一能力全程发射。

这是固定导入520级九能力、受控治疗的180.2秒预检，Summary记录13516真实渲染、0错误、原退出0。CurrentBuild末样本seed28354315、gameSeconds9179.998、quality5；画面152:59是导入后的游戏时钟，不是152分钟实际运行。PNG没有独立world/quality sidecar，精确当帧角色ID/时间/位置未知；不从已解锁档案推定角色。运行上下文来自单列CurrentBuild与Summary，不冒充逐帧元数据。

该帧未见新的静态美术直接阻断，但不覆盖完整预警/命中/闪避/长期遮挡或时序。1个完整窗口59.42FPS与5个窗口p95>16.67ms保留，不作为性能通过。实际非固定持续成长7200秒刚启动、末帧未提供；不计完成。三人核心观看数保持原快照，此次本人新增1张静态原图，最终汇总时再去重。共享ArtReview.md/ArtReviewResult.json等待实际末帧复核后更新。
'''
(out/'DeepPreflightReview.md').write_text(md,encoding='utf-8')
p=out/'Review.json';w=read(p)
w['supplementaryActualViewing']={'deepPreflightOriginalStatic':1,'deepPreflightDynamic':0,'report':'Docs/ArtReviews/Round51/World/DeepPreflightReview.json','coreCountsUnchanged':True}
p.write_text(json.dumps(w,ensure_ascii=False,indent=2),encoding='utf-8')
notes=out/'BatchNotes.md'
with notes.open('a',encoding='utf-8') as f:f.write('\n## 补充批次5：固定520级九武器预检末帧\n\n另开Soak51Preflight/FinalFrame.png原始1920×1080。中心头脸/青足环可辨，边缘精英金冠与青蓝攻击轨迹仍拥挤，保留E2=4及核心91，未见新静态阻断。29构建条目同版；seed28354315来自末邻CurrentBuild，PNG本身无world sidecar。完整SHA/观察/180.2秒及59.42FPS窗口限制见DeepPreflightReview.json/md。此批仅新增1静态原图、0动态/0联系表，核心146/40/46计数保持快照，后续统一去重。实际7200末帧未产生，不标通过，不更新共享结论。\n')
print('Written World preflight supplement:',record['source']['sha256'],'identity',len(index(a)),'core unchanged')
