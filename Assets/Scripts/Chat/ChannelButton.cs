using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class ChannelButton : MonoBehaviour
{
    public ChannelData channelData; // 生成される時に割り当てられる
    private ChatUIManager chatUIManager;

    private List<MessageData> masageDatas = new List<MessageData>();


    void Start()
    {
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
        GetComponent<Button>().onClick.AddListener(OnClickButton);
    }

    public void OnClickButton()
    {
        PlayFabData.CurrentChannelId = channelData.ChannelId;

        if (PlayFabData.CurrentRoomChannels[PlayFabData.CurrentChannelId].ChannelType == "Public")
        {
            chatUIManager.text_channelName.text = "# " + channelData.ChannelName;
        }
        else
        {
            chatUIManager.text_channelName.text = channelData.ChannelName;
        }
    }
}
