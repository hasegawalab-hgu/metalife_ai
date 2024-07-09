using System.Collections.Generic;
using Fusion;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using Newtonsoft.Json;
using UnityEditor.PackageManager.Requests;
using UnityEditor;

public class PlayerData : NetworkBehaviour
{
    [Networked]
    public string PlayFabId {get; set;}

    public GetPlayerCombinedInfoRequestParams PlayerInfoParams;

    public TMP_Text TextDisplayName;
    [Networked]
    public string DisplayName { get; set;}

    [Networked]
    public string GraduationYear { get; set;}

    [Networked]
    public bool KeepLoginInfo { get; set;}

    private bool isOnline;
    


    void Start()
    {
        isOnline = true;
        SetUserData();
        if(this.PlayFabId == PlayFabSettings.staticPlayer.PlayFabId)
        {
            GetPlayerCombinedInfo();
        }

        // 他ユーザーのテキストUIを設定
        TextDisplayName.SetText(DisplayName);
    }

    private void GetPlayerCombinedInfo()
    {
        var request = new GetPlayerCombinedInfoRequest{PlayFabId = this.PlayFabId, InfoRequestParameters = PlayerInfoParams};
        PlayFabClientAPI.GetPlayerCombinedInfo(request, OnGetPlayerCombinedInfoSuccess, error => {Debug.Log("PlayerCombinedInfoの取得に失敗");});
    }
    private void OnGetPlayerCombinedInfoSuccess(GetPlayerCombinedInfoResult result)
    {
        DisplayName = result.InfoResultPayload.UserData["DisplayName"].Value;
        GraduationYear = result.InfoResultPayload.UserData["GraduationYear"].Value;
        KeepLoginInfo = result.InfoResultPayload.UserData["KeepLoginInfo"].Value == "True" ? true : false;
        // 自分のテキストUIを設定
        TextDisplayName.SetText(DisplayName);
    }

    // プレイヤーが切断したときの処理
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, true);
        isOnline = false;
        SetUserData();
    }

    private void SetUserData()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>{{"IsOnline", isOnline.ToString()}},
            Permission = UserDataPermission.Public
        };
        PlayFabClientAPI.UpdateUserData(request, _ => Debug.Log("IsOnline変更成功"), _=> Debug.Log("IsOnline変更失敗"));
    }
}
