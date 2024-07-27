using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using Newtonsoft.Json;
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
    
    private ChatManager chatManager;
    private NetworkRunner runner;

    private void Start()
    {   
        chatManager = GameObject.Find("ChatManager").GetComponent<ChatManager>();
        
        if(Object.HasInputAuthority)
        {
            // Invoke("GetPlayerCombinedInfo", 1f); // すぐに実行すると反映されていないため1秒後に実行
            isOnline = true;
            
            DisplayName = PlayFabData.MyName;
            GraduationYear = PlayFabData.MyGraduationYear;
            // 自分のテキストUIを設定
            TextDisplayName.SetText(DisplayName);
            SetUserData();
            chatManager.chatSender = GetComponent<ChatSender>();
            Invoke("CheckDoubleLogin", 0.1f);
        }
        else
        {
            PlayFabData.CurrentRoomPlayersRefs[this.PlayFabId] = this;
            Debug.Log("start" + DisplayName);
            
            // 他ユーザーのテキストUIを設定
            Invoke("SetTextDisplayName", 2f); // すぐに実行すると反映されていないため1秒後に実行
        }
        //Debug.Log(PlayFabData.DictDMScripts[this.PlayFabId]);
        Invoke("AddDictDMScripts", 1.5f);
    }

    private void CheckDoubleLogin()
    {
        if(PlayFabData.CurrentRoomPlayersRefs.ContainsKey(this.PlayFabId))
        {
            Debug.LogError("このアカウントは別の端末でログインしています。");
            GameObject.Find("Logout").GetComponent<PlayFabLogout>().OnClickLogout();
        }
        else
        {
            PlayFabData.CurrentRoomPlayersRefs[this.PlayFabId] = this;
        }
    }

    private void AddDictDMScripts()
    {
        if(PlayFabData.DictDMScripts.ContainsKey(this.PlayFabId) & PlayFabData.DictDMScripts[this.PlayFabId].playerInstance == null)
        {
            PlayFabData.DictDMScripts[this.PlayFabId].playerInstance = this.gameObject;
        }
        else
        {
            Invoke("AddDictDMScripts", 1.0f);
        }
    }

    private void SetTextDisplayName()
    {
        TextDisplayName.text = DisplayName;
    }

    private void GetPlayerCombinedInfo()
    {
        var request = new GetPlayerCombinedInfoRequest{PlayFabId = PlayFabSettings.staticPlayer.PlayFabId, InfoRequestParameters = PlayerInfoParams};
        PlayFabClientAPI.GetPlayerCombinedInfo(request, OnGetPlayerCombinedInfoSuccess, error => {Debug.Log("PlayerCombinedInfoの取得に失敗");});
    }
    private void OnGetPlayerCombinedInfoSuccess(GetPlayerCombinedInfoResult result)
    {
        DisplayName = result.InfoResultPayload.UserData["DisplayName"].Value;
        GraduationYear = result.InfoResultPayload.UserData["GraduationYear"].Value;
        // 自分のテキストUIを設定
        TextDisplayName.SetText(DisplayName);
    }

    // プレイヤーが切断したときの処理
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        Debug.Log("despauwnd");
        base.Despawned(runner, true);
        if (Object.HasInputAuthority)
        {
            isOnline = false;
            PlayFabData.DictDMScripts = new Dictionary<string, DMButton>();
            PlayFabData.DictChannelScripts = new Dictionary<string, ChannelButton>();
            PlayFabData.CurrentRoomPlayersRefs = new Dictionary<string, PlayerData>();
            PlayFabData.CurrentChannelId = "general";
            Debug.Log("despauwnd" + Object);
        }
    }

    private void OnDestroy()
    {
        isOnline = false;
        // SetUserData();
    }

    void OnExitButtonClicked()
    {

    }

    void OnApplicationQuit()
    {
        isOnline = false;
        // StartCoroutine(SetUserData());
        // SetUserData();

    }

    private void SetUserData()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>{{"IsOnline", isOnline.ToString()}, },
            Permission = UserDataPermission.Public
        };
        PlayFabClientAPI.UpdateUserData(request, 
            _ => 
            {
                Debug.Log("IsOnline変更成功");
                if (isOnline == false)
                {
                    PlayFabSettings.staticPlayer.ForgetAllCredentials();
                }
            }, 
            _=> 
            {
                Debug.Log("IsOnline変更失敗");
            }
        );
    }
}
