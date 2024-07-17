using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoomButton : PlayFabLoginAndSignup
{
    TMP_Text text;
    void Start()
    {
        text = GetComponentInChildren<TMP_Text>();
        messageText.text = "Hello, " + PlayFabData.MyName;
        messageText.color = Color.white;
        ChangeText();
    }

    private void ChangeText()
    {
        if(this.gameObject.name == "AllYearRoomButton")
        {
            text.text = PlayFabData.AllYearLabSharedGroupId;
        }
        if(this.gameObject.name == "MyYearRoomButton")
        {
            text.text = PlayFabData.MyYearLabSharedGroupId;
        }
    }

    public void OnClickAllYearLab()
    {
        PlayFabData.CurrentSharedGroupId = PlayFabData.AllYearLabSharedGroupId;
        GetSharedGroupData(PlayFabData.CurrentSharedGroupId);
        //launcher.Launch();
    }

    public void OnClickMyYearLab()
    {
        PlayFabData.CurrentSharedGroupId = PlayFabData.MyYearLabSharedGroupId;
        GetSharedGroupData(PlayFabData.CurrentSharedGroupId);
        //launcher.Launch();
    }
}
