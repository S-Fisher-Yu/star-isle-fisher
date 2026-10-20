using System;
using System.IO;
using System.Text;

namespace CelestayFishing {
    public static class StatusFile {
        public static bool Write(string path,string text) {
            string pending=path+".tmp";
            try {
                File.WriteAllText(pending,text,new UTF8Encoding(false));
                if(File.Exists(path)) File.Replace(pending,path,null);
                else File.Move(pending,path);
                return true;
            } catch(IOException e) {
                int code=e.HResult & 0xffff;
                // Old controller versions may briefly deny replacement while reading.
                // Keep the previous snapshot and retry at the next regular update.
                if(code==32 || code==33) return false;
                throw;
            }
        }
        public static string Read(string path) {
            using(var file=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete))
            using(var reader=new StreamReader(file)) return reader.ReadToEnd();
        }
    }
}
