using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.GroupsModels;
using TMPro;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEditor;

public class PlayFabLogin : PlayFabLoginAndSignup
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    // private bool keepLoginInfo;

    void Start()
    {
        PlayFabSettings.TitleId = "35895";
        OnLoginWithDevice();
    }

    void OnLoginWithDevice()
    {
        // デバイスによる一意の値
        var customId = SystemInfo.deviceUniqueIdentifier;

        // カスタムIDでログインを試みる
        var customIdRequest = new LoginWithCustomIDRequest
        {
            CustomId = customId,
            CreateAccount = false, // アカウントを新規作成しない
            TitleId = PlayFabSettings.TitleId,
            InfoRequestParameters = playerInfoParams
        };

        PlayFabClientAPI.LoginWithCustomID(customIdRequest, OnLoginWithCustomIDSuccess, _ => Debug.Log("カスタムIDでのログイン失敗。PlayFabアカウントでログインしてください。: "));
    }

    void OnLoginWithCustomIDSuccess(LoginResult result)
    {
        Debug.Log("カスタムIDでのログイン成功");
        PlayFabData.MyYearLabSharedGroupId = PlayFabData.AllYearLabSharedGroupId + "_" + (int.Parse(result.InfoResultPayload.UserData["GraduationYear"].Value) - 1).ToString();

        if(result.InfoResultPayload.UserData["KeepLoginInfo"].Value == "True")
        {
            GetSharedGroupData(PlayFabData.AllYearLabSharedGroupId);
            GetSharedGroupData(PlayFabData.MyYearLabSharedGroupId);
            loginUI.SetActive(false);
            fusion.SetActive(true);
            PlayFabSettings.staticPlayer.PlayFabId = result.InfoResultPayload.AccountInfo.PlayFabId;
        }
    }

    // カスタムIDでのログインに失敗した場合は、ユーザー名とパスワードでログイン
    public void OnLoginWIthPlayFab()
    {
        var request = new LoginWithPlayFabRequest
        {
            Username = usernameInput.text,
            Password = passwordInput.text,
            TitleId = PlayFabSettings.TitleId,
            InfoRequestParameters = playerInfoParams
        };

        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginWithPlayFabSuccess, OnLoginWithPlayFabFailure);
    }

    void OnLoginWithPlayFabSuccess(LoginResult result)
    {
        Debug.Log("ユーザー名とパスワードでのログイン成功");

        PlayFabSettings.staticPlayer.PlayFabId = result.InfoResultPayload.AccountInfo.PlayFabId;
        PlayFabData.MyYearLabSharedGroupId = PlayFabData.AllYearLabSharedGroupId + "_" + (int.Parse(result.InfoResultPayload.UserData["GraduationYear"].Value) - 1).ToString();
        GetSharedGroupData(PlayFabData.AllYearLabSharedGroupId);
        GetSharedGroupData(PlayFabData.MyYearLabSharedGroupId);

        if(result.InfoResultPayload.UserData.Count == 0 || result.InfoResultPayload.UserData.ContainsKey("DisplayName") == false || result.InfoResultPayload.UserData.ContainsKey("GraduationYear") == false)
        {
            // 初期設定の入力画面に遷移
            OnSwitchToInitialSetting();
        }
        else
        {
            if(result.InfoResultPayload.UserData["KeepLoginInfo"].Value == "True") // キャメルケースで取得される
            {
                LinkCustomId(SystemInfo.deviceUniqueIdentifier);
            }
            loginUI.SetActive(false);
            fusion.SetActive(true);
        }
    }

    void OnLoginWithPlayFabFailure(PlayFabError error)
    {
        string errorMessage = "ユーザー名とパスワードでのログイン失敗";
        messageText.SetText(errorMessage);
        messageText.color = Color.red;

        Debug.LogError(errorMessage);
    }

    private void GetSharedGroupData(string groupId)
    {
        var request = new GetSharedGroupDataRequest
        {
            SharedGroupId = groupId
        };
        PlayFabClientAPI.GetSharedGroupData(request, 
        result => {
            if(result.Data.ContainsKey("Players"))
            {
                string jsonData = result.Data["Players"].Value;
                HashSet<string> datas = JsonConvert.DeserializeObject<HashSet<string>>(jsonData);
                if(!datas.Contains(PlayFabSettings.staticPlayer.PlayFabId))
                {
                    SetSharedGroupData(groupId, datas, PlayFabSettings.staticPlayer.PlayFabId, true);
                }
            }
            else
            {
                SetSharedGroupData(groupId, new HashSet<string>(), PlayFabSettings.staticPlayer.PlayFabId, true);
            }

            if(result.Data.ContainsKey("Channels")) // あとで変更
            {
                string jsonData = result.Data["Channels"].Value;

                if(groupId == PlayFabData.AllYearLabSharedGroupId)
                {
                    PlayFabData.AllYearLabChannels = JsonConvert.DeserializeObject<List<ChannelData>>(jsonData);
                }
                else if(groupId == PlayFabData.MyYearLabSharedGroupId)
                {
                    PlayFabData.MyYearLabChannels = JsonConvert.DeserializeObject<List<ChannelData>>(jsonData);
                }
            }
            else
            {
                // generalを作る
                ChannelData channelData = new ChannelData("general", "general", new List<string>(){PlayFabSettings.staticPlayer.PlayFabId});
                List<ChannelData> list = new List<ChannelData>() {channelData};
                string jsonData = JsonConvert.SerializeObject(list);

                var request = new UpdateSharedGroupDataRequest
                {
                    SharedGroupId = groupId,
                    Data = new Dictionary<string, string> { {"Channels", jsonData}}
                };
                PlayFabClientAPI.UpdateSharedGroupData(request, _ => Debug.Log("チャンネル作成成功"), e => Debug.Log("チャンネル作成失敗: " + e.Error));
            }
        }
        , error => Debug.Log("get失敗: " + error.GenerateErrorReport()));
    }

    /// <summary>
    /// 自分のIDを共有グループデータに格納、firstCallは関数内で呼ばれたものかを判断するため
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="players"></param>
    /// <param name="addPlayer"></param>
    /// <param name="firstCall"></param>
    private void SetSharedGroupData(string groupId, HashSet<string> players, string addData, bool firstCall)
    {
        players.Add(addData);
        string jsonData = JsonConvert.SerializeObject(players);
        var request = new UpdateSharedGroupDataRequest
        {
            SharedGroupId = groupId,
            Data = new Dictionary<string, string> {{"Players", jsonData}},
            Permission = UserDataPermission.Public
        };
        PlayFabClientAPI.UpdateSharedGroupData(request,
        _ => Debug.Log("Players更新成功"),
        e => 
        {
            if(e.GenerateErrorReport() == "/Client/UpdateSharedGroupData: NotAuthorized" && firstCall == true) // 共有グループデータのメンバーに追加されていないことによるエラー
            {
                // 共有グループデータの管理者（Lab_Admin）をログインさせ、自分を共有グループデータに追加してもらう（すでにメンバーになっているアカウントから呼ばないと追加できない）
                var request = new LoginWithCustomIDRequest
                {
                    CustomId = PlayFabData.SharedGroupAdminId,
                    CreateAccount = false,
                    TitleId = PlayFabSettings.TitleId
                };
                PlayFabClientAPI.LoginWithCustomID(request, 
                result => 
                {
                    var request = new AddSharedGroupMembersRequest
                    {
                        SharedGroupId = groupId,
                        PlayFabIds = new List<string>(){ addData },
                    };
                    // 追加に成功したらもう一度データの保存を行う
                    PlayFabClientAPI.AddSharedGroupMembers(request, _ => SetSharedGroupData(groupId, players, addData, false), error => Debug.Log("メンバー追加失敗" + error.GenerateErrorReport()));
                },
                error => Debug.Log("メンバー追加失敗" + PlayFabSettings.staticPlayer.PlayFabId + error.Error)
                );
            }
            else
            {
                Debug.Log("Players更新失敗: " + e.GenerateErrorReport());
            }
        });
    }
}
