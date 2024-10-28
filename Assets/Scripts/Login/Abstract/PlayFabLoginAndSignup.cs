using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using System.Linq;


/// <summary>
/// ログイン処理やサインアップ処理を行うクラスに継承するクラス
/// </summary>
public class PlayFabLoginAndSignup : MonoBehaviour
{
    [SerializeField] protected GameObject loginUI;
    [SerializeField] protected GameObject signUpUI;
    [SerializeField] protected GameObject initialSettingUI;
    [SerializeField] protected TMP_Text messageText;
    [SerializeField] protected GameObject fusion;
    public GetPlayerCombinedInfoRequestParams playerInfoParams;


    protected Launcher launcher;

    void Awake()
    {
        launcher = GameObject.Find("FusionLauncher").GetComponent<Launcher>();
    }

    protected void LinkCustomId(string customId)
    {
        var request = new LinkCustomIDRequest
        {
            CustomId = customId,
            ForceLink = true 
        };

        PlayFabClientAPI.LinkCustomID(request, _ => Debug.Log("カスタムIDの保存成功"), e => Debug.Log("カスタムIDの保存失敗" + e.GenerateErrorReport()));
    }

    protected void UnlinkCustomId(string customId)
    {
        var request = new UnlinkCustomIDRequest
        {
            CustomId = customId,
        };

        PlayFabClientAPI.UnlinkCustomID(request, _ => Debug.Log("カスタムIDの解除成功"), e => Debug.Log("カスタムIDの解除失敗" + e.GenerateErrorReport()));
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
                    PlayFabData.CurrentRoomChannels = JsonConvert.DeserializeObject<Dictionary<string, ChannelData>>(jsonData);
                }

                if(result.Data.ContainsKey("PlayerInfos"))
                {
                    string jsonData = result.Data["PlayerInfos"].Value;
                    PlayFabData.DictPlayerInfos = JsonConvert.DeserializeObject<Dictionary<string, PlayerInfo>>(jsonData);
                    PlayFabData.MyName = PlayFabData.DictPlayerInfos[PlayFabSettings.staticPlayer.PlayFabId].name;
                    PlayFabData.MyTexturePath = PlayFabData.DictPlayerInfos[PlayFabSettings.staticPlayer.PlayFabId].texturePath;

                    if(!PlayFabData.DictPlayerInfos.ContainsKey(PlayFabSettings.staticPlayer.PlayFabId))
                    {
                        SetSharedGroupData(groupId, PlayFabSettings.staticPlayer.PlayFabId, PlayFabData.MyName, "");
                    }
                    else
                    {
                        if(PlayFabData.DictPlayerInfos[PlayFabSettings.staticPlayer.PlayFabId].name != PlayFabData.MyName)
                        {
                            SetSharedGroupData(groupId, PlayFabSettings.staticPlayer.PlayFabId, PlayFabData.MyName, PlayFabData.MyTexturePath);
                        }
                        else
                        {
                            launcher.Launch();
                        }
                    }
                }
                else
                {
                    SetSharedGroupData(groupId, PlayFabSettings.staticPlayer.PlayFabId, PlayFabData.MyName, "");
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
    private void SetSharedGroupData(string groupId, string id, string playerName, string texturePath)
    {
        if(!PlayFabData.DictPlayerInfos.ContainsKey(id))
        {
            PlayFabData.DictPlayerInfos[id] = new PlayerInfo{id = id, name = playerName, texturePath = texturePath};
        }
        string jsonDataPlayers = JsonConvert.SerializeObject(PlayFabData.DictPlayerInfos);

        // playerが1人（自分のみ）の場合はgeneralを作る
        if(PlayFabData.DictPlayerInfos.Count == 1 && PlayFabData.CurrentRoomChannels.Count == 0)
        {
            ChannelData channelData = new ChannelData("general", "general", new List<string>(){ id }, "Public");
            PlayFabData.CurrentRoomChannels.Add(channelData.ChannelId, channelData);
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
        if(PlayFabData.CurrentRoomChannels.Count != 0 && PlayFabData.CurrentRoomChannels != new Dictionary<string, ChannelData>())
        {
            Dictionary<string, ChannelData> list = new Dictionary<string , ChannelData>();
            foreach(var tmp in PlayFabData.CurrentRoomChannels)
            {
                if(tmp.Value.ChannelType == "Public" && tmp.Value.MemberIds.Contains(id) == false)
                {
                    tmp.Value.MemberIds.Add(id);
                }
                list.Add(tmp.Key, tmp.Value);
            }
            PlayFabData.CurrentRoomChannels = list;
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
                launcher.Launch();
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
                                PlayFabIds = new List<string>(){ id },
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
