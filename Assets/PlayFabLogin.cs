using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PlayFabLogin : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private GameObject loginUI;
    [SerializeField] private GameObject SignUpUI;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject fusion;

    private void Awake()
    {
        OnLoginWithDevice();
    }

    void OnLoginWithDevice()
    {
        var customId = SystemInfo.deviceUniqueIdentifier;

        // カスタムIDでログインを試みる
        var customIdRequest = new LoginWithCustomIDRequest
        {
            CustomId = customId,
            CreateAccount = false // アカウントを新規作成しない
        };

        PlayFabClientAPI.LoginWithCustomID(customIdRequest, OnLoginWithCustomIDSuccess, OnLoginWithCustomIDFailure);
    }

    void OnLoginWithCustomIDSuccess(LoginResult result)
    {
        transform.gameObject.SetActive(false);
        fusion.SetActive(true);
        Debug.Log("カスタムIDでのログイン成功");
        // ログイン成功時の処理
    }

    void OnLoginWithCustomIDFailure(PlayFabError error)
    {
        Debug.Log("カスタムIDでのログイン失敗。PlayFabアカウントでログインしてください。: " + error.GenerateErrorReport());
    }

    // カスタムIDでのログインに失敗した場合は、ユーザー名とパスワードでログイン
    public void OnLoginWIthPlayFab()
    {
        var request = new LoginWithPlayFabRequest
        {
            Username = usernameInput.text,
            Password = passwordInput.text
        };

        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginWithPlayFabSuccess, OnLoginWithPlayFabFailure);
    }



    void OnLoginWithPlayFabSuccess(LoginResult result)
    {
        Debug.Log("ユーザー名とパスワードでのログイン成功");

        // カスタムIDを保存
        var customId = SystemInfo.deviceUniqueIdentifier;
        LinkCustomId(customId);
    }

    void OnLoginWithPlayFabFailure(PlayFabError error)
    {
        string errorMessage = "ユーザー名とパスワードでのログイン失敗: " + error.GenerateErrorReport().Split(':')[2];
        messageText.SetText(errorMessage);
        messageText.color = Color.red;

        Debug.LogError(errorMessage);
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
        transform.parent.gameObject.SetActive(false);
        fusion.SetActive(true);

    }

    void OnLinkCustomIdFailure(PlayFabError error)
    {
        string failedMessage = "カスタムIDの保存に失敗: " + error.GenerateErrorReport().Split(' ')[2];
        Debug.LogError(failedMessage);
        messageText.SetText(failedMessage);
        messageText.color = Color.red;
    }

    public void OnSwitchToSignUp()
    {
        loginUI.SetActive(false);
        SignUpUI.SetActive(true);
    }
}
