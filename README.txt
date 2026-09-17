WeatherController.Client
一个用于 SPT 5.0 (IL2CPP) 的 BepInEx 客户端插件，让你在战局内用快捷键即时改写天气与季节。

绕过游戏的动态天气系统，直接写入 WeatherDebug 参数，覆盖云量、雾、降雨、雷暴概率、温度、风向等；同时支持季节切换（夏 / 秋 / 冬）与雨声静音。


✨ 功能特性
实时天气改写：云量、雾、雨、雷暴概率、温度、风速、风向、高层风

季节切换：夏 / 秋 / 冬，含冬季专属的雪花与粒子效果，季节切换在单局内为一次性，以避免一些引擎产生的近远景使用景观不一致的问题

雨声静音：切到冬季后将静音下雨音效，并持续纠偏（引擎会周期性重置 AudioSource.mute）

⚠️ 已知问题：在下雨时切换冬季，能正确下雪，但屏幕会有雨点

📦 安装
确认已安装 BepInEx 6 (IL2CPP) 与 SPT 5.0。

下载最新 Release 中的 WeatherController.Client.dll。

放到：

<SPT 根目录>\BepInEx\plugins\WeatherController.Client\WeatherController.Client.dll
启动游戏。BepInEx 控制台应输出：

🎮 使用方法
按住 左 Alt 或右 Alt，再按小键盘数字键：

快捷键	预设	说明
Alt + Num 0	晴天	云量 -0.2，雾 0.004，无雨，22℃
Alt + Num 1	阴天	云量 0.5，雾 0.006，无雨，18℃
Alt + Num 2	小雨	仅修改降雨 = 0.3
Alt + Num 3	大雨	近修改降雨 = 0.9
Alt + Num 4	小雾	仅修改雾气 = 0.01
Alt + Num 6	大雾	仅修改雾气 = 0.05
Alt + Num 7	夏	切换季节到夏季，22℃
Alt + Num 8	秋	切换季节到秋季，10℃
Alt + Num 9	冬	切换季节到冬季，-10℃ + 静音雨声

组合示例
太阳雨：先按 Alt + Num 0（晴天），再按 Alt + Num 2/3（下雨）

冬日暴雪：按 Alt + Num 9（冬），再按 Alt + Num 3（大雨 → 大雪）

⚠️ 已知限制
冬季锁定：一旦切到冬季，本战局内无法切回其他季节。引擎不支持来回切换季节，反复切会破坏粒子/材质状态。退出战局后自动恢复正常。

藏身处 / 工厂：这些场景没有动态天气系统，天气键无效（会输出警告日志）。

仅限 SPT 5.0：依赖该版本的 Assembly-CSharp.dll 与 interop 接口，其他版本不保证兼容。

🔧 从源码构建
.NET SDK 6.0+

PT 5.0 已至少启动过一次（生成 BepInEx\interop 程序集）

构建
bash
# 方式一：使用批处理脚本（会自动复制到 plugins 目录）
build.bat

# 方式二：手动指定 SPTPath
dotnet build WeatherController.Client.csproj -c Release /p:SPTPath="D:\SPT-5.0.0-47242-BE"
SPTPath 默认为 D:\SPT-5.0.0-47242-BE，可通过环境变量或 MSBuild 参数覆盖：

bash
set SPTPath=E:\MySPT
build.bat
构建产物会自动复制到：

text
%SPTPath%\BepInEx\plugins\WeatherController.Client\
🧩 项目结构
文件	作用
Plugin.cs	BepInEx 入口，应用 Harmony 补丁并挂载 Behaviour
WeatherPresets.cs	预设表与预设类型定义（Full / RainOnly / FogOnly / SeasonOnly）
WeatherHotkeyBehaviour.cs	监听 Alt + 小键盘，写入 WeatherDebug 参数
SeasonOverride.cs	季节切换、state 缓存、冬季锁定、雨声静音
WeatherAudioTick.cs	定期扫描并纠偏雨声静音（约 1 秒一次）
ServerClient.cs	把玩家选择 fire-and-forget 地 POST 给本机服务端
build.bat	一键构建 + 部署脚本
🛠 技术细节
季节覆盖通过 Harmony Postfix 补丁 SeasonsController.get_Season 实现，让游戏逻辑读到的季节与视觉一致。

天气写入前会强制打开 WeatherDebug.Enabled，否则引擎会忽略所有写入。

雨声静音采用轮询式纠偏：引擎会周期性重置 AudioSource.mute，所以 WeatherAudioTick 每约 1 秒扫描一次全场景 AudioSource，匹配 clip 名或节点路径中含 rain / storm / thunder 的源。

组合天气的关键是 PresetKind 的字段级覆盖：RainOnly 只写 Rain，FogOnly 只写 Fog，未设置的字段保持原值。

🙏 致谢
timeweatherchanger mod 用于参考游戏接口
SPT

