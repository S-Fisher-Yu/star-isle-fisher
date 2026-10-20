using System;
using System.IO;
using CelestayFishing;

class InstallerTests
{
    static int checks;
    static void Check(bool ok,string name) { checks++; if(!ok) throw new Exception(name); }
    static void Reject(Action action,string name) { bool fail=false; try { action(); } catch(Exception) { fail=true; } Check(fail,name); }
    static int Main(string[] args) {
        string root=Path.Combine(Path.GetTempPath(),"CelestayFishingTests_"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try {
            string game=Path.Combine(root,"game"),payload=Path.Combine(root,"payload");
            Directory.CreateDirectory(Path.Combine(game,"CELESTAY_Data","Managed")); Directory.CreateDirectory(payload);
            string exe=Path.Combine(game,"CELESTAY.exe"); File.WriteAllText(exe,"test fixture, never executable");
            string assembly=Path.Combine(game,"CELESTAY_Data","Managed","Assembly-CSharp.dll");
            File.WriteAllText(assembly,"wrong version"); File.WriteAllText(Path.Combine(payload,"winhttp.dll"),"test payload");
            Reject(delegate { InstallerCore.Install(exe,payload); },"Wrong version cannot install");
            Check(!File.Exists(Path.Combine(game,"winhttp.dll")),"Wrong version writes no loader");
            File.Copy(args[0],assembly,true);
            File.WriteAllText(Path.Combine(game,"winhttp.dll"),"preexisting");
            Reject(delegate { InstallerCore.Install(exe,payload); },"Existing loader cannot be replaced");
            Check(File.ReadAllText(Path.Combine(game,"winhttp.dll"))=="preexisting","Existing loader preserved");
            File.Delete(Path.Combine(game,"winhttp.dll"));
            InstallerCore.Install(exe,payload);
            Check(File.Exists(Path.Combine(game,InstallerCore.ManifestName)),"Install records owned files");
            Check(File.ReadAllText(Path.Combine(game,"winhttp.dll"))=="test payload","Install copies exact payload");
            string pluginRelative=Path.Combine("BepInEx","plugins","CelestayFishing","CelestayFishing.dll");
            string plugin=Path.Combine(game,pluginRelative), nextPlugin=Path.Combine(payload,pluginRelative);
            Directory.CreateDirectory(Path.GetDirectoryName(plugin)); Directory.CreateDirectory(Path.GetDirectoryName(nextPlugin));
            File.WriteAllText(plugin,"old plugin"); File.WriteAllText(nextPlugin,"new plugin");
            File.AppendAllLines(Path.Combine(game,InstallerCore.ManifestName),new string[]{InstallerCore.Hash(plugin)+"|"+pluginRelative});
            InstallerCore.UpdatePlugin(exe,payload);
            Check(File.ReadAllText(plugin)=="new plugin","Update installs new plugin");
            Check(File.ReadAllText(Path.Combine(game,"winhttp.dll"))=="test payload","Update leaves loader intact");
            File.WriteAllText(plugin,"unexpected modification");
            Reject(delegate { InstallerCore.UpdatePlugin(exe,payload); },"Update refuses altered plugin");
            Check(File.ReadAllText(plugin)=="unexpected modification","Refused update preserves file");
            File.WriteAllText(plugin,"new plugin");
            File.WriteAllText(Path.Combine(game,"keep.txt"),"unrelated");
            File.WriteAllText(Path.Combine(game,"winhttp.dll"),"changed after install");
            Check(InstallerCore.Uninstall(exe)==1,"Uninstall retains altered loader");
            Check(File.Exists(Path.Combine(game,"winhttp.dll")),"Altered loader still exists");
            File.WriteAllText(Path.Combine(game,"winhttp.dll"),"test payload");
            Check(InstallerCore.Uninstall(exe)==0,"Uninstall removes matching owned loader");
            Check(!File.Exists(Path.Combine(game,"winhttp.dll")) && File.Exists(assembly) && File.Exists(Path.Combine(game,"keep.txt")),"Uninstall preserves game and unrelated files");
            Console.WriteLine("PASS: "+checks+" installer checks"); return 0;
        } catch(Exception e) { Console.WriteLine("FAIL: "+e.Message); return 1; }
        finally { Directory.Delete(root,true); }
    }
}
