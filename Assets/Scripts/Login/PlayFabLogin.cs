using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PlayFabLogin : PlayFabLoginAndSignup
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    private bool keepLoginInfo;

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
        Debug.Log("カスタムIDでのログイン成功");

        if(result.InfoResultPayload.UserData["KeepLoginInfo"].Value == "True")
        {
            loginUI.SetActive(false);
            fusion.SetActive(true);
            playFabIdObj.playFabId = result.InfoResultPayload.AccountInfo.PlayFabId;
        }
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

        playFabIdObj.playFabId = result.InfoResultPayload.AccountInfo.PlayFabId;

        if(result.InfoResultPayload.UserData.Count == 0 || string.IsNullOrEmpty(result.InfoResultPayload.UserData["DisplayName"].Value) || string.IsNullOrEmpty(result.InfoResultPayload.UserData["GraduationYear"].Value))
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
