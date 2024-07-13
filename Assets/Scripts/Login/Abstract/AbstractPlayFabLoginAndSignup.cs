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
public class PlayFabLoginAndSignup : MonoBehaviour
{
    [SerializeField] protected GameObject loginUI;
    [SerializeField] protected GameObject signUpUI;
    [SerializeField] protected GameObject initialSettingUI;
    [SerializeField] protected TMP_Text messageText;
    [SerializeField] protected GameObject fusion;
    public GetPlayerCombinedInfoRequestParams playerInfoParams;

    protected string username;
    protected string password;

    private void Awake()
    {
        // playerInfoParams.GetUserAccountInfo = true;
        // playerInfoParams.GetUserData = true;
        // playerInfoParams.GetCharacterList = true;
        // playerInfoParams.GetPlayerProfile = true;
    }

    protected void LinkCustomId(string customId)
    {
        var request = new LinkCustomIDRequest
        {
            CustomId = customId,
            ForceLink = true // 既存のIDにリンクする場合はtrue
        };

        PlayFabClientAPI.LinkCustomID(request, OnLinkCustomIdSuccess, OnLinkCustomIdFailure);
    }

    void OnLinkCustomIdSuccess(LinkCustomIDResult result)
    {
        string successMessage = "カスタムIDの保存に成功";
        Debug.Log(successMessage);
    }

    void OnLinkCustomIdFailure(PlayFabError error)
    {
        string failedMessage = "カスタムIDの保存に失敗";
        Debug.LogError(failedMessage);
    }


    protected void GetSharedGroupData(string groupId)
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
        string jsonDataPlayers = JsonConvert.SerializeObject(players);

        // generalのメンバーに追加
        string jsonDataChannels = "";
        if(groupId == PlayFabData.AllYearLabSharedGroupId && PlayFabData.AllYearLabChannels.Count != 0)
        {
            if(!PlayFabData.AllYearLabChannels[0].MemberIds.Contains(addData))
            {
                PlayFabData.AllYearLabChannels[0].MemberIds.Add(addData);
            }
            jsonDataChannels = JsonConvert.SerializeObject(PlayFabData.AllYearLabChannels);
        }
        else if(groupId == PlayFabData.MyYearLabSharedGroupId && PlayFabData.MyYearLabChannels.Count != 0)
        {
            if(!PlayFabData.MyYearLabChannels[0].MemberIds.Contains(addData))
            {
                PlayFabData.MyYearLabChannels[0].MemberIds.Add(addData);
            }
            jsonDataChannels = JsonConvert.SerializeObject(PlayFabData.MyYearLabChannels);
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
            // playerが1人（自分のみ）の場合はgeneralを作る
            if(players.Count == 1 && firstCall == false)
            {
                ChannelData channelData = new ChannelData("general", "general", new List<string>(){addData});
                List<ChannelData> list = new List<ChannelData>() {channelData};
                string jsonData = JsonConvert.SerializeObject(list);

                var request = new UpdateSharedGroupDataRequest
                {
                    SharedGroupId = groupId,
                    Data = new Dictionary<string, string> { {"Channels", jsonData}}
                };
                PlayFabClientAPI.UpdateSharedGroupData(request, _ => Debug.Log("チャンネル作成成功"), e => Debug.Log("チャンネル作成失敗: " + e.Error));
            }
        },
        e => 
        {
            if(e.GenerateErrorReport() == "/Client/UpdateSharedGroupData: NotAuthorized" && firstCall == true) // 共有グループデータのメンバーに追加されていないことによるエラー
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
                    PlayFabClientAPI.AddSharedGroupMembers(request, _ => SetSharedGroupData(groupId, players, addData, false), error => Debug.Log("メンバー追加失敗" + error.GenerateErrorReport()));
                },
                error => Debug.Log("メンバー追加失敗" + PlayFabSettings.staticPlayer.PlayFabId + error.Error)
                );

                // 元のアカウントでログインし直す
                var request2 = new LoginWithPlayFabRequest
                {
                    Username = username,
                    Password = password,
                    TitleId = PlayFabSettings.TitleId,
                    InfoRequestParameters = playerInfoParams
                };
                PlayFabClientAPI.LoginWithPlayFab(request2, _ => Debug.Log(PlayFabSettings.staticPlayer), _ => e.GenerateErrorReport());

            }
            else
            {
                Debug.Log("Players更新失敗: " + e.GenerateErrorReport());
            }
        });
    }



    public void OnSwitchToLogin()
    {
        signUpUI.SetActive(false);
        initialSettingUI.SetActive(false);
        loginUI.SetActive(true);
        messageText.text = "";
    }

    public void OnSwitchToSignUp()
    {
        loginUI.SetActive(false);
        initialSettingUI.SetActive(false);
        signUpUI.SetActive(true);
        messageText.text = "";
    }

    public void OnSwitchToInitialSetting()
    {
        loginUI.SetActive(false);
        signUpUI.SetActive(false);
        initialSettingUI.SetActive(true);
        messageText.text = "";
    }
}
