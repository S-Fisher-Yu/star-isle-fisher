using System;
using CelestayFishing;

class PolicyTests
{
    static int checks;
    static void Check(bool value, string name) { checks++; if(!value) throw new Exception(name); }
    static int Main()
    {
        try { Run(); return 0; }
        catch(Exception e) { Console.WriteLine("FAIL: "+e.Message); return 1; }
    }
    static void Run()
    {
        var s = new SessionPolicy();
        Check(!s.Check(0), "Never autostart");
        foreach(int depth in new int[]{3,11,12,15})
            Check(SessionPolicy.SupportsDepth(depth,1), "Allow shallow single-stage depth "+depth);
        foreach(int depth in new int[]{16,19,20})
            Check(SessionPolicy.SupportsDepth(depth,2), "Allow two-stage depth "+depth);
        Check(!SessionPolicy.SupportsDepth(2,1) && !SessionPolicy.SupportsDepth(101,2), "Reject depths outside supported range");
        Check(!SessionPolicy.SupportsDepth(12,2), "Reject changed multi-stage game configuration");
        Check(!SessionPolicy.SupportsDepth(20,1) && !SessionPolicy.SupportsDepth(20,3), "Reject unexpected deep-stage counts");
        Check(SessionPolicy.SupportsDepth(100,4), "Allow 100m four-stage fishing");
        Check(!SessionPolicy.SupportsDepth(100,0) && !SessionPolicy.SupportsDepth(100,5), "Reject unsupported stage counts");
        foreach(int depth in new[]{21,40,60,80,99,100})
            foreach(int stages in new[]{1,2,3,4})
                Check(SessionPolicy.SupportsDepth(depth,stages), "Read actual stage count at deep depth "+depth);
        Check(!SessionPolicy.SupportsDepth(101,4), "Reject depths beyond 100m");
        s.Start(2, 3, 0);
        Check(s.BeginAttempt(1), "First cast allowed");
        Check(!s.BeginAttempt(2), "Cannot cast twice before result");
        s.FinishAttempt(true, true);
        s.FinishAttempt(true, true);
        Check(s.Caught==1 && s.Perfect==1, "Count a result only once");
        Check(s.BeginAttempt(3), "Second cast allowed");
        s.FinishAttempt(false, false);
        Check(!s.BeginAttempt(4) && !s.Running && s.Attempts==2 && s.Missed==1, "Attempt limit prevents more spending");
        s.Start(99999, 480, 100);
        Check(s.Check(28899), "Eight hour run permitted before deadline");
        Check(!s.Check(28900) && !s.Running, "Stop at eight hour deadline");
        s.Start(5, 3, 0); s.Stop("User stopped");
        Check(!s.BeginAttempt(1), "Stop prevents further actions");
        foreach(int count in new int[]{0,-1,100000}) {
            bool rejected=false; try { s.Start(count,3,0); } catch(ArgumentException) { rejected=true; }
            Check(rejected, "Reject invalid count");
        }
        foreach(double minutes in new double[]{0,-1,481,double.NaN,double.PositiveInfinity}) {
            bool rejected=false; try { s.Start(5,minutes,0); } catch(ArgumentException) { rejected=true; }
            Check(rejected, "Reject invalid duration");
        }
        Check(!SessionPolicy.InTarget(20,20,80,true,40,10), "Orange prevents early blue strike");
        Check(SessionPolicy.InTarget(40,20,80,true,40,10), "Aim at orange center");
        Check(!SessionPolicy.InTarget(44.9,20,80,true,40,10), "Avoid orange edge");
        Check(SessionPolicy.InTarget(-120,-120,80,false,40,10), "New random blue position");
        Check(!SessionPolicy.InTarget(40,20,80,true,40,0), "Invalid orange never falls back to blue");
        Check(!SessionPolicy.InTarget(double.NaN,20,80,false,40,10), "Invalid coordinates rejected");
        Console.WriteLine("PASS: "+checks+" policy checks");
    }
}
