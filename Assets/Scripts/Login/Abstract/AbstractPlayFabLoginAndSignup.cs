using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;


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

    public PlayerData playerData {get; set;}

    private void Awake()
    {
        // playerInfoParams.GetUserAccountInfo = true;
        // playerInfoParams.GetUserData = true;
        // playerInfoParams.GetCharacterList = true;
        // playerInfoParams.GetPlayerProfile = true;


        //playerData = GameObject.Find("PlayerData").GetComponent<PlayerData>();
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


    public void OnSwitchToLogin()
    {
        signUpUI.SetActive(false);
        initialSettingUI.SetActive(false);
        loginUI.SetActive(true);
    }

    public void OnSwitchToSignUp()
    {
        loginUI.SetActive(false);
        initialSettingUI.SetActive(false);
        signUpUI.SetActive(true);
    }

    public void OnSwitchToInitialSetting()
    {
        loginUI.SetActive(false);
        signUpUI.SetActive(false);
        initialSettingUI.SetActive(true);
    }
}
