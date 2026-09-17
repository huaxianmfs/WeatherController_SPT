using System;
using UnityEngine;

namespace WeatherController.Client;

public class WeatherHotkeyBehaviour : MonoBehaviour
{
    public WeatherHotkeyBehaviour(IntPtr ptr) : base(ptr) { }

    public void Update()
    {
        bool altHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        if (!altHeld)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Keypad0)) { Apply(0); return; }
        if (Input.GetKeyDown(KeyCode.Keypad1)) { Apply(1); return; }
        if (Input.GetKeyDown(KeyCode.Keypad2)) { Apply(2); return; }
        if (Input.GetKeyDown(KeyCode.Keypad3)) { Apply(3); return; }
        if (Input.GetKeyDown(KeyCode.Keypad4)) { Apply(4); return; }
        if (Input.GetKeyDown(KeyCode.Keypad6)) { Apply(6); return; }
        if (Input.GetKeyDown(KeyCode.Keypad7)) { Apply(7); return; }
        if (Input.GetKeyDown(KeyCode.Keypad8)) { Apply(8); return; }
        if (Input.GetKeyDown(KeyCode.Keypad9)) { Apply(9); return; }
    }

    private static void Apply(int index)
    {
        var preset = WeatherPresets.Get(index);
        if (preset == null)
        {
            WeatherControllerPlugin.Logger.LogWarning(
                $"[WeatherController] 未定义的预设索引 {index}");
            return;
        }

        // 所有分支都先缓存 SeasonsController（季节键要用，天气键用不上也无害）
        CacheSeasonsController();

        var controller = global::EFT.Weather.WeatherController.Instance;
        if (controller == null)
        {
            WeatherControllerPlugin.Logger.LogWarning(
                "[WeatherController] 当前场景没有动态天气（藏身处或工厂）。");
            return;
        }

        var dbg = controller.WeatherDebug;
        if (dbg == null)
        {
            WeatherControllerPlugin.Logger.LogWarning(
                "[WeatherController] WeatherDebug 不可用，无法改写天气。");
            return;
        }

        // 打开 override 开关，否则下面的写入会被引擎忽略
        dbg.Enabled = true;

        // 季节键：切季节 + 顺带设温度
        if (preset.Kind == PresetKind.SeasonOnly)
        {
            if (preset.Season.HasValue)
            {
                SeasonOverride.SetSeason(preset.Season.Value);
            }

            if (preset.Temperature.HasValue)
            {
                dbg.Temperature = preset.Temperature.Value;
                WeatherControllerPlugin.Logger.LogInfo(
                    $"[WeatherController] 温度设为 {preset.Temperature.Value}℃");
            }

            WeatherControllerPlugin.Logger.LogInfo(
                $"[WeatherController] 已应用：{preset.Name}（Preset {index}）");
            return;
        }

        // 天气键：只写 preset 里非 null 的字段
        // 这是"小雨 + 大雾 = 太阳雨"能工作的关键：
        //   按 2 只动 Rain，按 6 只动 Fog，互不覆盖
        if (preset.CloudDensity.HasValue)
            dbg.CloudDensity = preset.CloudDensity.Value;
        if (preset.Fog.HasValue)
            dbg.Fog = preset.Fog.Value;
        if (preset.Rain.HasValue)
            dbg.Rain = preset.Rain.Value;
        if (preset.LightningThunderProbability.HasValue)
            dbg.LightningThunderProbability = preset.LightningThunderProbability.Value;
        if (preset.Temperature.HasValue)
            dbg.Temperature = preset.Temperature.Value;
        if (preset.WindMagnitude.HasValue)
            dbg.WindMagnitude = preset.WindMagnitude.Value;
        if (preset.WindDirection.HasValue)
            dbg.WindDirection = preset.WindDirection.Value;
        if (preset.TopWind.HasValue)
            dbg.TopWindDirection = preset.TopWind.Value;

        WeatherControllerPlugin.Logger.LogInfo(
            $"[WeatherController] 已应用：{preset.Name}（Preset {index}）");
    }

    private static void CacheSeasonsController()
    {
        var gameWorld = global::Comfort.Common.Singleton<global::EFT.GameWorld>.Instance;
        if (gameWorld == null)
        {
            return;
        }

        var seasons = gameWorld.SeasonsController;
        if (seasons == null)
        {
            return;
        }

        var controller = seasons.TryCast<global::SeasonsController>();
        if (controller != null)
        {
            SeasonOverride.Cache(controller);
        }
    }
}