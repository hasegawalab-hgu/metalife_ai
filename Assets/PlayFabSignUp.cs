using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabSignUp : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject loginUI;
    [SerializeField] private GameObject signUpUI;

    public void OnRegisterButtonClicked()
    {
        var request = new RegisterPlayFabUserRequest
        {
            Username = usernameInput.text,
            Password = passwordInput.text,
            Email = emailInput.text
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterFailure);
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        string successMessage = "Success";
        Debug.Log(successMessage);
        messageText.SetText(successMessage);
        messageText.color = Color.green;

        // カスタムID（デバイスID）を保存
        var customId = SystemInfo.deviceUniqueIdentifier;
        LinkCustomId(customId);
    }


    void LinkCustomId(string customId)
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
        OnSwitchToLogin();
    }

    void OnLinkCustomIdFailure(PlayFabError error)
    {
        string failedMessage = "カスタムIDの保存に失敗: " + error.GenerateErrorReport().Split(' ')[2];
        Debug.LogError(failedMessage);
        messageText.SetText(failedMessage);
        messageText.color = Color.red;
    }


    void OnRegisterFailure(PlayFabError error)
    {
        string failedMessage = "Failed: " + error.GenerateErrorReport().Split(':')[2];
        Debug.LogError(failedMessage);
        messageText.SetText(failedMessage);
        messageText.color = Color.red;
    }

    public void OnSwitchToLogin()
    {
        signUpUI.SetActive(false);
        loginUI.SetActive(true);
    }
}

