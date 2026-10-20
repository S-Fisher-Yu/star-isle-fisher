using System;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Runtime.InteropServices;
using BepInEx;
using UnityEngine;

namespace CelestayFishing
{
    [BepInPlugin("local.celestay.fishing", "Celestay Fishing", "0.2.3")]
    [BepInProcess("CELESTAY.exe")]
    public class FishingPlugin : BaseUnityPlugin
    {
        [DllImport("user32.dll")] static extern short GetAsyncKeyState(int key);
        readonly SessionPolicy session = new SessionPolicy();
        FishingActionGame action;
        FishingEventController events;
        FishingSettingsWindow settings;
        FishingUI_Slider depth;
        DepthSettings depthRanges;
        object checkCountData;
        GameObject trigger, gauge;
        RectTransform background, needle, blue, orange;
        FieldInfo canClick, successCount, resultShown;
        string folder, lastCommand="", detail="Waiting for game", lastStrike="", error="";
        double nextPoll, nextStatus, actionTime, sessionStart;
        bool originalBackground, changedBackground, strikePerfect;
        int observedDepth, requiredChecks, completedChecks, judgments, statusWriteDeferrals;
        static double Now { get { return Time.realtimeSinceStartupAsDouble; } }

        void Awake()
        {
            folder=Path.Combine(Paths.ConfigPath,"CelestayFishing");
            Directory.CreateDirectory(folder);
            lastCommand=ReadCommand(); // Old commands never resume on game startup.
            WriteStatus();
        }

        T Field<T>(object instance, string name)
        {
            FieldInfo field=instance.GetType().GetField(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if(field==null) throw new MissingFieldException(instance.GetType().Name,name);
            return (T)field.GetValue(instance);
        }

        bool Bind()
        {
            if(action!=null && events!=null) return true;
            action=UnityEngine.Object.FindFirstObjectByType<FishingActionGame>();
            if(action==null) { detail="Waiting for game"; return false; }
            events=Field<FishingEventController>(action,"fishingEventController");
            settings=Field<FishingSettingsWindow>(action,"fishingSettingsWindow");
            depth=Field<FishingUI_Slider>(action,"fishingUI_Slider");
            depthRanges=Field<DepthSettings>(action,"depthSettings");
            checkCountData=Field<object>(action,"boxAmountData");
            trigger=Field<GameObject>(action,"trigger"); gauge=Field<GameObject>(action,"gaugeGroup");
            background=Field<RectTransform>(action,"bgArea"); needle=Field<RectTransform>(action,"needle");
            blue=Field<RectTransform>(action,"targetArea"); orange=Field<RectTransform>(action,"targetArea_orange");
            var flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            canClick=typeof(FishingActionGame).GetField("canClick",flags);
            successCount=typeof(FishingActionGame).GetField("currentSuccessCount",flags);
            resultShown=typeof(FishingEventController).GetField("resultShown",flags);
            if(events==null || settings==null || depth==null || trigger==null || gauge==null || background==null || needle==null || blue==null || orange==null || canClick==null || successCount==null || resultShown==null)
                throw new InvalidOperationException("Unsupported game layout");
            detail="Ready - choose 3-100m skill fishing";
            return true;
        }

        string ReadCommand()
        {
            try {
                string path=Path.Combine(folder,"command.txt");
                if(!File.Exists(path) || new FileInfo(path).Length>512) return "";
                return File.ReadAllText(path).Trim();
            } catch(IOException) { return ""; }
        }

        void PollCommand()
        {
            string command=ReadCommand();
            if(command.Length==0 || command==lastCommand) return;
            string[] parts=command.Split('|');
            if(parts.Length<2) return;
            if(parts[1]=="STOP") { lastCommand=command; Stop("User stopped"); return; }
            if(parts.Length!=4 || parts[1]!="START") return;
            lastCommand=command;
            if(session.Running) { detail="Already running"; return; }
            int count; double minutes;
            if(!int.TryParse(parts[2],out count) || !double.TryParse(parts[3],NumberStyles.Float,CultureInfo.InvariantCulture,out minutes)) { detail="Invalid limits"; return; }
            if(!Bind()) { detail="Game not ready; press Start again"; return; }
            if(!SupportedDepth() || settings.gameObject.activeInHierarchy) { detail="Set 3-100m skill fishing and close settings first"; return; }
            session.Start(count,minutes,Now);
            originalBackground=Application.runInBackground;
            Application.runInBackground=true; changedBackground=true;
            error=""; lastStrike=""; strikePerfect=false; sessionStart=actionTime=Now;
            judgments=completedChecks=0;
            detail="Running - orange first";
        }

        void Stop(string reason)
        {
            session.Stop(reason); detail=reason;
            RestoreBackground(); WriteStatus();
        }

        void RestoreBackground()
        {
            if(changedBackground) { Application.runInBackground=originalBackground; changedBackground=false; }
        }

        float Position(RectTransform target) { return background.InverseTransformPoint(target.position).x; }

        bool SupportedDepth()
        {
            observedDepth=depth.Depth;
            string fieldName=depthRanges.GetDepthTypeByDepth(observedDepth).ToString();
            requiredChecks=Field<int>(checkCountData,fieldName);
            return SessionPolicy.SupportsDepth(observedDepth,requiredChecks);
        }

        void Update()
        {
            try {
                if(Now>=nextPoll) { nextPoll=Now+.2; PollCommand(); }
                if(session.Running && (GetAsyncKeyState(0x77)&0x8000)!=0) Stop("Stopped with F8");
                if(session.Check(Now)) Tick(); else RestoreBackground();
                if(Now>=nextStatus) { nextStatus=Now+1; WriteStatus(); }
            } catch(Exception e) {
                error=e.GetType().Name+": "+e.Message;
                session.Stop("Stopped after error"); detail="Stopped after error"; RestoreBackground();
                try { WriteStatus(); } catch(Exception) { }
            }
        }

        void Tick()
        {
            if(!Bind()) { Stop("Game scene changed"); return; }
            if(!SupportedDepth() || settings.gameObject.activeInHierarchy || Time.timeScale<=0) { Stop("Game paused or settings changed"); return; }
            if((bool)resultShown.GetValue(events)) {
                if(session.Pending) { session.FinishAttempt(true,strikePerfect); actionTime=Now; detail="Caught fish"; }
                if(!session.Running) { RestoreBackground(); return; }
                if(Now-actionTime>.8) { events.OnResultCloseButton(); actionTime=Now; detail="Closed reward"; }
                return;
            }
            if(gauge.activeInHierarchy) {
                if(!session.Pending) {
                    if(!(bool)canClick.GetValue(action)) return;
                    if(!session.BeginAttempt(Now)) return;
                    actionTime=Now; strikePerfect=false; completedChecks=0;
                }
                if((bool)canClick.GetValue(action)) {
                    bool hasOrange=orange.gameObject.activeSelf;
                    float nx=Position(needle), bx=Position(blue), ox=Position(orange);
                    if(SessionPolicy.InTarget(nx,bx,blue.rect.width,hasOrange,ox,orange.rect.width)) {
                        int before=(int)successCount.GetValue(action);
                        action.OnClickCheckTiming(); // Use the game's normal judge and rewards.
                        judgments++;
                        completedChecks=(int)successCount.GetValue(action);
                        bool hit=completedChecks>before;
                        strikePerfect=hit && action.IsOrange;
                        lastStrike=string.Format(CultureInfo.InvariantCulture,"{0}; needle={1:F2}; center={2:F2}; perfect={3}; focused={4}; checks={5}/{6}",hasOrange?"orange":"blue",nx,hasOrange?ox:bx,strikePerfect,Application.isFocused,completedChecks,requiredChecks);
                        actionTime=Now;
                        detail=!hit?"Miss":(completedChecks<requiredChecks?"Hit; waiting for next stage":"Hit; waiting for reward");
                        if(!hit) session.FinishAttempt(false,false);
                    }
                }
            } else if(trigger.activeInHierarchy && Now-actionTime>1) {
                if(!session.BeginAttempt(Now)) return;
                strikePerfect=false; completedChecks=0; action.OnClickStartFishing(); actionTime=Now;
                if(!gauge.activeInHierarchy) { Stop("Unable to cast; check fish balance and skill mode"); return; }
                detail="Cast; watching target";
            }
            if(Now-actionTime>30) Stop("No progress for 30 seconds");
        }

        void WriteStatus()
        {
            if(folder==null) return;
            string text="version=0.2.3\nrunning="+session.Running+"\nattempts="+session.Attempts+"\ncaught="+session.Caught+"\nmissed="+session.Missed+"\nperfect="+session.Perfect+
                "\ndepth="+observedDepth+"\nrequired_checks="+requiredChecks+"\ncompleted_checks="+completedChecks+"\njudgments="+judgments+
                "\nstatus_write_deferrals="+statusWriteDeferrals+
                "\nreason="+session.Reason+"\ndetail="+detail+"\nlast_strike="+lastStrike+"\nerror="+error.Replace('\n',' ')+"\nfocused="+Application.isFocused+"\nelapsed_seconds="+(sessionStart>0?(Now-sessionStart).ToString("F1",CultureInfo.InvariantCulture):"0")+"\nupdated_utc="+DateTime.UtcNow.ToString("o")+"\n";
            if(!StatusFile.Write(Path.Combine(folder,"status.txt"),text)) statusWriteDeferrals++;
        }
        void OnDestroy() { session.Stop("Plugin unloaded"); RestoreBackground(); }
    }
}

