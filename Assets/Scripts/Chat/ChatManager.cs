using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using System;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;

[Serializable]
public class MessageData
{
    public string Timestamp;
    public string ChannelId;
    public string SenderId;
    public string ReceiverId;
    public string Content;
}

[Serializable]
public class ChannelData
{
    public string ChannelId;
    public string ChannelName;
    public List<string> MemberIds;
    public string ChannelType;

    public ChannelData(string chId, string chName, List<string> menberIds, string chType)
    {
        ChannelId = chId;
        ChannelName = chName;
        MemberIds = menberIds;
        ChannelType = chType;
    }
}

public class ChatManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;

    private ChatUIManager chatUIManager;
    [SerializeField]
    private TMP_Text outputField;

    private ChannelData addChannelData; // 追加するチャンネルデータ

    public ChatSender chatSender;


    private void Start()
    {
        chatUIManager = GetComponent<ChatUIManager>();
    }

    public void CreatChannel(string channelId, string channelName, List<string> menberIds, string channelType)
    {
        addChannelData = new ChannelData(channelId, channelName, menberIds, channelType);
        AddChannel(addChannelData);
    }

    private void AddChannel(ChannelData data)
    {
        PlayFabData.CurrentRoomChannels.Add(data.ChannelId, data);
        string jsonData = JsonConvert.SerializeObject(PlayFabData.CurrentRoomChannels);

        var request = new UpdateSharedGroupDataRequest
        {
            SharedGroupId = PlayFabData.CurrentSharedGroupId,
            Data = new Dictionary<string, string> { { "Channels", jsonData } },
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateSharedGroupData(
            request, 
            _ => 
                {
                    Debug.Log("channelData保存成功" + addChannelData.ChannelId);
                    chatUIManager.OnClickReturn();
                    chatUIManager.DisplayChannelTargets();
                },
            error => Debug.Log(error.GenerateErrorReport()));
    }
}
