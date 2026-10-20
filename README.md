# 星屿渔夫 · Star Isle Fisher

[English](#english-manual) | [简体中文](#chinese-manual)

<a id="english-manual"></a>

# English manual

Star Isle Fisher is a Windows fishing companion for CELESTAY. Set a cast limit and a time limit, then let the game continue skill fishing in the background while you use other applications.

The current controller version is **0.2.4**, and the in-game plugin version is **0.2.3**. The project grew from an automatic fishing script into a game utility. It is not an official CELESTAY product.

The application currently uses Chinese interface labels. The English instructions below include those labels so you can find the corresponding controls.

## Features

- Background fishing without moving the mouse or simulating keyboard input; supports switching applications and minimizing the game.
- Skill fishing at 3–100 m with up to four timing checks, tracking moving blue targets and prioritizing the orange perfect zone.
- Limits of up to 99,999 casts and 480 minutes; fishing stops when either limit is reached.
- Separate counters for catches, perfect catches, failures and casts.
- Automatic Steam library detection, with manual game path selection when needed.
- A pixel-art interface with a seaside background, fisherman icon and separate settings panel.

## Requirements

Use Windows 10/11, a 64-bit installation of CELESTAY, and a game version compatible with the plugin. The installer checks the game files; a game update may require a plugin update.

The release package does not require Python. The tool does not call model services at runtime. Keep your computer powered on and awake, and keep the game running.

## First-time setup

1. Download `StarIsleFisher-0.2.4-Windows.zip`, extract the entire package, and open `CelestayFishing.exe`. Do not run it inside the archive or keep only the EXE.
2. Exit CELESTAY normally, then open **Cabin settings (小屋设置)** in the upper-right corner of the controller.
3. Check the detected game path. If no path was found, click **Select game (选择游戏)** and select `CELESTAY.exe` in your game installation directory.
4. Click **Install components (安装组件)**. If an existing loader or conflicting files are detected, installation stops without overwriting them.
5. Launch CELESTAY from your **Steam library** and load your save.
6. Select skill fishing at 3–100 m in the game, save the settings, and close the fishing settings panel.
7. Return to Star Isle Fisher, check that the plugin is connected at the bottom of the window, and start with the default **5 casts / 3 minutes** for a short trial.

**Launch instructions (启动说明)** only displays guidance. It does not launch the game EXE directly.

## Starting and stopping

Set **Maximum casts (最多抛竿)** and **Maximum duration in minutes (最长停留)**, then click **Start fishing (开始垂钓)**. You can use other applications or minimize the game while it runs. The start button is disabled during a session to prevent an accidental restart from resetting the counters.

Click **Stop (收竿 · 停止)** or press **F8** to stop. **Closing the controller also stops fishing**; minimize it if you want fishing to continue. Restarting the game or controller does not automatically resume the previous session.

Each cast still consumes the resources normally required by the game. Opening fishing settings, pausing the game, running out of resources, or making no progress for more than 30 seconds may stop the session. Check the reason shown below the controls.

## Understanding the counters

| Label | Meaning |
| --- | --- |
| Catches (捕获) | Completed casts for which a reward has been confirmed |
| Perfect catches (完美) | Catches completed with a perfect judgment; these are already included in Catches |
| Failures (失败) | Recorded failed timing attempts |
| Casts (抛竿) | Attempts started in this session, which may include a cast that has not yet finished |
| Checks for this cast (本竿判定) | Completed timing checks and the number required for the current cast |

Fishing at 3–15 m supports one check; 16–20 m supports two. At 21–100 m, the plugin reads the game's configuration and supports up to four checks. Passing one stage does not count as a separate fish: a catch is counted only when the whole cast settles.

## Updating the controller and plugin

| Current version | Update steps |
| --- | --- |
| Controller or plugin 0.2.3 | Stop the session, close the old controller, and open the EXE from the 0.2.4 package. No in-game plugin replacement or game restart is needed |
| Plugin 0.2.0, 0.2.1 or 0.2.2 | Exit the game and old controller normally. Open the new controller, select **Update plugin (更新插件)** in **Cabin settings (小屋设置)**, then relaunch the game through Steam |

The controller and in-game plugin can have different versions. Version 0.2.4 primarily updates the interface and icon and continues to use plugin 0.2.3.

## Uninstalling

Exit the game normally, open **Cabin settings (小屋设置)**, and click **Uninstall components (卸载组件)**. Uninstallation removes only files recorded by the installer whose contents have not changed. Saves and original game files are preserved. Loader-generated logs, caches or modified configuration files may remain.

If installation or uninstallation reports other components, review the message before proceeding so that other plugins are not overwritten.

## Compatibility and feedback

Actual gameplay records cover shallow fishing, two-stage fishing and four checks at 100 m. There is also usage feedback for switching applications, minimizing the game and moving targets. Depth, frame rate, hardware and game version can affect results; these records do not guarantee success on every cast.

The current plugin has not completed a fully documented eight-hour run. The lock-screen scenario has not been verified either. The reward window may remain open after the final cast reaches a limit; you can close it manually.

For stops or errors, see [Troubleshooting](docs/TROUBLESHOOTING.md). Include the controller version, plugin version, fishing depth, stop reason and reproduction steps when reporting a problem. Hide personal information in screenshots. See [Compatibility and validation records](docs/COMPATIBILITY.md) for the verification boundaries. These supporting documents are currently in Chinese.

## Files and resource usage

The tool does not create a screenshot for each cast. It writes status to fixed files and keeps at most one temporary status file. The background image and icon load when the window opens and do not accumulate with the cast count.

The game and loader may still generate their own logs. The tool uses memory and does not prevent the game's own memory or log usage from growing. The release package contains no game binaries, saves or personal game paths.

## Source code and license

See [Local build instructions](docs/BUILD.md) and the [Changelog](CHANGELOG.md), currently in Chinese. The project was developed with AI assistance and uses the game's existing fishing handlers for in-game actions.

Project-owned source code and documentation are licensed under the [MIT License](LICENSE), with copyright attribution to [S-Fisher-Yu](https://github.com/S-Fisher-Yu). Third-party components retain their own licenses; see [Third-party notices](THIRD_PARTY_NOTICES.md) and `licenses/`. Artwork provenance is documented in [Asset notes](background/assets/README.md). The code license does not transfer rights to the game name, game assets or third-party materials.

[简体中文 ↓](#chinese-manual) · [Back to English ↑](#english-manual)

<a id="chinese-manual"></a>

# 简体中文手册

星屿渔夫是 CELESTAY 的 Windows 钓鱼辅助工具。设置次数和时长后，可以让游戏在后台继续技巧钓鱼，同时使用其他软件。

当前控制程序版本为 **0.2.4**，游戏内插件版本为 **0.2.3**。本项目由一个自动钓鱼脚本逐步发展而来，是作者制作的游戏小插件。它不是 CELESTAY 官方产品。

## 支持的功能

- 后台钓鱼：不移动鼠标、不模拟键盘输入，支持切换窗口和最小化游戏。
- 技巧判定：支持 3–100 米、最多四次判定，跟随移动蓝条，优先命中橙色完美区。
- 运行限制：最多 99,999 次、480 分钟，任一上限先到即停止。
- 渔获统计：分别显示捕获、完美、失败和抛竿次数。
- 路径查找：自动查找 Steam 游戏库，也可手动选择游戏位置。
- 像素界面：海景背景、草帽渔夫图标和独立设置面板。

## 使用前准备

需要 Windows 10/11、64 位 CELESTAY，以及与当前插件兼容的游戏版本。程序会检查游戏文件版本；游戏更新后可能需要等待插件适配。

使用发布包不需要安装 Python。程序运行时不调用模型服务。电脑需要保持开机和唤醒，游戏进程需要持续运行。

## 首次安装

1. 下载发布包 `StarIsleFisher-0.2.4-Windows.zip`，完整解压后打开 `CelestayFishing.exe`。不要在压缩包内直接运行，也不要只保留 EXE。
2. 正常退出 CELESTAY，打开右上角的 **小屋设置**。
3. 确认自动找到的游戏路径。如果没有找到，点击 **选择游戏**，选择游戏安装目录中的 `CELESTAY.exe`。
4. 点击 **安装组件**。检测到现有加载器或冲突文件时，程序会停止安装，不会直接覆盖。
5. 从 **Steam 游戏库**启动 CELESTAY，进入存档。
6. 在游戏中选择 3–100 米技巧钓鱼，保存并关闭钓鱼设置面板。
7. 回到星屿渔夫，确认底部显示插件已连接，先用默认的 **5 次 / 3 分钟**运行一轮。

**启动说明**只显示操作提示，不会直接启动游戏 EXE。

## 开始与停止

在主界面填写 **最多抛竿**和 **最长停留**，点击 **开始垂钓**。运行期间可以使用其他软件或最小化游戏；开始按钮会暂时禁用，避免重复启动重置计数。

点击 **收竿 · 停止**或按 **F8**可以停止。**关闭控制窗口也会停止钓鱼**；需要继续运行时，请最小化控制窗口。重新启动游戏或控制程序不会自动恢复上一轮任务。

工具仍会正常消耗游戏中每竿所需的资源。打开钓鱼设置、暂停游戏、资源不足或超过 30 秒没有进展时，任务可能停止；请查看界面下方的原因。

## 读懂渔获统计

| 项目 | 含义 |
| --- | --- |
| 捕获 | 已确认获得奖励的整竿数量 |
| 完美 | 捕获中以完美判定完成的数量，不需要再加到捕获数上 |
| 失败 | 已记录的判定失败数量 |
| 抛竿 | 本轮已开始的尝试数量，可能包含尚未结算的一竿 |
| 本竿判定 | 当前这一竿已完成和需要完成的判定数 |

3–15 米支持一次判定，16–20 米支持两次判定；21–100 米读取游戏当前配置，支持最多四次判定。每段命中不会单独计为一条鱼，整竿结算后才计入捕获。

## 更新程序与插件

| 当前版本 | 更新方式 |
| --- | --- |
| 已使用控制程序或插件 0.2.3 | 停止任务并关闭旧控制窗口，打开 0.2.4 发布包中的 EXE。无需更换游戏内插件，也无需重启游戏 |
| 插件仍为 0.2.0、0.2.1 或 0.2.2 | 正常退出游戏和旧控制窗口，打开新版，在 **小屋设置**中点击 **更新插件**，然后从 Steam 重新启动游戏 |

控制程序和游戏内插件的版本可以不同。0.2.4 主要更新界面与图标，继续使用 0.2.3 钓鱼插件。

## 卸载

正常退出游戏，在 **小屋设置**中点击 **卸载组件**。卸载仅删除安装记录中内容未被修改的文件，保留存档和游戏原文件。加载器生成的日志、缓存或已修改配置可能残留。

如果安装或卸载提示存在其他组件，请先检查提示内容，避免覆盖其他插件。

## 兼容范围与反馈

目前已有浅水、双次判定和 100 米四次判定的实际运行记录，也有切换窗口、最小化和移动目标的使用反馈。深度、帧率、电脑环境和游戏版本都会影响实际表现，不能据此保证每次都成功。

新版尚未完成一次有完整记录的连续八小时验证；锁屏场景也未验证。最后一竿达到上限后，奖励画面可能保留，手动关闭即可。

如果出现停止或异常，请先查看 [故障排查](docs/TROUBLESHOOTING.md)。反馈时附上程序版本、插件版本、钓鱼深度、停止原因和复现步骤；截图请遮住个人信息。详细验证边界见 [兼容性与验证记录](docs/COMPATIBILITY.md)。

## 文件与资源占用

工具不会按每一竿生成截图。状态写入固定文件，临时状态文件最多一份；界面背景和图标在打开窗口时加载，不随抛竿次数累积。

游戏和加载器仍可能生成自己的日志。程序不是零内存占用，也不会阻止游戏本身的内存或日志增长。发布包不包含游戏程序、存档或个人游戏路径。

## 源代码与许可

开发说明见 [本地构建](docs/BUILD.md)，版本变化见 [更新记录](CHANGELOG.md)。本项目在 AI 辅助下开发，游戏内操作通过原有钓鱼处理入口完成。

本项目自有源代码与文档采用 [MIT 许可](LICENSE)，作者署名为 [S-Fisher-Yu](https://github.com/S-Fisher-Yu)。第三方组件保留各自的许可声明，见 [第三方说明](THIRD_PARTY_NOTICES.md) 和 `licenses/`。背景与图标的制作来源见 [素材说明](background/assets/README.md)；游戏名称、游戏资源及第三方权利不因代码许可而转移。

[Back to English ↑](#english-manual)
