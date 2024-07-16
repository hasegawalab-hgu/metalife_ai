using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;


/// <summary>
/// ログイン処理やサインアップ処理を行うクラスに継承するクラス
/// </summary>
public class AbstractPlayFabLoginAndSignup : MonoBehaviour
{
    [SerializeField] protected GameObject loginUI;
    [SerializeField] protected GameObject signUpUI;
    [SerializeField] protected GameObject initialSettingUI;
    [SerializeField] protected TMP_Text messageText;
    [SerializeField] protected GameObject fusion;
    public GetPlayerCombinedInfoRequestParams playerInfoParams;

    protected string username;
    protected string password;

    protected void LinkCustomId(string customId)
    {
        var request = new LinkCustomIDRequest
        {
            CustomId = customId,
            ForceLink = true // 既存のIDにリンクする場合はtrue
        };

        PlayFabClientAPI.LinkCustomID(request, _ => Debug.Log("カスタムIDの保存成功"), e => Debug.Log("カスタムIDの保存失敗" + e.GenerateErrorReport()));
    }

    protected void GetSharedGroupData(string groupId)
    {
        var request = new GetSharedGroupDataRequest
        {
            SharedGroupId = groupId
        };
        PlayFabClientAPI.GetSharedGroupData(request, 
            result => {
                if(result.Data.ContainsKey("Channels")) // あとで変更
                {
                    string jsonData = result.Data["Channels"].Value;
                    PlayFabData.CurrentRoomChannels = JsonConvert.DeserializeObject<List<ChannelData>>(jsonData);
                }

                if(result.Data.ContainsKey("Players"))
                {
                    string jsonData = result.Data["Players"].Value;
                    HashSet<string> datas = JsonConvert.DeserializeObject<HashSet<string>>(jsonData);
                    if(!datas.Contains(PlayFabSettings.staticPlayer.PlayFabId))
                    {
                        SetSharedGroupData(groupId, datas, PlayFabSettings.staticPlayer.PlayFabId);
                    }
                }
                else
                {
                    SetSharedGroupData(groupId, new HashSet<string>(), PlayFabSettings.staticPlayer.PlayFabId);
                }
            }
            , error => Debug.Log("get失敗: " + error.GenerateErrorReport()));
    }

    /// <summary>
    /// 自分のIDを共有グループデータに格納
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="players"></param>
    /// <param name="addPlayer"></param>
    /// <param name="firstCall"></param>
    private void SetSharedGroupData(string groupId, HashSet<string> players, string addData)
    {
        players.Add(addData);
        string jsonDataPlayers = JsonConvert.SerializeObject(players);

        // playerが1人（自分のみ）の場合はgeneralを作る
        if(players.Count == 1 && PlayFabData.CurrentRoomChannels.Count == 0)
        {
            ChannelData channelData = new ChannelData("general", "general", new List<string>(){addData});
            PlayFabData.CurrentRoomChannels.Add(channelData);
            List<ChannelData> list = new List<ChannelData>() {channelData};
            string jsonData = JsonConvert.SerializeObject(list);

            var request2 = new UpdateSharedGroupDataRequest
            {
                SharedGroupId = groupId,
                Data = new Dictionary<string, string> { {"Channels", jsonData}}
            };
            PlayFabClientAPI.UpdateSharedGroupData(request2, _ => Debug.Log("チャンネル作成成功"), e => Debug.Log("チャンネル作成失敗: " + e.Error));
        }

        // generalのメンバーに追加
        string jsonDataChannels = JsonConvert.SerializeObject(PlayFabData.CurrentRoomChannels);
        if(PlayFabData.CurrentRoomChannels.Count != 0 && PlayFabData.CurrentRoomChannels != new List<ChannelData>())
        {
            if(!PlayFabData.CurrentRoomChannels[0].MemberIds.Contains(addData))
            {
                PlayFabData.CurrentRoomChannels[0].MemberIds.Add(addData);
            }
            jsonDataChannels = JsonConvert.SerializeObject(PlayFabData.CurrentRoomChannels);
        }

        var request = new UpdateSharedGroupDataRequest
        {
            SharedGroupId = groupId,
            Data = new Dictionary<string, string> {{"Players", jsonDataPlayers}, {"Channels", jsonDataChannels}},
            Permission = UserDataPermission.Public
        };
        PlayFabClientAPI.UpdateSharedGroupData(request,
            result => 
            {
                Debug.Log("Players更新成功");
            },
            e => 
            {
                if(e.GenerateErrorReport() == "/Client/UpdateSharedGroupData: NotAuthorized") // 共有グループデータのメンバーに追加されていないことによるエラー
                {
                    // 共有グループデータの管理者（Lab_Admin）をログインさせ、自分を共有グループデータに追加してもらう（すでにメンバーになっているアカウントから呼ばないと追加できない）
                    var request = new LoginWithCustomIDRequest
                    {
                        CustomId = PlayFabData.SharedGroupAdminId,
                        CreateAccount = false,
                        TitleId = PlayFabSettings.TitleId,
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
                            PlayFabClientAPI.AddSharedGroupMembers(request, 
                                _ => 
                                {
                                    // ログイン情報を削除する
                                    PlayFabSettings.staticPlayer.ForgetAllCredentials();
                                    // ログインし直す
                                    OnSwitchToLogin();
                                }, 
                                error => Debug.Log("メンバー追加失敗" + error.GenerateErrorReport())
                            );
                        },
                        error => 
                        {
                            Debug.Log("adminログイン追加失敗" + PlayFabSettings.staticPlayer.PlayFabId + error.Error);
                        }
                    );
                }
                else
                {
                    Debug.Log("Players更新失敗: " + e.GenerateErrorReport());
                }
            }
        );
    }

    public void OnSwitchToLogin()
    {
        signUpUI.SetActive(false);
        initialSettingUI.SetActive(false);
        fusion.SetActive(false);
        loginUI.SetActive(true);
        messageText.text = "";
    }

    public void OnSwitchToSignUp()
    {
        loginUI.SetActive(false);
        initialSettingUI.SetActive(false);
        fusion.SetActive(false);
        signUpUI.SetActive(true);
        messageText.text = "";
    }

    public void OnSwitchToInitialSetting()
    {
        loginUI.SetActive(false);
        signUpUI.SetActive(false);
        initialSettingUI.SetActive(true);
        fusion.SetActive(false);
        messageText.text = "";
    }
}
