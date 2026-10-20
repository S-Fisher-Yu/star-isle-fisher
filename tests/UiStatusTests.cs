using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CelestayFishing;

class UiStatusTests {
    static int checks;
    static void Check(bool condition,string name) { checks++; if(!condition) throw new Exception(name); }
    static T Field<T>(ControlApp app,string name) {
        return (T)typeof(ControlApp).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(app);
    }
    [STAThread] static int Main() {
        try {
            using(var app=new ControlApp("",true)) {
                var button=Field<Button>(app,"startButton");
                var totals=Field<Label[]>(app,"totals");
                var detail=Field<TextBox>(app,"status");
                Check(!button.Enabled,"Disconnected preview cannot start fishing");
                app.RenderStatus("version=0.2.3\nrunning=True\ncaught=4428\nperfect=341\nmissed=0\nattempts=4429\ndepth=100\nrequired_checks=4\ncompleted_checks=3\nreason=Running\nerror=\n");
                Check(!button.Enabled && button.Text.Contains("…"),"Running session cannot be accidentally restarted");
                Check(totals.Select(x=>x.Text).SequenceEqual(new[]{"4428","341","0","4429"}),"Metrics preserve their individual meanings");
                Check(Field<Label>(app,"stage").Text.Contains("3 / 4"),"Stage display uses actual completed and required checks");
                app.RenderStatus("version=0.2.3\nrunning=False\nreason=Stopped after error\nerror=IOException: test failure\n");
                Check(detail.Text.Contains("IOException: test failure"),"Runtime errors are visible instead of hidden by translated details");
                Check(totals.All(x=>x.Text=="—"),"Missing metrics do not retain values from a previous run");
                Check(button.Enabled,"Stopped session permits a new start");
                Check(Field<NumericUpDown>(app,"count").Maximum==99999 && Field<NumericUpDown>(app,"minutes").Maximum==480,"Existing session limits are preserved");
                Check(!Field<Timer>(app,"timer").Enabled,"Preview never polls or starts game work");
                Check(app.BackgroundImage!=null,"Shipped artwork is embedded and available without external paths");
            }
            Console.WriteLine("PASS: "+checks+" UI status checks"); return 0;
        } catch(Exception e) { Console.WriteLine("FAIL: "+e.Message); return 1; }
    }
}
