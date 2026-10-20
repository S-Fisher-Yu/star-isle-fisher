# 在 Windows 本地构建

本页适用于源码仓库。使用发行版的玩家直接运行发布包中的 EXE，不需要执行这些步骤。发布包内附带的平铺源码另见 `source/BUILD.md`。

## 准备编译器与本地引用

源代码采用 C# 5 语法，使用 Windows .NET Framework 编译器。控制程序引用系统自带的 Windows Forms、Drawing 和 Core 程序集。

游戏内插件还需要自己的 CELESTAY 安装，以及 [BepInEx 5.4.23.5 Windows x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5)。游戏的 `Assembly-CSharp.dll`、`UnityEngine.dll`、`UnityEngine.CoreModule.dll` 和 `netstandard.dll` 只用于本地引用，不得放入源代码仓库或发布包。

官方加载器压缩包的已记录 SHA-256：

```text
82f9878551030f54657792c0740d9d51a09500eeae1fba21106b0c441e6732c4
```

在仓库根目录打开 PowerShell，确认编译器存在并创建输出目录：

```powershell
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $csc)) { throw 'Missing .NET Framework compiler' }
New-Item -ItemType Directory -Path build -Force | Out-Null
```

## 编译控制程序

背景、图标和窗口资源均内嵌到 EXE。只编译控制界面不需要游戏文件：

```powershell
$controller = @(
  'background\SessionPolicy.cs', 'background\StatusFile.cs',
  'background\InstallerCore.cs', 'background\GameLocator.cs',
  'background\HarborTheme.cs', 'background\ControlApp.cs'
)
& $csc /nologo /target:winexe /out:build\CelestayFishing.exe `
  /reference:System.Windows.Forms.dll /reference:System.Drawing.dll `
  /reference:System.Core.dll /win32icon:background\assets\fisher.ico `
  /resource:background\assets\harbor.png,Harbor.png `
  /resource:background\assets\fisher.ico,Fisher.ico $controller
if ($LASTEXITCODE -ne 0) { throw 'Controller build failed' }
```

可运行 `build\CelestayFishing.exe --preview` 查看界面。预览模式不发送钓鱼、安装或停止指令。正常安装需要完整的 `payload/`，单独编译出的 EXE 不包含加载器安装文件。

## 编译游戏内插件

以下示例路径需要替换为本地游戏和加载器解压目录：

```powershell
$gameRoot = 'C:\Games\CELESTAY'
$loaderRoot = 'C:\Tools\BepInEx'
$managed = Join-Path $gameRoot 'CELESTAY_Data\Managed'
$references = @(
  "/reference:$loaderRoot\BepInEx\core\BepInEx.dll",
  "/reference:$managed\netstandard.dll",
  "/reference:$managed\UnityEngine.dll",
  "/reference:$managed\UnityEngine.CoreModule.dll",
  "/reference:$managed\Assembly-CSharp.dll"
)
& $csc /nologo /target:library /out:build\CelestayFishing.dll `
  $references background\SessionPolicy.cs background\StatusFile.cs `
  background\FishingPlugin.cs
if ($LASTEXITCODE -ne 0) { throw 'Plugin build failed' }
```

游戏版本兼容性由 `InstallerCore.GameHash` 校验。游戏更新后，应重新检查接口并实际测试，不能只改哈希值来绕过检查。

## 运行针对性检查

流程模拟使用测试替身，不引用游戏程序集，也不操作正在运行的游戏：

```powershell
& $csc /nologo /out:build\PluginFlowTests.exe `
  background\SessionPolicy.cs background\StatusFile.cs `
  background\FishingPlugin.cs tests\GameDoubles.cs tests\PluginFlowTests.cs
if ($LASTEXITCODE -ne 0) { throw 'Test build failed' }
& .\build\PluginFlowTests.exe
if ($LASTEXITCODE -ne 0) { throw 'Plugin flow tests failed' }
```

其他检查的源文件组合如下。每个测试文件都有独立入口，编译为控制台 EXE 后运行：

| 测试 | 额外源文件与引用 |
| --- | --- |
| `PolicyTests.cs` | `SessionPolicy.cs` |
| `StatusFileTests.cs` | `StatusFile.cs` |
| `GameLocatorTests.cs` | `GameLocator.cs`，引用 `System.Core.dll` |
| `UiStatusTests.cs` | 控制程序全部源文件和资源，指定 `/main:UiStatusTests /target:exe` |
| `InstallerTests.cs` | `SessionPolicy.cs`、`InstallerCore.cs`，参数为本地游戏 `Assembly-CSharp.dll` 路径 |

安装检查要求游戏已正常退出。它在独立临时目录创建模拟安装文件；不要从该目录启动游戏。测试替身不能编入实际插件。

## 组装发布包

发行包的核心布局如下：

```text
StarIsleFisher-0.2.4/
  CelestayFishing.exe
  README.md
  docs/
  licenses/
  THIRD_PARTY_NOTICES.md
  source/
  payload/
    winhttp.dll
    doorstop_config.ini
    .doorstop_version
    BepInEx/
      core/
      config/BepInEx.cfg
      plugins/CelestayFishing/CelestayFishing.dll
```

从已校验的官方加载器获取 `payload/` 的加载器文件，并保留第三方许可。把本项目插件放入自己的插件目录。不要打包游戏程序集、游戏存档、日志、`status.txt`、`command.txt` 或个人路径文件。

控制程序使用 `payload/` 安装组件。运行游戏只通过 Steam，由玩家主动启动。
