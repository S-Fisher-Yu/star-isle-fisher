using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace CelestayFishing
{
    class ControlApp : Form
    {
        readonly TextBox status=new TextBox();
        readonly NumericUpDown count=new NumericUpDown(), minutes=new NumericUpDown();
        readonly Timer timer=new Timer();
        readonly HarborTheme theme=new HarborTheme();
        readonly Label[] totals=new Label[4];
        readonly bool preview;
        Label connection, state, stage;
        Button startButton;
        Image scenery;
        Icon appIcon;
        string gamePath="";
        readonly string saved=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CelestayFishing","game-path.txt");
        string discoveryMessage="尚未找到游戏，请点击自动查找或手动选择 CELESTAY.exe。";
        string payload { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"payload"); } }
        internal ControlApp(string initial,bool previewMode=false)
        {
            preview=previewMode;
            Text="星屿渔夫 · CELESTAY"; AutoScaleDimensions=new SizeF(96,96); AutoScaleMode=AutoScaleMode.Dpi;
            ClientSize=new Size(760,600); Font=theme.Body; ForeColor=HarborTheme.Ink; BackColor=Color.FromArgb(145,202,218);
            DoubleBuffered=true;
            FormBorderStyle=FormBorderStyle.FixedSingle; MaximizeBox=false; StartPosition=FormStartPosition.CenterScreen;
            using(var stream=typeof(ControlApp).Assembly.GetManifestResourceStream("Fisher.ico"))
                if(stream!=null) { appIcon=new Icon(stream); Icon=appIcon; }
            using(var stream=typeof(ControlApp).Assembly.GetManifestResourceStream("Harbor.png"))
                if(stream!=null) using(var source=Image.FromStream(stream)) scenery=new Bitmap(source);
            BackgroundImage=scenery; BackgroundImageLayout=ImageLayout.Stretch;
            theme.Label(this,"CELESTAY  /  FISHING COMPANION",26,22,440,22,theme.Mono);
            theme.Label(this,"星屿渔夫",24,49,450,54,theme.Heading);
            theme.Label(this,"把时间留给自己，把渔获交给小屋。",27,108,450,26,theme.Body);
            AddButton(this,"小屋设置",634,22,102,36,ShowSettings,false);
            var card=new HarborPanel { Location=new Point(24,148),Size=new Size(474,414) }; Controls.Add(card);
            theme.Label(card,"本次渔获",20,18,200,26,theme.Body);
            state=theme.Label(card,"未连接",342,18,112,26,theme.Body); state.TextAlign=ContentAlignment.TopRight;
            string[] names={"捕获","完美","失败","抛竿"};
            for(int i=0;i<4;i++) {
                theme.Label(card,names[i],20+i*110,56,102,22,theme.Small).ForeColor=HarborTheme.Muted;
                totals[i]=theme.Label(card,"—",20+i*110,78,106,42,theme.Number);
            }
            card.Controls.Add(new Panel { Location=new Point(20,132),Size=new Size(434,1),BackColor=Color.FromArgb(220,199,160) });
            theme.Label(card,"最多抛竿 / 次",20,150,198,25,theme.Small);
            theme.Label(card,"最长停留 / 分钟",242,150,212,25,theme.Small);
            ConfigureNumber(card,count,20,178,198,99999,5,"最多抛竿次数");
            ConfigureNumber(card,minutes,242,178,212,480,3,"最长运行分钟");
            startButton=AddButton(card,"开始垂钓",20,230,272,44,delegate { InstallerCore.Command(gamePath,true,(int)count.Value,(double)minutes.Value); },true);
            AddButton(card,"收竿 · 停止",306,230,148,44,delegate { InstallerCore.Command(gamePath,false,0,0); },false);
            stage=theme.Label(card,"等待连接你的小屋",20,290,434,24,theme.Body);
            status.SetBounds(20,319,434,48); status.Multiline=true; status.ReadOnly=true;
            status.BorderStyle=BorderStyle.None; status.BackColor=HarborTheme.Paper; status.ForeColor=HarborTheme.Muted;
            status.Font=theme.Small; status.ScrollBars=ScrollBars.None; status.AccessibleName="当前状态详情"; card.Controls.Add(status);
            theme.Label(card,"F8 停止  ·  关闭窗口也会停止  ·  可最小化挂机",20,382,434,20,theme.Small).ForeColor=HarborTheme.Muted;
            connection=theme.Label(this,"",26,574,710,22,theme.Small);
            connection.BackColor=HarborTheme.Paper;
            if(!preview) {
                gamePath=GameLocator.ChoosePath(initial,ReadSavedGame(),new string[0]);
                if(gamePath.Length==0) LocateGame();
                timer.Interval=1000; timer.Tick+=delegate { RefreshStatus(); }; timer.Start(); RefreshStatus();
            } else SetDisconnected("界面预览","预览模式不会发送钓鱼、安装或停止指令。");
            FormClosing+=delegate { timer.Stop(); try { if(!preview && gamePath.Length>0) InstallerCore.Command(gamePath,false,0,0); } catch(Exception) { } };
        }
        void ConfigureNumber(Control parent,NumericUpDown input,int x,int y,int width,int maximum,int value,string name) {
            input.SetBounds(x,y,width,32); input.Minimum=1; input.Maximum=maximum; input.Value=value;
            input.BackColor=Color.FromArgb(255,253,244); input.ForeColor=HarborTheme.Ink; input.Font=theme.Body;
            input.AccessibleName=name; input.ThousandsSeparator=true; parent.Controls.Add(input);
        }
        void ShowSettings() {
            using(var settings=new Form { Text="星屿渔夫 · 小屋设置",Icon=Icon,ClientSize=new Size(584,352),Font=theme.Body,
                ForeColor=HarborTheme.Ink,BackColor=HarborTheme.Paper,FormBorderStyle=FormBorderStyle.FixedDialog,
                MaximizeBox=false,MinimizeBox=false,StartPosition=FormStartPosition.CenterParent }) {
                theme.Label(settings,"小屋设置",24,20,400,32,theme.Body);
                theme.Label(settings,"游戏位置",24,64,500,24,theme.Small);
                var path=new TextBox { Text=gamePath,ReadOnly=true,Location=new Point(24,92),Size=new Size(536,28),
                    BackColor=Color.FromArgb(255,253,244),AccessibleName="CELESTAY 游戏路径" }; settings.Controls.Add(path);
                AddButton(settings,"自动查找",24,136,128,36,delegate { LocateGame(); path.Text=gamePath; },false);
                AddButton(settings,"选择游戏",164,136,128,36,delegate {
                using(var dialog=new OpenFileDialog{Filter="CELESTAY|CELESTAY.exe",Title="选择游戏安装目录中的 CELESTAY.exe"})
                    if(dialog.ShowDialog(settings)==DialogResult.OK) { InstallerCore.Validate(dialog.FileName,false); gamePath=dialog.FileName; RememberGame(); path.Text=gamePath; }
                },false);
                AddButton(settings,"启动说明",432,136,128,36,delegate { MessageBox.Show(settings,"请从 Steam 游戏库启动 CELESTAY，进入存档后，设置 3–100 米技巧钓鱼并关闭设置面板，再点击「开始垂钓」。","出发之前"); },false);
                AddButton(settings,"安装组件",24,194,168,38,delegate { InstallerCore.Install(gamePath,payload); MessageBox.Show(settings,"安装完成。请从 Steam 启动游戏。","小屋已备好"); },false);
                AddButton(settings,"更新插件",208,194,168,38,delegate { InstallerCore.UpdatePlugin(gamePath,payload); MessageBox.Show(settings,"插件已更新。请从 Steam 重新启动游戏。","更新完成"); },false);
                AddButton(settings,"卸载组件",392,194,168,38,delegate { int left=InstallerCore.Uninstall(gamePath); MessageBox.Show(settings,left==0?"后台组件已卸载。可能保留加载器生成的配置和日志。":"卸载完成；已改动的文件予以保留。","卸载"); },false);
                theme.Label(settings,"首次安装或更新游戏内插件前，请先正常退出游戏。\n界面 0.2.4 · 钓鱼插件 0.2.3 · 优先完美判定",24,256,536,48,theme.Small).ForeColor=HarborTheme.Muted;
                var done=theme.Button(settings,"返回小屋",432,306,128,32,delegate { settings.Close(); },false);
                settings.CancelButton=done; settings.ShowDialog(this);
            }
        }
        string ReadSavedGame() {
            try { return File.Exists(saved)?File.ReadAllText(saved).Trim():""; }
            catch(IOException) { return ""; } catch(UnauthorizedAccessException) { return ""; }
        }
        void RememberGame() {
            try { Directory.CreateDirectory(Path.GetDirectoryName(saved)); File.WriteAllText(saved,gamePath); }
            catch(IOException) { } catch(UnauthorizedAccessException) { }
        }
        void LocateGame() {
            string[] candidates=GameLocator.Discover();
            gamePath=GameLocator.ChoosePath("","",candidates);
            discoveryMessage=candidates.Length>1?"找到多个游戏副本，请点击选择游戏，指定需要使用的 CELESTAY.exe。":"未找到游戏。请先安装或启动一次 CELESTAY，然后自动查找；也可手动选择。";
            if(gamePath.Length>0) RememberGame();
        }
        Button AddButton(Control parent,string text,int x,int y,int width,int height,Action action,bool primary) {
            return theme.Button(parent,text,x,y,width,height,delegate {
                if(preview && text!="小屋设置") return;
                try { action(); if(!preview) RefreshStatus(); }
                catch(Exception e) { MessageBox.Show(parent.FindForm(),e.Message,"操作未完成",MessageBoxButtons.OK,MessageBoxIcon.Information); }
            },primary);
        }
        void SetDisconnected(string heading,string message) {
            state.Text=heading; state.ForeColor=HarborTheme.Muted; startButton.Enabled=false; startButton.Text="开始垂钓";
            foreach(var value in totals) value.Text="—";
            stage.Text="等待连接你的小屋"; status.Text=message;
            connection.Text="星屿渔夫 0.2.4  ·  "+heading+"  ·  支持 3–100 米技巧钓鱼";
        }
        void RefreshStatus() {
            try {
                if(gamePath.Length==0) { SetDisconnected("未找到游戏",discoveryMessage); return; }
                string path=InstallerCore.StatusPath(gamePath);
                if(!File.Exists(path) || (DateTime.UtcNow-File.GetLastWriteTimeUtc(path)).TotalSeconds>8) { SetDisconnected("未连接","请在「小屋设置」中安装组件，再从 Steam 启动游戏。"); return; }
                RenderStatus(StatusFile.Read(path));
            } catch(IOException) { SetDisconnected("读取中","暂时无法读取状态，下次刷新将重试。"); }
            catch(Exception e) { SetDisconnected("连接异常",e.Message); }
        }
        internal void RenderStatus(string text) {
            var values=new Dictionary<string,string>();
            foreach(string line in text.Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries)) { int at=line.IndexOf('='); if(at>0) values[line.Substring(0,at)]=line.Substring(at+1); }
            Func<string,string> get=key=>values.ContainsKey(key)?values[key]:"";
            bool running=get("running")=="True", failed=get("error").Length>0;
            state.Text=failed?"需要查看":running?"垂钓中":"已停靠";
            state.ForeColor=failed?Color.FromArgb(145,58,38):HarborTheme.Sea;
            string[] keys={"caught","perfect","missed","attempts"};
            for(int i=0;i<keys.Length;i++) totals[i].Text=get(keys[i]).Length==0?"—":get(keys[i]);
            startButton.Enabled=!running; startButton.Text=running?"正在垂钓…":"开始垂钓";
            stage.Text=get("required_checks")!="" && get("required_checks")!="0"?
                "水深 "+get("depth")+" 米   /   本竿判定 "+get("completed_checks")+" / "+get("required_checks"):"等待下一次出发";
            status.Text=failed?"错误："+get("error"):Translate(get("reason"))+"  "+Translate(get("detail"));
            status.ScrollBars=failed?ScrollBars.Vertical:ScrollBars.None;
            connection.Text="星屿渔夫 0.2.4  ·  插件 "+get("version")+" 已连接  ·  支持 3–100 米技巧钓鱼";
        }
        protected override void Dispose(bool disposing) {
            if(disposing) timer.Dispose();
            base.Dispose(disposing);
            if(disposing) { if(scenery!=null) scenery.Dispose(); if(appIcon!=null) appIcon.Dispose(); theme.Dispose(); }
        }
        static string Translate(string value) {
            switch(value) {
                case "Idle": return "等待开始。";
                case "Running": return "运行中。";
                case "User stopped": return "已按要求停止。";
                case "Stopped with F8": return "已通过 F8 停止。";
                case "Attempt limit reached": return "已达到次数上限。";
                case "Time limit reached": return "已达到时长上限。";
                case "Running - orange first": return "有橙色时优先橙色。";
                case "Cast; watching target": return "已抛竿，正在等待目标。";
                case "Hit; waiting for reward": return "已命中，正在等待奖励。";
                case "Hit; waiting for next stage": return "本次已命中，等待下一次判定。";
                case "Caught fish": return "已获得奖励。";
                case "Closed reward": return "已关闭奖励，准备下一竿。";
                case "Set 11m skill fishing and close settings first": return "请设置 11 米技巧钓鱼并关闭设置面板，然后重新开始。";
                case "Set 0-20m single-stage skill fishing and close settings first": return "请使用 3–15 米单次判定钓鱼，保存并关闭设置；暂不支持两次判定。";
                case "Ready - choose 0-20m single-stage skill fishing": return "已就绪：支持 3–15 米单次判定钓鱼。";
                case "Set 3-20m skill fishing and close settings first": return "请使用 3–20 米技巧钓鱼，保存并关闭设置；更深水域尚未支持。";
                case "Ready - choose 3-20m skill fishing": return "已就绪：3–15 米一次判定；16–20 米两次判定。";
                case "Set 3-100m skill fishing and close settings first": return "请使用 3–100 米技巧钓鱼，保存并关闭设置；最多支持四次判定。";
                case "Ready - choose 3-100m skill fishing": return "已就绪：支持 3–100 米，按游戏实际配置完成判定。";
                case "Game paused or settings changed": return "游戏暂停或设置变更，已停止。";
                case "Unable to cast; check fish balance and skill mode": return "未能抛竿，已停止。请检查鱼数量及技巧钓鱼模式。";
                case "No progress for 30 seconds": return "30 秒未取得进展，已停止。";
                default: return value;
            }
        }
        [STAThread] static int Main(string[] args) {
            try {
                if(args.Length>=2 && args[0].StartsWith("--")) {
                    switch(args[0]) {
                        case "--install": InstallerCore.Install(args[1],Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"payload")); break;
                        case "--uninstall": InstallerCore.Uninstall(args[1]); break;
                        case "--update": InstallerCore.UpdatePlugin(args[1],Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"payload")); break;
                        case "--stop": InstallerCore.Command(args[1],false,0,0); break;
                        case "--start": InstallerCore.Command(args[1],true,int.Parse(args[2]),double.Parse(args[3],CultureInfo.InvariantCulture)); break;
                        case "--status": Console.WriteLine(StatusFile.Read(InstallerCore.StatusPath(args[1]))); break;
                        default: throw new ArgumentException("Unknown option");
                    }
                    return 0;
                }
                Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
                bool preview=args.Length==1 && args[0]=="--preview";
                Application.Run(new ControlApp(preview?"":args.Length>0?args[0]:"",preview)); return 0;
            } catch(Exception e) { Console.Error.WriteLine(e.Message); if(args.Length==0) MessageBox.Show(e.Message); return 1; }
        }
    }
}

