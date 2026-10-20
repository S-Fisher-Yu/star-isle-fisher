// Test-only stand-ins: compile with the real plugin source, never ship in the runtime DLL.
using System;

namespace BepInEx {
    public class BaseUnityPlugin { }
    public class BepInPlugin : Attribute { public BepInPlugin(string id,string name,string version) { } }
    public class BepInProcess : Attribute { public BepInProcess(string name) { } }
    public static class Paths { public static string ConfigPath; }
}
namespace UnityEngine {
    public class Object {
        public static object Current;
        public static T FindFirstObjectByType<T>() { return (T)Current; }
    }
    public static class Time { public static double realtimeSinceStartupAsDouble; public static float timeScale=1; }
    public static class Application { public static bool runInBackground,isFocused; }
    public class GameObject { public bool activeSelf,activeInHierarchy; public void SetActive(bool active) { activeSelf=activeInHierarchy=active; } }
    public struct Vector3 { public float x; }
    public struct Rect { public float width; }
    public class RectTransform {
        public GameObject gameObject=new GameObject();
        public Vector3 position; public Rect rect;
        public Vector3 InverseTransformPoint(Vector3 value) { return new Vector3{x=value.x-position.x}; }
    }
}
public class FishingSettingsWindow { public UnityEngine.GameObject gameObject=new UnityEngine.GameObject(); }
public class FishingUI_Slider { public int Depth=20; }
public enum DepthType { depth_A }
public class DepthSettings { public DepthType GetDepthTypeByDepth(int depth) { return DepthType.depth_A; } }
public class FishingActionDataItem { public int depth_A=2; }
public class FishingEventController {
    public bool resultShown; public int CloseCalls;
    public void OnResultCloseButton() { resultShown=false; CloseCalls++; }
}
public class FishingActionGame {
    public FishingEventController fishingEventController=new FishingEventController();
    public FishingSettingsWindow fishingSettingsWindow=new FishingSettingsWindow();
    public FishingUI_Slider fishingUI_Slider=new FishingUI_Slider();
    public DepthSettings depthSettings=new DepthSettings();
    public FishingActionDataItem boxAmountData=new FishingActionDataItem();
    public UnityEngine.GameObject trigger=new UnityEngine.GameObject(),gaugeGroup=new UnityEngine.GameObject();
    public UnityEngine.RectTransform bgArea=new UnityEngine.RectTransform(),needle=new UnityEngine.RectTransform(),targetArea=new UnityEngine.RectTransform(),targetArea_orange=new UnityEngine.RectTransform();
    public bool canClick,IsOrange,RejectNext;
    public int currentSuccessCount,StartCalls,JudgeCalls;
    public FishingActionGame() { trigger.SetActive(true); targetArea.rect=new UnityEngine.Rect{width=80}; targetArea_orange.rect=new UnityEngine.Rect{width=10}; }
    public void OnClickStartFishing() {
        StartCalls++; currentSuccessCount=0; IsOrange=false; canClick=true; trigger.SetActive(false); gaugeGroup.SetActive(true);
    }
    public void OnClickCheckTiming() {
        if(!canClick) throw new Exception("Judge called during an animation");
        JudgeCalls++; canClick=false;
        if(RejectNext) { RejectNext=false; return; }
        IsOrange=targetArea_orange.gameObject.activeSelf;
        currentSuccessCount=IsOrange?boxAmountData.depth_A:currentSuccessCount+1;
    }
}
