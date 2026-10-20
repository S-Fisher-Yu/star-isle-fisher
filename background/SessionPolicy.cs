using System;

namespace CelestayFishing
{
    public class SessionPolicy
    {
        public bool Running, Pending;
        public int Attempts, Caught, Missed, Perfect;
        public string Reason = "Idle";
        public static bool SupportsDepth(int depth, int requiredChecks) {
            return (depth>=3 && depth<=15 && requiredChecks==1) || (depth>=16 && depth<=20 && requiredChecks==2)
                || (depth>=21 && depth<=100 && requiredChecks>=1 && requiredChecks<=4);
        }
        int limit;
        double deadline;
        public void Start(int attempts, double minutes, double now)
        {
            if(attempts<1 || attempts>99999 || double.IsNaN(minutes) || double.IsInfinity(minutes) || minutes<=0 || minutes>480)
                throw new ArgumentException("Use 1-99999 attempts and up to 480 minutes.");
            limit=attempts; deadline=now+minutes*60;
            Attempts=Caught=Missed=Perfect=0; Pending=false; Running=true; Reason="Running";
        }
        public bool Check(double now)
        {
            if(Running && now>=deadline) Stop("Time limit reached");
            return Running;
        }
        public bool BeginAttempt(double now)
        {
            if(!Check(now) || Pending) return false;
            if(Attempts>=limit) { Stop("Attempt limit reached"); return false; }
            Attempts++; Pending=true; return true;
        }
        public void FinishAttempt(bool success, bool perfect)
        {
            if(!Running || !Pending) return;
            Pending=false;
            if(success) { Caught++; if(perfect) Perfect++; } else Missed++;
            if(Attempts>=limit) Stop("Attempt limit reached");
        }
        public void Stop(string reason) { Running=false; Reason=reason; }
        public static bool InTarget(double needle, double blueCenter, double blueWidth, bool hasOrange, double orangeCenter, double orangeWidth)
        {
            double center=hasOrange?orangeCenter:blueCenter, width=hasOrange?orangeWidth:blueWidth;
            if(double.IsNaN(needle) || double.IsNaN(center) || double.IsInfinity(needle) || double.IsInfinity(center) || double.IsInfinity(width) || width<=0) return false;
            return Math.Abs(needle-center)<=width*(hasOrange?.25:.28);
        }
    }
}
