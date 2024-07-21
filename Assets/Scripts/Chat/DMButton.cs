using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DMButton : MonoBehaviour
{
    public string myName;
    public string myId;

    public GameObject playerInstance;

    private ChatUIManager chatUIManager;
    public string beforeSendText;

    public List<MessageData> messageDatas = new List<MessageData>();

    void Start()
    {
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
        GetComponent<Button>().onClick.AddListener(OnClickButton);
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
}
