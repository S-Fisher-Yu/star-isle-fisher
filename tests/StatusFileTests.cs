using System;
using System.IO;
using CelestayFishing;

class StatusFileTests {
    static int checks;
    static void Check(bool value,string name) { checks++; if(!value) throw new Exception(name); }
    static int Main() {
        string root=Path.Combine(Path.GetTempPath(),"CelestayStatusTests-"+Guid.NewGuid().ToString("N"));
        try {
            Directory.CreateDirectory(root);
            string path=Path.Combine(root,"status.txt");
            Check(StatusFile.Write(path,"caught=1\n"),"Create first complete snapshot");
            using(var reader=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read)) {
                Check(!StatusFile.Write(path,"caught=2\n"),"Old reader's sharing lock defers update instead of throwing");
                Check(File.ReadAllText(path)=="caught=1\n","Sharing conflict preserves complete previous snapshot");
            }
            Check(StatusFile.Write(path,"caught=2\n") && StatusFile.Read(path)=="caught=2\n","Next update recovers after reader releases file");
            using(var reader=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete)) {
                Check(StatusFile.Write(path,"caught=3\n"),"Compatible readers do not block atomic replacement");
                using(var text=new StreamReader(reader)) Check(text.ReadToEnd()=="caught=2\n","Existing reader retains a consistent old snapshot");
            }
            Check(StatusFile.Read(path)=="caught=3\n","New readers receive complete latest snapshot");
            bool allWritten=true;
            for(int i=0;i<50;i++) allWritten &= StatusFile.Write(path,"caught="+i+"\n");
            Check(allWritten,"Repeated overwrite recovers without accumulating files");
            Check(Directory.GetFiles(root).Length==1,"Successful writes do not accumulate temporary files");
            bool rejected=false;
            try { StatusFile.Write(Path.Combine(root,"missing","status.txt"),"bad"); }
            catch(DirectoryNotFoundException) { rejected=true; }
            Check(rejected,"Unexpected storage errors are not silently hidden");
            Console.WriteLine("PASS: "+checks+" status file checks"); return 0;
        } catch(Exception e) { Console.WriteLine("FAIL: "+e.Message); return 1; }
        finally { if(Directory.Exists(root)) Directory.Delete(root,true); }
    }
}
