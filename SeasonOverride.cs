using System;
using HarmonyLib;

namespace WeatherController.Client;

public static class SeasonOverride
{
    private static global::SeasonsController _cached;
    private static global::ESeason? _current;

    private static global::SeasonsController.IState _summerState;
    private static global::SeasonsController.IState _autumnState;
    private static global::SeasonsController.IState _winterState;

    public static global::ESeason? CurrentForce => _current;

    /// <summary>当前是否处于冬季。</summary>
    public static bool IsWinterActive => _current == global::ESeason.Winter;

    public static void Cache(global::SeasonsController controller)
    {
        _cached = controller;
    }

    public static void SetSeason(global::ESeason season)
    {
        if (_cached == null || _cached.Pointer == IntPtr.Zero)
        {
            WeatherControllerPlugin.Logger.LogWarning(
                "[WeatherController] SeasonsController 未缓存，无法切换季节");
            return;
        }

        // 一旦进入冬季，本战局内锁死。
        // 引擎不支持来回切换季节，反复切会破坏粒子/材质状态。
        if (_current == global::ESeason.Winter && season != global::ESeason.Winter)
        {
            WeatherControllerPlugin.Logger.LogWarning(
                "[WeatherController] 冬季已锁定，本战局内不可逆。退出战局后自动恢复。");
            return;
        }

        if (_current == season)
        {
            WeatherControllerPlugin.Logger.LogInfo(
                $"[WeatherController] 季节已经是 {season}，跳过");
            return;
        }

        var target = BuildState(season);
        if (target == null)
        {
            WeatherControllerPlugin.Logger.LogWarning(
                $"[WeatherController] 没有为 {season} 准备 state 实现");
            return;
        }

        var previous = _cached._state;
        if (previous != null && previous.Pointer != IntPtr.Zero)
        {
            try
            {
                var prevAbstract = previous.TryCast<global::SeasonsController.AbstractState>();
                if (prevAbstract != null)
                {
                    prevAbstract.Dispose();
                }
            }
            catch (Exception ex)
            {
                WeatherControllerPlugin.Logger.LogWarning(
                    $"[WeatherController] Dispose 旧 state 失败（可忽略）：{ex.Message}");
            }
        }

        try
        {
            _cached._state = target;
            target.Run();
            WeatherControllerPlugin.Logger.LogInfo(
                $"[WeatherController] 季节已切换为 {season}");

            try
            {
                global::SeasonsController.AbstractState.UpdateImpostors();
            }
            catch (Exception ex)
            {
                WeatherControllerPlugin.Logger.LogWarning(
                    $"[WeatherController] UpdateImpostors 失败：{ex.Message}");
            }

            try
            {
                var visual = _cached._visual;
                if (visual != null && visual.Pointer != IntPtr.Zero)
                {
                    target.SetupVisual(visual);
                }
            }
            catch (Exception ex)
            {
                WeatherControllerPlugin.Logger.LogWarning(
                    $"[WeatherController] SetupVisual 失败：{ex.Message}");
            }

            _current = season;

            // 立即静音一次，效果更快出来
            MuteRainAudioOnce();
        }
        catch (Exception ex)
        {
            WeatherControllerPlugin.Logger.LogError(
                $"[WeatherController] 切换季节到 {season} 失败：{ex}");
        }
    }

    private static bool? _lastMute;

    /// <summary>
    /// 扫描全场景 AudioSource，对 rain/storm/thunder 的源静音或恢复。
    /// 由 WeatherAudioTick 定期调用，防止游戏重置 mute。
    /// </summary>
    public static void MuteRainAudioOnce()
    {
        bool mute = IsWinterActive;
        bool stateChanged = !_lastMute.HasValue || _lastMute.Value != mute;

        try
        {
            var all = UnityEngine.Object.FindObjectsOfType<UnityEngine.AudioSource>();
            if (all == null)
            {
                return;
            }

            int matched = 0;

            foreach (var src in all)
            {
                if (src == null || src.Pointer == IntPtr.Zero) continue;

                string clipName = "<none>";
                try
                {
                    var clip = src.clip;
                    if (clip != null && clip.Pointer != IntPtr.Zero)
                    {
                        clipName = clip.name;
                    }
                }
                catch { }

                string path = "<unknown>";
                try
                {
                    path = GetTransformPath(src.transform);
                }
                catch { }

                string lower = (clipName + "|" + path).ToLowerInvariant();
                bool isRainSound = lower.Contains("rain")
                                || lower.Contains("storm")
                                || lower.Contains("thunder");

                if (isRainSound)
                {
                    src.mute = mute;
                    matched++;
                }
            }

            if (stateChanged)
            {
                _lastMute = mute;
                WeatherControllerPlugin.Logger.LogInfo(
                    $"[WeatherController] 雨声 mute={mute}，共 {matched} 个");
            }
        }
        catch (Exception ex)
        {
            if (stateChanged)
            {
                WeatherControllerPlugin.Logger.LogWarning(
                    $"[WeatherController] 扫描音频源失败：{ex.Message}");
            }
        }
    }

    private static string GetTransformPath(UnityEngine.Transform t)
    {
        if (t == null || t.Pointer == IntPtr.Zero) return "<null>";

        string path = t.name;
        var parent = t.parent;
        int safety = 0;

        while (parent != null && parent.Pointer != IntPtr.Zero && safety < 20)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
            safety++;
        }

        return path;
    }

    private static global::SeasonsController.IState BuildState(global::ESeason season)
    {
        switch (season)
        {
            case global::ESeason.Summer:
                if (_summerState == null)
                {
                    _summerState = new global::SeasonsController.StateSummer(_cached)
                        .TryCast<global::SeasonsController.IState>();
                }
                return _summerState;

            case global::ESeason.Autumn:
                if (_autumnState == null)
                {
                    _autumnState = new global::SeasonsController.StateAutumn(_cached)
                        .TryCast<global::SeasonsController.IState>();
                }
                return _autumnState;

            case global::ESeason.Winter:
                if (_winterState == null)
                {
                    _winterState = new global::SeasonsController.StateWinter(_cached, false)
                        .TryCast<global::SeasonsController.IState>();
                }
                return _winterState;

            default:
                return null;
        }
    }
}

[HarmonyPatch(typeof(global::SeasonsController), "get_Season")]
public static class SeasonsControllerSeasonPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref global::ESeason __result)
    {
        var force = SeasonOverride.CurrentForce;
        if (force.HasValue)
        {
            __result = force.Value;
        }
    }
}