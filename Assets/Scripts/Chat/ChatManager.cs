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

    public ChannelData(string chId, string chName, List<string> menberIds)
    {
        ChannelId = chId;
        ChannelName = chName;
        MemberIds = menberIds;
    }
}

public class ChatManager : NetworkBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;
    [SerializeField]
    private TMP_Text outputField;

    private ChannelData channelData; // 追加するチャンネルデータ

    private MessageData receivedMessageData;


    private void Start()
    {
        // CreatChannel("testChannel", "test", new string[] {"a"});
        // RPC_SendMessage(PlayFabSettings.staticPlayer.PlayFabId, PlayFabSettings.staticPlayer.PlayFabId, "general", "aaaa");
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

    public void CreatChannel(string channelId, string channelName, List<string> menberIds)
    {
        channelData = new ChannelData(channelId, channelName, menberIds);
        
        var request = new GetSharedGroupDataRequest
        {
            SharedGroupId = PlayFabData.CurrentSharedGroupId
        };
        PlayFabClientAPI.GetSharedGroupData(request, OnGetChannelDatasSuccess, error => Debug.Log(error.GenerateErrorReport()));
    }

    public void OnGetChannelDatasSuccess(GetSharedGroupDataResult result)
    {
        if (result.Data.ContainsKey("Channels") == false)
        {
            AddChannel(new List<ChannelData>{}, channelData);
        }
        else
        {
            string jsonData = result.Data["Channels"].Value;
            List<ChannelData> channels = JsonConvert.DeserializeObject<List<ChannelData>>(jsonData);
            AddChannel(channels, channelData);
        }
    }

    public void AddChannel(List<ChannelData> channelDatas, ChannelData data)
    {
        channelDatas.Add(data);
        string jsonData = JsonConvert.SerializeObject(channelDatas);

        var request = new UpdateSharedGroupDataRequest
        {
            SharedGroupId = PlayFabData.CurrentSharedGroupId,
            Data = new Dictionary<string, string> { { "Channels", jsonData } },
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateSharedGroupData(request, _ => Debug.Log("channelData保存成功" + channelData.ChannelId), error => Debug.Log(error.GenerateErrorReport()));
    }
}
