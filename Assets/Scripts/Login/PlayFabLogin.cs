using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.GroupsModels;
using TMPro;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine.UI;

public class PlayFabLogin : PlayFabLoginAndSignup
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Toggle KeepLoginInfo;
    [SerializeField] private TMP_InputField chatGPTApiKeyInput;
    private string ChatGPTApiKey;

    void Start()
    {
        usernameInput.text = "";
        passwordInput.text = "";
        KeepLoginInfo.isOn = false;
        ChatGPTApiKey = PlayerPrefs.GetString("GATHER_LAB_GPT_KEY", "");
        if(!string.IsNullOrEmpty(ChatGPTApiKey))
        {
            chatGPTApiKeyInput.gameObject.SetActive(false);
        }
        if (PlayFabData.Islogouted == false)
        {
            OnLoginWithDevice();
        }
        else
        {
            PlayFabData.Islogouted = false;
        }
    }

    void OnLoginWithDevice()
    {
        if(string.IsNullOrEmpty(ChatGPTApiKey))
        {
            if(string.IsNullOrEmpty(chatGPTApiKeyInput.text))
            {
                Debug.Log("ChatGPTのAPIKeyが入力されていません");
                messageText.SetText("ChatGPTのAPIKeyが入力されていません");
                return;
            }
            else
            {
                PlayerPrefs.SetString("GATHER_LAB_GPT_KEY", chatGPTApiKeyInput.text);
                PlayerPrefs.Save();
            }
        }

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
        PlayFabData.MyName = result.InfoResultPayload.UserData["DisplayName"].Value;
        PlayFabData.MyGraduationYear = result.InfoResultPayload.UserData["GraduationYear"].Value;

        Debug.Log("カスタムIDでのログイン成功");
        PlayFabData.MyYearLabSharedGroupId = PlayFabData.AllYearLabSharedGroupId + "_" + (int.Parse(result.InfoResultPayload.UserData["GraduationYear"].Value) - 1).ToString();
        
        if(result.InfoResultPayload.UserData.ContainsKey("DictReadMessageCount"))
        {
            PlayFabData.DictReadMessageCount = JsonConvert.DeserializeObject<Dictionary<string, int>>(result.InfoResultPayload.UserData["DictReadMessageCount"].Value);
        }
        if(result.InfoResultPayload.UserData.ContainsKey("CharacterPath"))
        {
            // PlayFabData.MyTexturePath = result.InfoResultPayload.UserData["CharacterPath"].Value;
        }

        if(result.InfoResultPayload.UserData.ContainsKey("Login"))
        {
            PlayFabData.DictLoginTime = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.InfoResultPayload.UserData["Login"].Value);
        }

        int now = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds + 3600 * 9; // 日本時間
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(now);
        PlayFabData.LoginStartTime = dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss");
        int time = Mathf.FloorToInt(PlayFabData.LoginTime);
        int hours = time / 3600;
        int minutes = (time % 3600) / 60;
        int secs = time % 60;

        // フォーマット
        string timeStr = $"{hours:D2}:{minutes:D2}:{secs:D2}";
        PlayFabData.DictLoginTime[PlayFabData.LoginStartTime] = timeStr;

        /*
        if (result.InfoResultPayload.UserData["IsOnline"].Value == "True")
        {
            messageText.text = "このアカウントは現在別の端末でログインされています。";
            messageText.color = Color.red;
        }
        else
        {
            loginUI.SetActive(false);
            fusion.SetActive(true);
        }
        */

        loginUI.SetActive(false);
        fusion.SetActive(true);
    }

    // カスタムIDでのログインに失敗した場合は、ユーザー名とパスワードでログイン
    public void OnLoginWIthPlayFab()
    {
        if(string.IsNullOrEmpty(ChatGPTApiKey))
        {
            if(string.IsNullOrEmpty(chatGPTApiKeyInput.text))
            {
                Debug.Log("ChatGPTのAPIKeyが入力されていません");
                messageText.SetText("ChatGPTのAPIKeyが入力されていません");
                return;
            }
            else
            {
                PlayerPrefs.SetString("GATHER_LAB_GPT_KEY", chatGPTApiKeyInput.text);
                PlayerPrefs.Save();
            }
        }

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

        if(KeepLoginInfo.isOn == true)
        {
            LinkCustomId(SystemInfo.deviceUniqueIdentifier);
        }
        else
        {
            UnlinkCustomId(SystemInfo.deviceUniqueIdentifier);
        }

        if(result.NewlyCreated == true || result.InfoResultPayload.UserData.Count == 0 || result.InfoResultPayload.UserData.ContainsKey("DisplayName") == false || result.InfoResultPayload.UserData.ContainsKey("GraduationYear") == false)
        {
            // 初期設定の入力画面に遷移
            OnSwitchToInitialSetting();
        }
        else
        {
            PlayFabData.MyYearLabSharedGroupId = PlayFabData.AllYearLabSharedGroupId + "_" + (int.Parse(result.InfoResultPayload.UserData["GraduationYear"].Value) - 1).ToString();

            // PlayFabData.MyName = result.InfoResultPayload.UserData["DisplayName"].Value;
            PlayFabData.MyGraduationYear = result.InfoResultPayload.UserData["GraduationYear"].Value;
            if(result.InfoResultPayload.UserData.ContainsKey("DictReadMessageCount"))
            {
                PlayFabData.DictReadMessageCount = JsonConvert.DeserializeObject<Dictionary<string, int>>(result.InfoResultPayload.UserData["DictReadMessageCount"].Value);
            }
            if(result.InfoResultPayload.UserData.ContainsKey("CharacterPath"))
            {
                // PlayFabData.MyTexturePath = result.InfoResultPayload.UserData["CharacterPath"].Value;
            }

            if(result.InfoResultPayload.UserData.ContainsKey("Login"))
            {
                PlayFabData.DictLoginTime = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.InfoResultPayload.UserData["Login"].Value);
            }

            int now = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds + 3600 * 9; // 日本時間
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(now);
            PlayFabData.LoginStartTime = dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss");
            int time = Mathf.FloorToInt(PlayFabData.LoginTime);
            int hours = time / 3600;
            int minutes = (time % 3600) / 60;
            int secs = time % 60;

            // フォーマット
            string timeStr = $"{hours:D2}:{minutes:D2}:{secs:D2}";
            PlayFabData.DictLoginTime[PlayFabData.LoginStartTime] = timeStr;

            Debug.Log("カスタムIDでのログイン成功 " + PlayFabData.MyName);

            if (result.InfoResultPayload.UserData.ContainsKey("IsOnline"))
            {
                /*
                if(result.InfoResultPayload.UserData["IsOnline"].Value == "True")
                {
                    messageText.text = "このアカウントは現在別の端末でログインされています。";
                    messageText.color = Color.red;
                }
                else
                {
                    loginUI.SetActive(false);
                    fusion.SetActive(true);
                }
                */
                loginUI.SetActive(false);
                fusion.SetActive(true);
            }
            else
            {
                loginUI.SetActive(false);
                fusion.SetActive(true);
            }
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
