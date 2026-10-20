using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace CelestayFishing {
    public static class GameLocator {
        public static bool IsGamePath(string path) {
            try {
                return !string.IsNullOrWhiteSpace(path) && string.Equals(Path.GetFileName(path),"CELESTAY.exe",StringComparison.OrdinalIgnoreCase)
                    && File.Exists(path) && File.Exists(Path.Combine(Path.GetDirectoryName(path),"CELESTAY_Data","Managed","Assembly-CSharp.dll"));
            } catch(ArgumentException) { return false; } catch(NotSupportedException) { return false; } catch(IOException) { return false; }
        }
        public static string ChoosePath(string initial,string saved,string[] candidates) {
            if(IsGamePath(initial)) return Path.GetFullPath(initial);
            if(IsGamePath(saved)) return Path.GetFullPath(saved);
            return candidates.Length==1?candidates[0]:"";
        }
        static string ReadSmallFile(string path) {
            try { return File.Exists(path) && new FileInfo(path).Length<=1024*1024?File.ReadAllText(path):""; }
            catch(IOException) { return ""; } catch(UnauthorizedAccessException) { return ""; }
        }
        static IEnumerable<string> Values(string text,string key) {
            string pattern="\""+Regex.Escape(key)+"\"\\s*\"((?:\\\\.|[^\"\\\\])*)\"";
            foreach(Match match in Regex.Matches(text,pattern,RegexOptions.IgnoreCase))
                yield return match.Groups[1].Value.Replace("\\\\","\\").Replace("\\\"","\"");
        }
        static void AddPath(HashSet<string> set,string path) {
            if(string.IsNullOrWhiteSpace(path)) return;
            try { set.Add(Path.GetFullPath(path)); }
            catch(ArgumentException) { } catch(NotSupportedException) { } catch(IOException) { }
        }
        public static string[] FindCandidates(IEnumerable<string> steamRoots,IEnumerable<string> runningPaths) {
            var libraries=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var games=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach(string path in runningPaths) if(IsGamePath(path)) AddPath(games,path);
            foreach(string root in steamRoots) AddPath(libraries,root);
            foreach(string root in libraries.ToArray())
                foreach(string library in Values(ReadSmallFile(Path.Combine(root,"steamapps","libraryfolders.vdf")),"path")) AddPath(libraries,library);
            foreach(string library in libraries) {
                string steamapps=Path.Combine(library,"steamapps"), common=Path.Combine(steamapps,"common");
                string standard=Path.Combine(common,"CELESTAY","CELESTAY.exe");
                if(IsGamePath(standard)) AddPath(games,standard);
                try {
                    if(!Directory.Exists(steamapps)) continue;
                    foreach(string manifest in Directory.EnumerateFiles(steamapps,"appmanifest_*.acf")) {
                        string text=ReadSmallFile(manifest);
                        if(!Values(text,"name").Any(name=>string.Equals(name,"CELESTAY",StringComparison.OrdinalIgnoreCase))) continue;
                        foreach(string directory in Values(text,"installdir")) {
                            // Install directories must remain under this Steam library's common folder.
                            if(directory.Length==0 || directory.IndexOfAny(Path.GetInvalidFileNameChars())>=0 || directory=="..") continue;
                            string candidate=Path.Combine(common,directory,"CELESTAY.exe");
                            if(IsGamePath(candidate)) AddPath(games,candidate);
                        }
                    }
                } catch(IOException) { } catch(UnauthorizedAccessException) { }
            }
            return games.OrderBy(path=>path,StringComparer.OrdinalIgnoreCase).ToArray();
        }
        public static string[] Discover() {
            var roots=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var running=new List<string>();
            foreach(string key in new[]{@"HKEY_CURRENT_USER\Software\Valve\Steam",@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam",@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam"}) {
                try {
                    AddPath(roots,Registry.GetValue(key,"SteamPath",null) as string);
                    AddPath(roots,Registry.GetValue(key,"InstallPath",null) as string);
                } catch(System.Security.SecurityException) { } catch(UnauthorizedAccessException) { }
            }
            string programFiles=Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if(programFiles.Length>0) AddPath(roots,Path.Combine(programFiles,"Steam"));
            foreach(Process process in Process.GetProcessesByName("CELESTAY")) {
                using(process) {
                    try { running.Add(process.MainModule.FileName); }
                    catch(System.ComponentModel.Win32Exception) { } catch(InvalidOperationException) { }
                }
            }
            return FindCandidates(roots,running);
        }
    }
}
