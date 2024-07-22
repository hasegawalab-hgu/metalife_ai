using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChannelButton : MonoBehaviour
{
    public ChannelData channelData; // 生成される時に割り当てられる
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

        if (PlayFabData.CurrentRoomChannels[channelData.ChannelId].ChannelType == "Public")
        {
            chatUIManager.text_channelName.text = "# " + channelData.ChannelName;
        }
        else
        {
            chatUIManager.text_channelName.text = channelData.ChannelName;
        }
        PlayFabData.CurrentChannelId = channelData.ChannelId;
        PlayFabData.CurrentMessageTarget = "All";

        chatUIManager.DestroyChildren(chatUIManager.spawner_message.transform);
        foreach(MessageData messagedata in messageDatas)
        {
            chatUIManager.DisplayMessage(messagedata);
        }
    }
}
