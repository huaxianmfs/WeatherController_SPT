using UnityEngine;

namespace WeatherController.Client;

internal enum PresetKind
{
    Full,        // 完整天气：所有字段一起设
    RainOnly,    // 只改降雨
    FogOnly,     // 只改雾气
    SeasonOnly,  // 只切季节（顺带设温度）
}

internal class WeatherPreset
{
    public string Name;
    public PresetKind Kind;

    // 天气字段。null = 不动这个值。
    public float? CloudDensity;
    public float? Fog;
    public float? Rain;
    public float? LightningThunderProbability;
    public float? Temperature;
    public float? WindMagnitude;
    public global::EFT.Weather.WeatherDebug.Direction? WindDirection;
    public Vector2? TopWind;

    // 季节
    public global::ESeason? Season;
}

internal static class WeatherPresets
{
    private static readonly WeatherPreset[] Table =
    {
        // ── 0. 晴 ─────────────────────────────────────────
        new WeatherPreset
        {
            Name = "晴天",
            Kind = PresetKind.Full,
            CloudDensity                = -0.2f,
            Fog                         = 0.004f,
            Rain                        = 0f,
            LightningThunderProbability = 0f,
            Temperature                 = 22f,
            WindMagnitude               = 0.1f,
            WindDirection               = global::EFT.Weather.WeatherDebug.Direction.North,
            TopWind                     = Vector2.left,
        },

        // ── 1. 阴 ─────────────────────────────────────────
        new WeatherPreset
        {
            Name = "阴天",
            Kind = PresetKind.Full,
            CloudDensity                = 0.5f,
            Fog                         = 0.006f,
            Rain                        = 0f,
            LightningThunderProbability = 0f,
            Temperature                 = 18f,
            WindMagnitude               = 0.3f,
            WindDirection               = global::EFT.Weather.WeatherDebug.Direction.West,
            TopWind                     = Vector2.left,
        },

        // ── 2. 小雨 ────────────────────────────────────────
        new WeatherPreset
        {
            Name = "小雨",
            Kind = PresetKind.RainOnly,
            Rain = 0.3f,
        },

        // ── 3. 大雨（冬季地图 = 大雪） ──────────────────────
        new WeatherPreset
        {
            Name = "大雨",
            Kind = PresetKind.RainOnly,
            Rain = 0.9f,
        },

        // ── 4. 小雾 ────────────────────────────────────────
        new WeatherPreset
        {
            Name = "小雾",
            Kind = PresetKind.FogOnly,
            Fog = 0.01f,
        },

        // ── 5. 留空 ────────────────────────────────────────
        null,

        // ── 6. 大雾 ────────────────────────────────────────
        new WeatherPreset
        {
            Name = "大雾",
            Kind = PresetKind.FogOnly,
            Fog = 0.05f,
        },

        // ── 7. 夏 ─────────────────────────────────────────
        new WeatherPreset
        {
            Name = "夏",
            Kind = PresetKind.SeasonOnly,
            Season = global::ESeason.Summer,
            Temperature = 22f,
        },

        // ── 8. 秋 ─────────────────────────────────────────
        new WeatherPreset
        {
            Name = "秋",
            Kind = PresetKind.SeasonOnly,
            Season = global::ESeason.Autumn,
            Temperature = 10f,
        },

        // ── 9. 冬 ─────────────────────────────────────────
        new WeatherPreset
        {
            Name = "冬",
            Kind = PresetKind.SeasonOnly,
            Season = global::ESeason.Winter,
            Temperature = -10f,
        },
    };

    public static WeatherPreset Get(int index)
    {
        if (index < 0 || index >= Table.Length)
        {
            return null;
        }
        return Table[index];
    }
}