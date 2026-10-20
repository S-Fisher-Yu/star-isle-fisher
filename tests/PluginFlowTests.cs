using System;
using System.Reflection;
using CelestayFishing;
using UnityEngine;

class PluginFlowTests {
    static int checks;
    static void Check(bool value,string message) { checks++; if(!value) throw new Exception(message); }
    static object Call(object target,string name) { return target.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,null); }
    class Fixture {
        public FishingPlugin Plugin=new FishingPlugin();
        public FishingActionGame Game=new FishingActionGame();
        public SessionPolicy Session;
        public Fixture(int count,int depth,int stages) {
            Game.fishingUI_Slider.Depth=depth; Game.boxAmountData.depth_A=stages;
            UnityEngine.Object.Current=Game; Time.realtimeSinceStartupAsDouble=0;
            Call(Plugin,"Bind");
            Session=(SessionPolicy)typeof(FishingPlugin).GetField("session",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(Plugin);
            Session.Start(count,3,0);
        }
        public void Tick(double now) { Time.realtimeSinceStartupAsDouble=now; if(Session.Check(now)) Call(Plugin,"Tick"); }
        public void SecondStage(float center) {
            Game.targetArea.position=new Vector3{x=center}; Game.needle.position=new Vector3{x=center}; Game.canClick=true;
        }
        public void Reward() { Game.gaugeGroup.SetActive(false); Game.fishingEventController.resultShown=true; }
    }
    static int Main() {
        try {
            var f=new Fixture(1,20,2); f.Tick(2);
            Check(f.Game.StartCalls==1 && f.Session.Attempts==1,"20m casts once");
            f.Tick(3);
            Check(f.Game.JudgeCalls==1 && f.Session.Caught==0 && f.Session.Pending && f.Session.Running,"First hit does not complete or spend another cast");
            f.Tick(3.2); Check(f.Game.JudgeCalls==1,"No duplicate click during transition");
            f.SecondStage(120); f.Tick(4);
            Check(f.Game.JudgeCalls==2 && f.Game.currentSuccessCount==2 && f.Session.Caught==0,"Second target is read anew and reward still awaited");
            f.Reward(); f.Tick(5); f.Tick(7);
            Check(f.Session.Caught==1 && f.Session.Attempts==1 && !f.Session.Running && f.Game.StartCalls==1,"Two judgments count as one cast and respect limit");

            f=new Fixture(3,16,2); f.Tick(2); f.Tick(3); f.SecondStage(-100); f.Game.RejectNext=true; f.Tick(4); f.Tick(4.2);
            Check(f.Session.Caught==0 && f.Session.Missed==1 && f.Session.Attempts==1,"Second-stage miss counts once, never catches");

            f=new Fixture(1,20,2); f.Tick(2);
            f.Game.targetArea_orange.gameObject.SetActive(true); f.Game.targetArea_orange.position=new Vector3{x=140};
            f.Tick(3); Check(f.Game.JudgeCalls==0,"Separate orange prevents early blue strike");
            f.Game.needle.position=new Vector3{x=140}; f.Tick(4); f.Reward(); f.Tick(5);
            Check(f.Game.JudgeCalls==1 && f.Session.Caught==1 && f.Session.Perfect==1,"Orange can complete both stages with one judgment");

            f=new Fixture(1,20,2); f.Tick(2); f.Tick(3); f.SecondStage(100);
            f.Game.targetArea_orange.gameObject.SetActive(true); f.Game.targetArea_orange.position=new Vector3{x=100};
            f.Tick(4); f.Reward(); f.Tick(5);
            Check(f.Game.JudgeCalls==2 && f.Session.Caught==1 && f.Session.Perfect==1,"Second-stage orange counts a perfect catch");

            f=new Fixture(2,20,2); f.Tick(2); f.Tick(3); f.Session.Stop("User stopped"); f.SecondStage(100); f.Tick(4);
            Check(f.Game.JudgeCalls==1 && f.Session.Caught==0,"Stop between stages prevents next judgment");
            f=new Fixture(2,20,2); f.Tick(2); f.Tick(3); f.SecondStage(100); f.Tick(181);
            Check(f.Game.JudgeCalls==1 && f.Session.Caught==0 && !f.Session.Running,"Deadline between stages prevents next judgment");
            f=new Fixture(1,12,1); f.Tick(2); f.Tick(3); f.Reward(); f.Tick(4);
            Check(f.Game.JudgeCalls==1 && f.Session.Caught==1,"12m single-stage behavior retained");
            f=new Fixture(1,100,4); f.Game.targetArea.rect=new Rect{width=12}; f.Tick(2);
            Check(f.Game.StartCalls==1,"100m can start a four-stage cast");
            for(int stage=0;stage<4;stage++) {
                f.Game.canClick=true;
                f.Game.needle.position=new Vector3{x=0};
                f.Game.targetArea.position=new Vector3{x=0};
                f.Game.targetArea.position=new Vector3{x=30+stage*10};
                f.Tick(3+stage*2);
                Check(f.Game.JudgeCalls==stage,"Moving blue no longer covers old position");
                f.Game.needle.position=f.Game.targetArea.position; f.Tick(3.5+stage*2);
                Check(f.Game.JudgeCalls==stage+1 && f.Session.Caught==0,"Each narrow moving target is judged at current position, not counted as a fish");
            }
            f.Reward(); f.Tick(12);
            Check(f.Session.Caught==1 && f.Session.Attempts==1 && !f.Session.Running,"Four hits complete exactly one catch");
            f=new Fixture(1,100,4); f.Tick(2); f.Tick(3); f.SecondStage(100);
            f.Game.targetArea_orange.rect=new Rect{width=4}; f.Game.targetArea_orange.gameObject.SetActive(true);
            f.Game.targetArea_orange.position=new Vector3{x=-120}; f.Tick(4);
            Check(f.Game.JudgeCalls==1,"Narrow stationary orange takes priority over moving blue");
            f.Game.needle.position=new Vector3{x=-120}; f.Tick(5); f.Reward(); f.Tick(6);
            Check(f.Session.Caught==1 && f.Session.Perfect==1 && f.Game.JudgeCalls==2,"Orange ends remaining deep stages using normal game result");
            f=new Fixture(1,100,4); f.Tick(2);
            for(int stage=0;stage<4;stage++) { f.SecondStage(30*stage); f.Game.RejectNext=stage==3; f.Tick(3+stage); }
            Check(f.Session.Caught==0 && f.Session.Missed==1 && f.Session.Attempts==1 && !f.Session.Running,"Fourth-stage miss fails one cast, not three successful fish");
            f=new Fixture(1,50,3); f.Tick(2);
            for(int stage=0;stage<3;stage++) { f.SecondStage(-30*stage); f.Tick(3+stage); }
            f.Reward(); f.Tick(6);
            Check(f.Session.Caught==1 && f.Game.JudgeCalls==3,"Intermediate depth uses actual three-stage requirement");
            Console.WriteLine("PASS: "+checks+" plugin flow checks"); return 0;
        } catch(Exception e) { Console.WriteLine("FAIL: "+e.GetBaseException().Message); return 1; }
    }
}
