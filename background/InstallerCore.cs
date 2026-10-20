using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace CelestayFishing
{
    public static class InstallerCore
    {
        public const string GameHash="FA8F7410A31F8205C11734CC412D626788195EB4ED85CEB295084EF3CC439F40";
        public const string ManifestName="CelestayFishing.install.txt";
        public static string Hash(string path) {
            using(var sha=SHA256.Create()) using(var stream=File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","");
        }
        public static string Validate(string exe, bool requireVersion) {
            string root=Path.GetDirectoryName(Path.GetFullPath(exe));
            if(!string.Equals(Path.GetFileName(exe),"CELESTAY.exe",StringComparison.OrdinalIgnoreCase) || !File.Exists(exe))
                throw new InvalidOperationException("请选择 CELESTAY.exe。");
            if(requireVersion && Hash(Path.Combine(root,"CELESTAY_Data","Managed","Assembly-CSharp.dll"))!=GameHash)
                throw new InvalidOperationException("游戏版本与已验证版本不同，已取消安装。请先更新兼容性。");
            return root;
        }
        static string Inside(string root, string relative) {
            string full=Path.GetFullPath(Path.Combine(root,relative));
            if(Path.IsPathRooted(relative) || !full.StartsWith(Path.GetFullPath(root)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Invalid package path");
            return full;
        }
        static void RequireClosed() {
            if(Process.GetProcessesByName("CELESTAY").Length>0) throw new InvalidOperationException("请先正常退出 CELESTAY，再安装或卸载。");
        }
        public static void Install(string exe, string payload) {
            string root=Validate(exe,true); RequireClosed();
            if(File.Exists(Path.Combine(root,ManifestName)) || Directory.Exists(Path.Combine(root,"BepInEx")) || File.Exists(Path.Combine(root,"winhttp.dll")) || File.Exists(Path.Combine(root,"doorstop_config.ini")))
                throw new InvalidOperationException("检测到已有插件或加载器。为避免覆盖，安装已停止。");
            string payloadRoot=Path.GetFullPath(payload);
            string[] files=Directory.GetFiles(payloadRoot,"*",SearchOption.AllDirectories);
            if(files.Length==0) throw new InvalidOperationException("安装包不完整。");
            var owned=new List<string>();
            foreach(string file in files) {
                string relative=file.Substring(payloadRoot.Length+1);
                if(File.Exists(Inside(root,relative))) throw new InvalidOperationException("目标文件已存在："+relative);
            }
            // Record every successfully copied file so an interrupted install is removable.
            File.WriteAllText(Path.Combine(root,ManifestName),"");
            foreach(string file in files) {
                string relative=file.Substring(payloadRoot.Length+1), target=Inside(root,relative);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                File.Copy(file,target,false);
                owned.Add(Hash(target)+"|"+relative);
                File.WriteAllLines(Path.Combine(root,ManifestName),owned.ToArray());
            }
        }
        public static int Uninstall(string exe) {
            string root=Validate(exe,false); RequireClosed();
            string manifest=Path.Combine(root,ManifestName);
            if(!File.Exists(manifest)) throw new InvalidOperationException("没有本工具的安装记录，不会删除文件。");
            var preserve=new List<string>(); var parents=new HashSet<string>();
            foreach(string line in File.ReadAllLines(manifest)) {
                string[] parts=line.Split('|');
                if(parts.Length!=2) throw new InvalidOperationException("安装记录损坏，已停止卸载。");
                string path=Inside(root,parts[1]);
                if(!File.Exists(path)) continue;
                if(Hash(path)!=parts[0]) { preserve.Add(line); continue; }
                File.Delete(path); parents.Add(Path.GetDirectoryName(path));
            }
            foreach(string name in new string[]{"status.txt","command.txt"}) {
                string path=Path.Combine(root,"BepInEx","config","CelestayFishing",name);
                if(File.Exists(path)) File.Delete(path);
                parents.Add(Path.GetDirectoryName(path));
            }
            if(preserve.Count>0) File.WriteAllLines(manifest,preserve.ToArray()); else File.Delete(manifest);
            foreach(string parent in parents.OrderByDescending(p=>p.Length)) {
                string dir=parent;
                while(dir!=root && Directory.Exists(dir) && Directory.GetFileSystemEntries(dir).Length==0) {
                    Directory.Delete(dir); dir=Path.GetDirectoryName(dir);
                }
            }
            return preserve.Count;
        }
        public static void UpdatePlugin(string exe, string payload) {
            string root=Validate(exe,true); RequireClosed();
            string relative=Path.Combine("BepInEx","plugins","CelestayFishing","CelestayFishing.dll");
            string target=Inside(root,relative), source=Inside(payload,relative), manifest=Path.Combine(root,ManifestName);
            string[] lines=File.ReadAllLines(manifest);
            int index=Array.FindIndex(lines,line=>line.EndsWith("|"+relative,StringComparison.OrdinalIgnoreCase));
            if(index<0 || !File.Exists(target) || Hash(target)!=lines[index].Split('|')[0])
                throw new InvalidOperationException("现有插件与安装记录不一致，已取消更新。");
            string suffix=".update-"+Guid.NewGuid().ToString("N"), temp=target+suffix, backup=target+suffix+".backup", manifestTemp=manifest+suffix;
            bool replaced=false, committed=false;
            try {
                File.Copy(source,temp,false);
                lines[index]=Hash(temp)+"|"+relative;
                File.WriteAllLines(manifestTemp,lines);
                File.Replace(temp,target,backup); replaced=true;
                File.Replace(manifestTemp,manifest,null); committed=true;
            } finally {
                if(replaced && !committed && File.Exists(backup)) File.Replace(backup,target,null);
                if(File.Exists(temp)) File.Delete(temp);
                if(File.Exists(manifestTemp)) File.Delete(manifestTemp);
                if(File.Exists(backup)) File.Delete(backup);
            }
        }
        public static string StatusPath(string exe) { return Path.Combine(Validate(exe,false),"BepInEx","config","CelestayFishing","status.txt"); }
        public static void Command(string exe, bool start, int count, double minutes) {
            string root=Validate(exe,false);
            if(!File.Exists(Path.Combine(root,ManifestName))) throw new InvalidOperationException("请先安装后台组件。");
            if(start) {
                new SessionPolicy().Start(count,minutes,0);
                string status=StatusPath(exe);
                if(!File.Exists(status) || (DateTime.UtcNow-File.GetLastWriteTimeUtc(status)).TotalSeconds>8)
                    throw new InvalidOperationException("插件尚未就绪。请启动游戏，或查看状态信息。");
            }
            string folder=Path.Combine(root,"BepInEx","config","CelestayFishing"); Directory.CreateDirectory(folder);
            string command=Guid.NewGuid().ToString("N")+"|"+(start?"START|"+count+"|"+minutes.ToString(System.Globalization.CultureInfo.InvariantCulture):"STOP");
            File.WriteAllText(Path.Combine(folder,"command.txt"),command);
        }
    }
}
