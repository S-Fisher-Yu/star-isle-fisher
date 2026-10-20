using System;
using System.IO;
using CelestayFishing;

class GameLocatorTests {
    static int checks;
    static void Check(bool condition,string message) { checks++; if(!condition) throw new Exception(message); }
    static string FakeGame(string root) {
        Directory.CreateDirectory(Path.Combine(root,"CELESTAY_Data","Managed"));
        string exe=Path.Combine(root,"CELESTAY.exe"); File.WriteAllText(exe,"");
        File.WriteAllText(Path.Combine(root,"CELESTAY_Data","Managed","Assembly-CSharp.dll"),""); return exe;
    }
    static string Quote(string value) { return value.Replace("\\","\\\\"); }
    static int Main() {
        string root=Path.Combine(Path.GetTempPath(),"CelestayLocatorTests-"+Guid.NewGuid().ToString("N"));
        try {
            Directory.CreateDirectory(root);
            string steam=Path.Combine(root,"Steam"), library=Path.Combine(root,"游戏 library");
            string game=FakeGame(Path.Combine(library,"steamapps","common","CELESTAY"));
            Directory.CreateDirectory(Path.Combine(steam,"steamapps"));
            File.WriteAllText(Path.Combine(steam,"steamapps","libraryfolders.vdf"),"\"libraryfolders\" { \"0\" { \"path\" \""+Quote(library)+"\" } }");
            var candidates=GameLocator.FindCandidates(new[]{steam},new string[0]);
            Check(candidates.Length==1 && candidates[0]==game,"Find game in secondary Steam library with spaces and Unicode");
            Check(GameLocator.FindCandidates(new[]{steam,steam},new[]{game}).Length==1,"Deduplicate running game and Steam result");
            Check(GameLocator.ChoosePath("",Path.Combine(root,"stale.exe"),candidates)==game,"Stale remembered path falls back to discovery");
            Check(GameLocator.FindCandidates(new string[0],new[]{game}).Length==1,"Find running game without Steam registry");
            string custom=FakeGame(Path.Combine(steam,"steamapps","common","CustomFolder"));
            File.WriteAllText(Path.Combine(steam,"steamapps","appmanifest_123.acf"),"\"AppState\" { \"name\" \"CELESTAY\" \"installdir\" \"CustomFolder\" }");
            candidates=GameLocator.FindCandidates(new[]{steam},new string[0]);
            Check(candidates.Length==2,"Find custom install directory from matching manifest");
            Check(GameLocator.ChoosePath("","",candidates)=="","Multiple installations require user selection");
            Check(GameLocator.ChoosePath("",game,candidates)==game,"Valid saved choice wins when multiple games exist");
            Check(GameLocator.ChoosePath(custom,game,candidates)==custom,"Explicit initial path wins over remembered path");
            File.WriteAllText(Path.Combine(root,"CELESTAY.exe"),"");
            Check(!GameLocator.IsGamePath(Path.Combine(root,"CELESTAY.exe")),"Reject same-name executable without game assembly");
            File.WriteAllText(Path.Combine(steam,"steamapps","appmanifest_123.acf"),"\"name\" \"Other Game\" \"installdir\" \"CustomFolder\"");
            Check(GameLocator.FindCandidates(new[]{steam},new string[0]).Length==1,"Ignore unrelated game manifest");
            Check(GameLocator.FindCandidates(new[]{Path.Combine(root,"missing")},new[]{"bad\0path"}).Length==0,"Missing or malformed paths do not crash startup");
            Check(GameLocator.ChoosePath("","",new string[0])=="","No results retain manual selection");
            Console.WriteLine("PASS: "+checks+" game locator checks"); return 0;
        } catch(Exception e) { Console.WriteLine("FAIL: "+e.Message); return 1; }
        finally { if(Directory.Exists(root)) Directory.Delete(root,true); }
    }
}
