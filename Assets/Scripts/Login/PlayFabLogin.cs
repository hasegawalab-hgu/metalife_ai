using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PlayFabLogin : PlayFabLoginAndSignup
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;

    void Start()
    {
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
            InfoRequestParameters = playerInfoParams
        };

        PlayFabClientAPI.LoginWithCustomID(customIdRequest, OnLoginWithCustomIDSuccess, OnLoginWithCustomIDFailure);
    }

    void OnLoginWithCustomIDSuccess(LoginResult result)
    {
        loginUI.SetActive(false);
        fusion.SetActive(true);
        Debug.Log("カスタムIDでのログイン成功");

        PlayerData.PlayFabId = result.InfoResultPayload.AccountInfo.PlayFabId;
    }

    void OnLoginWithCustomIDFailure(PlayFabError error)
    {
        Debug.Log("カスタムIDでのログイン失敗。PlayFabアカウントでログインしてください。: ");
    }

    // カスタムIDでのログインに失敗した場合は、ユーザー名とパスワードでログイン
    public void OnLoginWIthPlayFab()
    {
        var request = new LoginWithPlayFabRequest
        {
            Username = usernameInput.text,
            Password = passwordInput.text,
            InfoRequestParameters = playerInfoParams
        };

        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginWithPlayFabSuccess, OnLoginWithPlayFabFailure);
    }



    void OnLoginWithPlayFabSuccess(LoginResult result)
    {
        Debug.Log("ユーザー名とパスワードでのログイン成功");

        PlayerData.PlayFabId = result.InfoResultPayload.AccountInfo.PlayFabId;

        if(string.IsNullOrEmpty(result.InfoResultPayload.UserData["DisplayName"].Value) || string.IsNullOrEmpty(result.InfoResultPayload.UserData["GraduationYear"].Value))
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
}
