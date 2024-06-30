using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabSignUp : PlayFabLoginAndSignup
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField emailInput;

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

        OnSwitchToLogin();
    }


    void OnRegisterFailure(PlayFabError error)
    {
        string failedMessage = "Failed: " + error.GenerateErrorReport().Split(':')[2];
        Debug.LogError(failedMessage);
        messageText.SetText(failedMessage);
        messageText.color = Color.red;
    }
}

