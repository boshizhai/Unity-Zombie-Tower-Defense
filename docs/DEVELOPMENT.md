# 开发与验证说明

## 本次发布范围

上传现有 Unity 工程，新增 README、Git 忽略与 LFS 配置；没有重写业务代码。Unity 版本为 6000.4.2f1。首次发布以菜单进入 GameScene1 为运行入口。

发布前检查包括文件范围、包清单、场景配置、资源 GUID、Markdown 源码链接、常见凭据模式和 Git / LFS 完整性。静态检查不能替代实机试玩；本次发布未完成全流程 Play Mode 回归，也未验证独立 Player 构建。

## 已知边界

- EditorBuildSettings 目前仅启用 BeginScene 和 GameScene1。GameScene2 / GameScene3 及其配置保留，但使用前需检查场景完整性并加入 Build Profiles。
- GameDataMgr.cs 存在未使用的 UnityEditor.Timeline.Actions 引用。编辑器运行与独立 Player 构建是两种目标；构建 Player 前应移除这一编辑器引用，并检查第三方运行时脚本中的编辑器依赖。
- ChooseHeroPanel.BuyHoldCoroutine 使用 haveMoney <= 0 判断不足，钱包恰好等于价格时也会被拦截；购买流程仍需补充“刚好够钱”、长按期间切换英雄等边界测试。
- JsonMgr 优先读取 StreamingAssets 同名文件；当前目录未包含 PlayerData.json / MusicData.json。后续添加默认存档时，需要优先读取实际玩家存档。
- PoolMgr 保留的字符串 PushObj 接口会在 pushAction 为空时返回；当前怪物、特效和音效使用预制体引用版本，扩展旧接口前需调整空回调处理。
- 暂停面板、提示面板和结算面板分别管理时间和输入，重叠弹窗状态仍需回归验证。
- 对象池优化尚无性能基准数据，不宣称具体 FPS、GC 或内存提升比例。

## 建议手动回归清单

- [ ] 从 BeginScene 进入第一关，英雄、镜头、金币、主塔血量与波数正确。
- [ ] 长按购买成功，取消 / 移出恢复显示；重启后已拥有英雄和余额正确。
- [ ] 测试资金不足、资金刚好够、长按期间切换英雄和关闭面板。
- [ ] 按住 / 松开瞄准，切换单发 / 连发，翻滚、下蹲和移动正常。
- [ ] 建造、升级、升满、离开造塔点的面板状态正确。
- [ ] 怪物复用后出生、移动、攻击、死亡动画与碰撞状态正确。
- [ ] 怪物死亡只加一次奖励；最后一个怪物退场后胜利只结算一次。
- [ ] 主塔归零后失败奖励为剩余局内金币的 20%，退出再进入仍正常。
- [ ] 连续进出关卡无重复事件回调；暂停、提示框与返回菜单输入状态正常。
- [ ] 音乐开关 / 音量立即生效，音效设置影响后续播放，重开设置保持数值。

## Git LFS

大于等于 10 MiB 的非场景资源按具体路径存入 LFS；常规源码、.meta、配置与场景保持可读。新增大型资源时先执行 git lfs track，再暂存提交。

克隆后执行 git lfs pull，使用 git lfs fsck 验证资源对象。不要仅凭 ZIP 中的文件名判断资源齐全：LFS 指针文件不是真正的贴图或模型。

GitHub 的 LFS 存储和下载流量按账户配额计算，详情见 [Git LFS 计费说明](https://docs.github.com/en/billing/concepts/product-billing/git-lfs)；本次没有购买配额或修改计费设置。

