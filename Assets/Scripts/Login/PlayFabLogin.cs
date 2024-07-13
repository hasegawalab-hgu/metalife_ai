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
            messageText.text = PlayFabData.CurrentSharedGroupId + "に入室";
            loginUI.SetActive(false);
            fusion.SetActive(true);
            PlayFabSettings.staticPlayer.PlayFabId = result.InfoResultPayload.AccountInfo.PlayFabId;
        }
    }

    // カスタムIDでのログインに失敗した場合は、ユーザー名とパスワードでログイン
    public void OnLoginWIthPlayFab()
    {
        username = usernameInput.text;
        password = passwordInput.text;

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

        if(result.NewlyCreated == true || result.InfoResultPayload.UserData.Count == 0 || result.InfoResultPayload.UserData.ContainsKey("DisplayName") == false || result.InfoResultPayload.UserData.ContainsKey("GraduationYear") == false)
        {
            // 初期設定の入力画面に遷移
            OnSwitchToInitialSetting();
        }
        else
        {
            PlayFabData.MyYearLabSharedGroupId = PlayFabData.AllYearLabSharedGroupId + "_" + (int.Parse(result.InfoResultPayload.UserData["GraduationYear"].Value) - 1).ToString();
            GetSharedGroupData(PlayFabData.AllYearLabSharedGroupId);
            GetSharedGroupData(PlayFabData.MyYearLabSharedGroupId);

            if(result.InfoResultPayload.UserData["KeepLoginInfo"].Value == "True") // キャメルケースで取得される
            {
                LinkCustomId(SystemInfo.deviceUniqueIdentifier);
            }
            messageText.text = PlayFabData.CurrentSharedGroupId + "に入室";
            loginUI.SetActive(false);
            fusion.SetActive(true);
        }
    }

    void OnLoginWithPlayFabFailure(PlayFabError error)
    {
        string errorMessage = "ユーザー名とパスワードでのログイン失敗\n" + error.Error;
        messageText.SetText(errorMessage);
        messageText.color = Color.red;

        Debug.LogError(errorMessage);
    }
}
