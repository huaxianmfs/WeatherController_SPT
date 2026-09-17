using System;
using System.Text;
using UnityEngine.Networking;

namespace WeatherController.Client;

/// <summary>
/// 把玩家的选择 POST 给本机 SPT 服务端。
/// 服务端如果没装对应 mod，这一步会静默失败，不影响战局内的天气。
/// </summary>
internal static class ServerClient
{
    private const string BaseUrl = "http://127.0.0.1:6969";

    public static void PostPreference(string presetName)
    {
        try
        {
            var body = Encoding.UTF8.GetBytes("{\"preset\":\"" + presetName + "\"}");

            var request = new UnityWebRequest(BaseUrl + "/weathercontroller/set", "POST");
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // fire-and-forget：不等待结果，也不阻塞主线程
            request.SendWebRequest();
        }
        catch (Exception ex)
        {
            WeatherControllerPlugin.Logger.LogWarning(
                $"[WeatherController] 同步服务端失败（不影响本地天气）：{ex.Message}");
        }
    }
}