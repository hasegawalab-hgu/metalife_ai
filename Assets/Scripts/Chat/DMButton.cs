using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using UnityEditor;
using Newtonsoft.Json;

public class DMButton : MonoBehaviour
{
    public string myName;
    public string myId;

    public GameObject playerInstance;

    private ChatUIManager chatUIManager;
    public string beforeSendText;

    private string key;

    public List<MessageData> messageDatas = new List<MessageData>();

    void Start()
    {
        int result = string.Compare(PlayFabSettings.staticPlayer.PlayFabId, myId);
        if(result == 0)
        {
            key = myId;
        }
        else if(result == -1)
        {
            key = PlayFabSettings.staticPlayer.PlayFabId + "+" + myId;
        }
        else if(result == 1)
        {
            key = myId + "+" + PlayFabSettings.staticPlayer.PlayFabId;
        }
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
        GetComponent<Button>().onClick.AddListener(OnClickButton);
        GetSharedGroupData(true);
    }

    public void OnClickButton()
    {
        if(PlayFabData.CurrentChannelId == "DM")
        {
            PlayFabData.DictDMScripts[PlayFabData.CurrentMessageTarget].beforeSendText = chatUIManager.inputField.text;
            chatUIManager.inputField.text = "";
        }
        else if(PlayFabData.CurrentMessageTarget == "All")
        {
            PlayFabData.DictChannelScripts[PlayFabData.CurrentChannelId].beforeSendText = chatUIManager.inputField.text;
            chatUIManager.inputField.text = "";
        }
        
        if(!string.IsNullOrEmpty(beforeSendText))
        {
            chatUIManager.inputField.text = beforeSendText;
        }
        
        PlayFabData.CurrentChannelId = "DM";
        PlayFabData.CurrentMessageTarget = myId;
        chatUIManager.text_channelName.text = "DM : " + myName;

        chatUIManager.DestroyChildren(chatUIManager.spawner_message.transform);
        foreach (var messageData in messageDatas)
        {
            chatUIManager.DisplayMessage(messageData);
        }
    }

    private void GetSharedGroupData(bool calledByStart)
    {
        var request = new GetSharedGroupDataRequest
        {
            SharedGroupId = PlayFabData.CurrentSharedGroupId,
        };

        PlayFabClientAPI.GetSharedGroupData(request, 
            result => 
            {
                if (!string.IsNullOrEmpty(key) & result.Data.ContainsKey(key))
                {
                    messageDatas = JsonConvert.DeserializeObject<List<MessageData>>(result.Data[key].Value);
                    if(PlayFabData.CurrentMessageTarget == myId && calledByStart)
                    {
                        OnClickButton();
                    }
                }
            }, 
            e => e.GenerateErrorReport()
        );
    }
}
