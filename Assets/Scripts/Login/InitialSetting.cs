using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;
using PlayFab.MultiplayerModels;

public class InitialSetting : PlayFabLoginAndSignup
{
    [SerializeField] public TMP_InputField displayNameInput;
    [SerializeField] private TMP_InputField graduationYearInput;
    // [SerializeField] private Toggle keepLoginInfo;

    public void OnSaveInitialSetting()
    {
        if(string.IsNullOrEmpty(displayNameInput.text) || displayNameInput.text.Length > 20)
        {
            messageText.SetText("表示名を20文字以下で入力してください");
            messageText.color = Color.red;
        }
        else if(string.IsNullOrEmpty(graduationYearInput.text) || graduationYearInput.text.Length != 4)
        {
            messageText.SetText("卒業年度を半角英数字４桁で入力してください");
            messageText.color = Color.red;
        }
        else
        {   
            PlayFabData.MyName = displayNameInput.text;
            PlayFabData.MyGraduationYear = graduationYearInput.text;
            SetUserData();
        }
    }

    private void SetDisplayName()
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest{DisplayName = displayNameInput.text}, result => Debug.Log("DisplayNameの変更成功"),error => Debug.Log("DisplayNameの変更失敗" + error.GenerateErrorReport()));
    }

    private void SetUserData()
    {
        var request = new UpdateUserDataRequest
        {
            
            Data = new Dictionary<string, string>
            {
                {"DisplayName", displayNameInput.text},
                {"GraduationYear", graduationYearInput.text},
            }
        };
        PlayFabClientAPI.UpdateUserData(request, result => OnSetUserDataSuccess(), error => {Debug.Log("プレイヤーデータの更新失敗" + error.GenerateErrorReport());});
    }

    private void OnSetUserDataSuccess()
    {
        SetDisplayName();
        Debug.Log("プレイヤーデータの更新成功");
        PlayFabData.CurrentSharedGroupId = PlayFabData.AllYearLabSharedGroupId + graduationYearInput.text;

        initialSettingUI.SetActive(false);
        fusion.SetActive(true);
    }
}
