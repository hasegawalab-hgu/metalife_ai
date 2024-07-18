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

public class ChatManager : NetworkBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;

    private ChatUIManager chatUIManager;
    [SerializeField]
    private TMP_Text outputField;

    private ChannelData addChannelData; // 追加するチャンネルデータ

    private MessageData receivedMessageData;


    private void Start()
    {
        chatUIManager = GetComponent<ChatUIManager>();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SendMessage(string senderId, string receiverId, string channelId, string content)
    {
        var timestamp = DateTime.UtcNow.ToString("o");
        var messageData = new MessageData
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            ChannelId = channelId,
            Content = content,
            Timestamp = timestamp
        };

        // ローカルデータベースに保存する
        receivedMessageData = messageData;
        Debug.Log(receivedMessageData.Content);
        // メッセージ表示の処理など
    }

    public void CreatChannel(string channelId, string channelName, List<string> menberIds, string channelType)
    {
        addChannelData = new ChannelData(channelId, channelName, menberIds, channelType);
        AddChannel(addChannelData);
        /*
        var request = new GetSharedGroupDataRequest
        {
            SharedGroupId = PlayFabData.CurrentSharedGroupId
        };
        PlayFabClientAPI.GetSharedGroupData(request, OnGetChannelDatasSuccess, error => Debug.Log(error.GenerateErrorReport()));
        */
    }

    /*
    public void OnGetChannelDatasSuccess(GetSharedGroupDataResult result)
    {
        if (result.Data.ContainsKey("Channels") == false)
        {
            AddChannel(channelData);
        }
        else
        {
            string jsonData = result.Data["Channels"].Value;
            PlayFabData.CurrentRoomChannels = JsonConvert.DeserializeObject<Dictionary<string, ChannelData>>(jsonData);
            AddChannel(channelData);
        }
    }
    */
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
