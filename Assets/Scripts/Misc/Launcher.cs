using System;
using CustomConnectionHandler;
using Fusion;
using TMPro;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Launcher : MonoBehaviour {
    [SerializeField] private ConnectionData _initialConnection;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private GameObject _launchButton;

    public void Launch()
    {
        int now = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds + 3600 * 9; // 日本時間
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(now);
        PlayFabData.LoginStartTime = dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss");
        int time = Mathf.FloorToInt(PlayFabData.LoginTime);
        int hours = time / 3600;
        int minutes = (time % 3600) / 60;
        int secs = time % 60;

        // フォーマット
        string timeStr = $"{hours:D2}:{minutes:D2}:{secs:D2}";
        PlayFabData.DictLoginTime[PlayFabData.LoginStartTime] = timeStr;
        PlayFabData.DictLoginTime[PlayFabData.LoginStartTime] = timeStr;
        // 保存
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"Login", JsonConvert.SerializeObject(PlayFabData.DictLoginTime)},
            }
        };
        PlayFabClientAPI.UpdateUserData(request, result => Debug.Log("ログイン時間更新成功"), error => {Debug.Log("ログイン時間の更新失敗" + error.GenerateErrorReport());});

        _text.text = "Connecting to Lobby";
        _text.gameObject.SetActive(true);
        _launchButton.SetActive(false);
        _ = ConnectionManager.Instance.ConnectToRunner(_initialConnection, onFailed: OnConnectionFailed);
    }

    private void OnConnectionFailed(ShutdownReason reason)
    {
        _text.text = $"Failed: {reason}";
        _launchButton.SetActive(true);
    }
}
