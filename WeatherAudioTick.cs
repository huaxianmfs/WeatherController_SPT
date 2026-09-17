using System;
using UnityEngine;

namespace WeatherController.Client;

/// <summary>
/// 定期扫描并静音雨声。
/// 引擎会周期性重置 AudioSource.mute，一次性的静音不可靠，所以后台持续纠偏。
/// </summary>
public class WeatherAudioTick : MonoBehaviour
{
    public WeatherAudioTick(IntPtr ptr) : base(ptr) { }

    private const int ScanInterval = 60; // 约 1 秒一次

    private int _frameCounter;

    public void Update()
    {
        _frameCounter++;
        if (_frameCounter < ScanInterval)
        {
            return;
        }

        _frameCounter = 0;
        SeasonOverride.MuteRainAudioOnce();
    }
}