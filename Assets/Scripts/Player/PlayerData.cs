using Fusion;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

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


    void Start()
    {
        if(this.PlayFabId == GameObject.Find("PlayFabId").GetComponent<PlayFabId>().playFabId)
        {
            OnGetPlayerCombinedInfo();
        }

        // 他ユーザーのテキストUIを設定
        TextDisplayName.SetText(DisplayName);
    }

    private void OnGetPlayerCombinedInfo()
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
}
