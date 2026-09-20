# Unity Zombie Tower Defense

**第三人称射击 × 丧尸塔防**

基于 Unity/C# 开发的第三人称射击与丧尸塔防游戏，玩家参与射击战斗，通过建造、升级防御塔抵御尸潮并保护主塔。项目实现了英雄展示与长按解锁的金币数字动态反馈、关卡选择、通过异步加载场景与进度条衔接关卡、战斗、胜负结算和进度保存。

采用Input System 处理玩家输入，结合 CharacterController 实现角色移动与重力，通过摄像机与枪口双射线检测实现瞄准和射击命中判定；结合 NavMesh、Animator 与动画事件实现敌人寻路和攻击判定；使用 ScriptableObject 配置与数据库管理英雄、怪物、防御塔及关卡数据，通过泛型事件中心解耦业务与 UI，利用对象池复用怪物、音效和特效。

构建 GameDataMgr 管理玩家进数据、英雄选择、ScriptableObject配置访问及音频资源，GameLevelMgr 管理关卡初始化、波数、僵尸对象和奖励结算；通过 UIMgr＋BasePanel 实现面板管理与淡入淡出，配合 ResMgr、MonoMgr 提供资源加载和协程支持，并使用 JSON 存档持久保存金币、已解锁英雄及音频设置实现数据持久化。

**技术栈：** Unity3D · C# · Input System · NavMesh · Animator · UGUI · ScriptableObject · Coroutine · JSON / LitJson · Physics.Raycast / OverlapSphereNonAlloc · Coroutine


[快速运行](#快速运行) · [核心玩法](#核心玩法) · [模块与源码导航](#模块与源码导航) · [关键实现](#关键实现) · [操作说明](#操作说明)



## 核心技术实现

| 模块 | 实现要点 |
| --- | --- |
| 角色控制与战斗判定 | 基于 Input System 与 CharacterController 实现移动、重力和瞄准控制；采用摄像机与枪口双射线对齐射击方向，使用 OverlapSphereNonAlloc 实现近战范围检测，通过动画事件同步攻击时机 |
| 第三人称摄像机 | 实现肩侧跟随、俯仰角限制、位置与旋转平滑，以及瞄准状态下的镜头偏移和 FOV 过渡 |
| 事件驱动通信 | 基于字典、泛型与委托实现事件中心，提供参数类型校验及监听生命周期管理，解耦战斗状态、关卡逻辑与 UI 刷新 |
| 对象池与生命周期管理 | 以预制体引用区分对象池，采用“未激活取出—状态重置—激活”流程复用怪物、音效和特效，处理状态残留、重复回收与失效引用 |
| 配置与数据管理 | 使用 ScriptableObject 和配置数据库组织角色、怪物、防御塔及关卡数据；通过 GameDataMgr 管理全局数据，结合 JSON 保存玩家进度与音频设置 |
| 关卡流程与结算 | GameLevelMgr 统一管理关卡初始化、波次推进、战斗对象与胜负判定；分离死亡奖励和动画退场时机，通过结束状态标记避免重复结算 |
| UI 与异步加载 | UIMgr 通过泛型接口统一管理面板，BasePanel 封装淡入淡出与关闭回调；场景异步加载结合进度平滑显示，在加载完成后初始化关卡 |
| 英雄解锁交互 | 结合 EventTrigger 与协程实现可取消的长按解锁，同步进度和金币数字反馈；分离过程展示与实际扣款，完成后记录英雄 ID 并持久保存 |



## 核心玩法

- **英雄选择与购买：** 左右切换英雄，查看名称与介绍，拖拽旋转展示模型；长按解锁按钮购买付费英雄，免费英雄可直接使用。
- **第三人称射击：** 移动、鼠标视角、右键瞄准、单发 / 连发切换、翻滚与下蹲动画控制；射击命中反馈包含音效和特效；近战使用范围碰撞检测。
- **防御塔建造与升级：** 靠近造塔点显示候选塔，消耗局内金币建造或升级；支持单体攻击与范围攻击，满级后关闭升级选项。
- **尸潮防守：** 出生点按配置分波生成怪物；怪物完成出生动画后寻路，接近主塔后通过攻击动画事件造成伤害。
- **HUD 与菜单：** 展示金币、主塔血条、剩余波数与动态准星；包含开始、英雄选择、关卡选择、设置、加载、暂停、提示和结算面板。
- **奖励与成长：** 击杀怪物获得局内金币；胜利结算全部剩余局内金币，失败结算 20%，奖励进入局外钱包并存档。

## 模块与源码导航

### 基础管理模块

| 模块 | 职责与实现 | 源码 |
| --- | --- | --- |
| EventCenter | 字符串事件名对应委托容器；无参 / 泛型订阅、触发、退订；空参数检查、类型冲突检查及空监听清理 | [EventCenter.cs](Assets/Scripts/Manager/EventCenter.cs) |
| PoolMgr / PoolData | 字典组织不同对象池，列表保存待用实例；预制体引用键、未激活取出、重复回收保护、场景池清理；保留字符串资源接口 | [PoolMgr.cs](Assets/Scripts/Manager/PoolMgr.cs) |
| PoolSound | 配置 AudioSource 后激活播放，播放结束清空音频引用并归还原池 | [PoolSound.cs](Assets/Scripts/Manager/PoolSound.cs) |
| PoolEffect | 支持世界坐标和父节点两种生成方式；重置变换和残留粒子，按持续时间回收 | [PoolEffect.cs](Assets/Scripts/Manager/PoolEffect.cs) |
| ResMgr | 封装 Resources 同步 / 异步加载，区分资源引用与实例化，提供泛型回调和预制体实例化接口 | [ResMgr.cs](Assets/Scripts/Manager/ResMgr.cs) |
| MonoMgr / MonoController | 为普通 C# 管理类提供 MonoBehaviour 协程入口与逐帧委托；控制对象跨场景保留 | [MonoMgr.cs](Assets/Scripts/Manager/MonoMgr.cs) |
| BaseManager | 泛型单例基类，通过非公开无参构造函数创建管理器 | [BaseManager.cs](Assets/Scripts/Manager/BaseManager.cs) |
| JsonMgr | 封装 JSON 文件读写，支持 LitJson / JsonUtility，数据不存在时创建默认对象 | [JsonMgr.cs](Assets/Scripts/Manager/JsonMgr/JsonMgr.cs) |

### 数据与关卡管理

| 模块 | 职责与实现 | 源码 |
| --- | --- | --- |
| GameDataMgr | 加载玩家和音乐数据，保存设置与进度，记录选中英雄，提供配置数据库入口；缓存 AudioClip，并通过对象池播放音效 | [GameDataMgr.cs](Assets/Scripts/Manager/GameDataMgr.cs) |
| GameLevelMgr | 初始化英雄、摄像机、起始金币和主塔血量；维护出生点 / 怪物 / 防御塔列表；范围寻敌、波数通知、击杀奖励、统一胜负结算和退出清理 | [GameLevelMgr.cs](Assets/Scripts/Manager/GameLevelMgr.cs) |
| PlayerData / MusicData | 局外钱包、已拥有英雄 ID，以及音乐 / 音效开关和音量 | [PlayerData.cs](Assets/Scripts/Data/PlayerData.cs)、[MusicData.cs](Assets/Scripts/Data/MusicData.cs) |
| SingleScriptableObject | 从 Resources/ScriptableObject 按类型名称加载配置数据库 | [SingleScriptObject.cs](Assets/Scripts/Configs/SingleScriptObject.cs) |
| HeroConfig / HeroDatabase | 英雄属性、解锁价格、预制体和相关资源；提供集合及 ID 查询 | [英雄配置](Assets/Scripts/Configs/Hero) |
| MonsterConfig / MonsterDatabase | 怪物战斗、移动属性、预制体及动画控制器配置 | [怪物配置](Assets/Scripts/Configs/Monster) |
| TowerConfig / TowerDatabase | 建造费用、攻击类型 / 范围 / 间隔、资源与下一级塔引用 | [防御塔配置](Assets/Scripts/Configs/Tower) |
| LevelConfig / LevelDatabase | 场景名称、预览、介绍、初始金币和主塔生命值 | [关卡配置](Assets/Scripts/Configs/Level) |
| MonsterSpawnConfig | 波数、每波数量、首波延迟、波间隔、生成间隔和可生成怪物 | [刷怪配置](Assets/Scripts/Configs/MonsterSpawn) |

配置资源位于 [Assets/Resources/ScriptableObject](Assets/Resources/ScriptableObject)。ScriptableObject 保存设计配置，JSON 保存玩家运行产生的持久化数据。

### UI 与交互

| 模块 | 职责与实现 | 源码 |
| --- | --- | --- |
| UIMgr | 泛型 ShowPanel / GetPanel / HidePanel；按类名加载同名预制体，以字典管理已打开面板，共用跨场景 Canvas | [UIMgr.cs](Assets/Scripts/Manager/UIMgr.cs) |
| BasePanel | 统一 Awake、Init、ShowMe、HideMe；CanvasGroup 淡入淡出，隐藏完成后回调释放面板 | [BasePanel.cs](Assets/Scripts/Manager/BasePanel.cs) |
| BeginPanel / Main | 游戏入口与开始菜单，连接选角、设置、说明和退出流程 | [Main.cs](Assets/Scripts/Main.cs)、[BeginPanel.cs](Assets/Scripts/BeginScene/UI/BeginPanel.cs) |
| ChooseHeroPanel | 英雄预览、拖拽旋转、长按购买、取消恢复、解锁状态及钱包显示 | [ChooseHeroPanel.cs](Assets/Scripts/BeginScene/UI/ChooseHeroPanel.cs) |
| ChooseLevelPanel | 关卡切换与预览；异步加载完成后初始化关卡 | [ChooseLevelPanel.cs](Assets/Scripts/BeginScene/UI/ChooseLevelPanel.cs) |
| LoadingPanel | 异步场景加载、进度归一化和平滑显示、控制场景激活，使用非缩放时间支持暂停期间加载 | [LoadingPanel.cs](Assets/Scripts/BeginScene/UI/LoadingPanel.cs) |
| GamePanel / TowerItem | HUD 事件订阅及初始状态同步；准星平滑缩放、造塔选项、快捷键与升级信息 | [GamePanel.cs](Assets/Scripts/GameScene/UI/GamePanel.cs)、[TowerItem.cs](Assets/Scripts/Data/TowerItem.cs) |
| PausePanel / TipPanel | 暂停与继续、返回菜单、信息提示及相应输入 / 鼠标状态控制 | [PausePanel.cs](Assets/Scripts/GameScene/UI/PausePanel.cs)、[TipPanel.cs](Assets/Scripts/BeginScene/UI/TipPanel.cs) |
| GameOverPanel | 展示胜负与奖励、关闭游戏 HUD、返回开始场景；发钱和存档由 GameLevelMgr 负责 | [GameOverPanel.cs](Assets/Scripts/GameScene/UI/GameOverPanel.cs) |
| SettingPanel / BKMusic | 音乐开关与音量通过事件即时同步；音效设置在后续播放时读取，关闭设置面板时保存 | [SettingPanel.cs](Assets/Scripts/BeginScene/UI/SettingPanel.cs)、[BKMusic.cs](Assets/Scripts/BeginScene/BKMusic.cs) |
| CameraAnimator | 菜单镜头转场，动画完成时执行界面切换回调 | [CameraAnimator.cs](Assets/Scripts/BeginScene/Camera/CameraAnimator.cs) |

### 战斗对象

| 脚本 | 实现 |
| --- | --- |
| [PlayerObject](Assets/Scripts/GameScene/Object/Player/PlayerObject.cs) | 输入生命周期、移动与重力、视角、瞄准、射击模式、动画参数、命中判定、局内金币通知 |
| [CameraMove](Assets/Scripts/GameScene/Object/Camera/CameraMove.cs) | LateUpdate 跟随、肩侧偏移、俯仰限制、位置 / 旋转平滑、瞄准偏移和 FOV 过渡 |
| [MonsterBornPoint](Assets/Scripts/GameScene/Object/Monster/MonsterBornPoint.cs) | 根据配置分波生成怪物，登记后初始化并激活；提供出怪完成判断 |
| [MonsterObj](Assets/Scripts/GameScene/Object/Monster/MonsterObj.cs) | 出生、寻路、攻击、受伤、死亡与动画事件；复用初始化和死亡通知 |
| [TowerBulidPoint](Assets/Scripts/GameScene/Object/Tower/TowerBulidPoint.cs) | 进入 / 离开造塔点通知、费用判断、创建或替换升级塔、满级关闭 |
| [TowerObject](Assets/Scripts/GameScene/Object/Tower/TowerObject.cs) | 按范围选择目标、炮台转向、攻击间隔控制、单体 / 范围伤害与特效 |
| [MainTowerObject](Assets/Scripts/GameScene/Object/Tower/MainTowerObject.cs) | 主塔血量、伤害、血量变化通知，以及归零后的摧毁事件 |

## 关键实现

### 1. 长按解锁：事件触发器 + 协程 + 持久化

`ChooseHeroPanel` 使用 Unity UI 的 `EventTrigger` 接收 `PointerDown`、`PointerUp` 和 `PointerExit`。这与自定义的全局 `EventCenter` 是两套不同职责的机制。

1. 按下时缓存钱包和价格，启动购买协程。
2. 协程用 `Time.unscaledDeltaTime` 累加 2 秒进度，每帧同步按钮填充比例、预计剩余金币和剩余价格。
3. 松开或移出时停止协程并恢复显示，过程中的数字变化仅用于展示。
4. 长按完成后才真正扣款，把 `nowHero.Id` 加入 `PlayerData.haveHero`，调用 `SavePlayerData()`。
5. 刷新解锁状态，将购买按钮切换为开始按钮，并显示成功提示。

### 2. 对象池：先初始化，再激活

怪物、音效和特效通过预制体引用取得对应池中的实例。对象池根节点保持未激活，新实例不会过早触发 OnEnable；取出后由调用方配置并激活。

- **怪物：** 重置血量、死亡 / 出生标记、碰撞器、Animator 和 NavMeshAgent，避免上一次生命周期状态残留。
- **音效：** 设置音频片段和音量，播放结束自动回收。
- **特效：** 恢复位置 / 旋转 / 缩放，清除旧粒子，再开始新一轮播放。
- **池本身：** 拦截同一实例重复回收，跳过被外部销毁的缓存对象，场景根节点失效时清理旧索引。

### 3. 事件中心：业务通知与表现解耦

事件中心采用 `Dictionary<string, IEventInfo>`，由 `EventInfo<T>` 保存 `UnityAction<T>`。同名事件参数类型不一致会明确报错；最后一个监听移除后清理对应条目。

| 事件 | 发布方 → 主要接收方 |
| --- | --- |
| 金币变化 | PlayerObject → GamePanel |
| 主塔血量变化 | MainTowerObject → GamePanel |
| 僵尸波数变化 | GameLevelMgr → GamePanel |
| 造塔点更新 / 关闭 | TowerBulidPoint → GamePanel |
| 瞄准状态变化 | PlayerObject → GamePanel |
| 音乐开关 / 音量变化 | SettingPanel → BKMusic |
| 怪物死亡 / 退场 | MonsterObj → GameLevelMgr |
| 主塔摧毁 | MainTowerObject → GameLevelMgr |

面板在 OnEnable / OnDisable 成对订阅和退订，关卡管理器在初始化 / 清理时管理监听。持续状态通过读取当前数据补齐初始显示。事件同步执行；输入回调、动画事件、资源加载回调与直接业务操作保留各自职责。



### 4. UI 与异步加载

`UIMgr` 负责面板查找、创建和销毁，`BasePanel` 负责淡入淡出与隐藏完成回调。暂停和结算面板直接设置可见，避免 Time.timeScale 为 0 时淡入停住。

场景加载把 Unity 的 0～0.9 加载进度映射到 0～1，再使用非缩放时间平滑推进显示；进度完成后激活场景，等待加载结束才调用关卡初始化回调。

### 5. 射击与近战判定

枪械攻击先从摄像机屏幕中心发射射线确定瞄准点，再从枪口向该位置发射射线，使肩侧镜头与枪口命中方向对齐；末端增加距离补偿，降低表面命中的浮点误差。当前两条射线只检测 Monster 层，环境遮挡判定尚待扩展。

近战通过 OverlapSphereNonAlloc 复用碰撞器数组，按实际命中数量遍历；命中有效目标后播放特效并结算伤害。攻击判定由动画事件触发，使伤害时机与动作配合。

## 快速运行

### 环境

- **Unity Editor：6000.4.2f1**，与 [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt) 一致。
- **Git + Git LFS**：大型美术资源通过 LFS 保存，克隆后需要取得实际文件。
- 主要包：Input System 1.19.0、AI Navigation 2.0.14、UGUI 2.0.0、Timeline 1.8.12；完整依赖见 [manifest.json](Packages/manifest.json)，锁定版本见 [packages-lock.json](Packages/packages-lock.json)。


### 获取与启动

```bash
git lfs install
git clone https://github.com/boshizhai/Unity-Zombie-Tower-Defense.git
cd Unity-Zombie-Tower-Defense
git lfs pull
```

1. 在 Unity Hub 中添加克隆后的工程根目录，使用上述版本打开。
2. 等待 Package Manager 恢复依赖以及资源导入完成，首次导入大型美术资源需要时间。
3. 打开 **Assets/Scenes/BeginScene.unity** 并进入 Play Mode。
4. 从开始菜单选择英雄、选择第一个关卡，进入战斗。
5. 如需运行其他关卡，先检查 Build Profiles 的场景列表：当前提交的 EditorBuildSettings 仅启用 BeginScene 和 GameScene1；配置中还保留 GameScene2 / GameScene3。

不要直接从战斗场景启动作为首次试玩入口：英雄选择和关卡初始化由菜单流程完成。不要删除 .meta 文件，场景和预制体依赖其中的 GUID。

下载 ZIP 时务必确认包含真实 LFS 资源；推荐使用上面的克隆命令，避免只取得资源指针。

### 存档

局外金币、英雄 ID 列表与音乐设置保存在 `Application.persistentDataPath` 下的 PlayerData.json / MusicData.json；不是写回 ScriptableObject 配置。没有存档时创建默认数据，初始钱包为 1000，免费英雄无需购买。


## 操作说明

玩家操作定义来自 [GameInputActions.inputactions](Assets/Resources/Input/GameInputActions.inputactions)。

| 操作 | 输入 |
| --- | --- |
| 移动 | W / A / S / D |
| 调整视角 | 鼠标移动 |
| 射击 | 鼠标左键；连发模式下按住 |
| 瞄准 | 鼠标右键 |
| 切换单发 / 连发 | F |
| 翻滚 | Alt |
| 下蹲 | 左 Shift |
| 暂停 / 继续 | **Backspace** |
| 选择建造塔 | 1 / 2 / 3，需要靠近可用造塔点 |
| 升级当前塔 | 空格，需要靠近可升级造塔点 |


## 工程目录

```text
Assets/
├── Scripts/
│   ├── Manager/          # 事件、对象池、资源、UI、数据、关卡、JSON
│   ├── Configs/          # ScriptableObject 配置与数据库
│   ├── Data/             # 玩家 / 音乐数据、塔选项
│   ├── BeginScene/       # 菜单、长按购买、加载、设置、菜单镜头
│   ├── GameScene/        # 玩家、怪物、防御塔、战斗镜头与 HUD
│   └── Main.cs           # 开始菜单入口
├── Resources/            # 运行时加载的预制体、输入与配置资源
├── Scenes/               # 开始场景与战斗场景
├── StreamingAssets/      # 保留的早期 JSON 配置
└── ArtRes/               # 第三方场景、角色、材质和特效等资源
Packages/                 # Unity 包清单与版本锁定
ProjectSettings/          # 项目、输入、层与场景设置
```


## 资源说明

程序入口集中在 Assets/Scripts；第三方资源及其示例脚本集中在 Assets/ArtRes 等目录。LitJson 与第三方资源的版权、许可归原作者，仓库公开不代表这些资源可以脱离原许可任意再分发。保留资源附带的说明和许可文件。

仓库包含打开工程所需的源资源、场景、预制体、配置及 .meta；Library、Temp、Logs、IDE 缓存、构建输出以及备用 .unitypackage 导入包不提交。演示视频链接后续补充到本文顶部。


