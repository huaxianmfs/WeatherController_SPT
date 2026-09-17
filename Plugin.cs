using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace WeatherController.Client;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public class WeatherControllerPlugin : BasePlugin
{
    public const string PluginGuid = "com.weathercontroller.client";
    public const string PluginName = "WeatherController.Client";
    public const string PluginVersion = "1.0.0";

    internal static ManualLogSource Logger;

    public override void Load()
    {
        Logger = Log;

        // 1. 应用 Harmony 补丁
        try
        {
            var harmony = new Harmony(PluginGuid);
            harmony.PatchAll();
            Logger.LogInfo("[WeatherController] Harmony 补丁已应用。");
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"[WeatherController] Harmony 补丁失败：{ex}");
        }

        // 2. 挂上按键监听
        AddComponent<WeatherHotkeyBehaviour>();
        AddComponent<WeatherAudioTick>();

        Logger.LogInfo($"[{PluginName}] v{PluginVersion} 已加载。");
        Logger.LogInfo("[WeatherController] Alt + 小键盘 0~6");
        Logger.LogInfo("[WeatherController] 0=晴空 1=阴天 2=小雨 3=大雨 4=小雾 6=大雾");
    }
}