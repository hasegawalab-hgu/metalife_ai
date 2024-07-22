using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;


/// <summary>
/// メッセージをRPCで送信する、RPCをしようするためローカルプレイヤーに割り当てる
/// </summary>
public class ChatSender : NetworkBehaviour
{
    private ChatManager chatManager;
    private ChatUIManager chatUIManager;

    void Start()
    {
        chatManager = GameObject.Find("ChatManager").GetComponent<ChatManager>();
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SendMessageRequest(string senderId, string receiverId, string channelId, string content, string timestamp)
    {
        if(HasStateAuthority)
        {
            RPC_SendMessage(senderId, receiverId, channelId, content, timestamp);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SendMessage(string senderId, string receiverId, string channelId, string content, string timestamp)
    {
        string id = PlayFabSettings.staticPlayer.PlayFabId;
        // 受け取った時の処理
        if (senderId == id || 
        (channelId == "DM" && receiverId == id) || 
        (receiverId == "All" && PlayFabData.CurrentRoomChannels[channelId].MemberIds.Contains(id)))
        {
            var messageData = new MessageData
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                ChannelId = channelId,
                Content = content,
                Timestamp = timestamp
            };
            Debug.Log("message受信" + receiverId);

            if(channelId == "DM")
            {
                string targetId = senderId == id ? receiverId : receiverId == id ? senderId : "";
                
                PlayFabData.DictDMScripts[targetId].messageDatas.Add(messageData);
                if(PlayFabData.CurrentMessageTarget == targetId)
                {
                    chatUIManager.DisplayMessage(messageData);
                }
            }
            else if(receiverId == "All")
            {
                PlayFabData.DictChannelScripts[channelId].messageDatas.Add(messageData);
                if(PlayFabData.CurrentChannelId == channelId)
                {
                    chatUIManager.DisplayMessage(messageData);
                }
            }
        }
        
        // ローカルデータベースに保存する
        //receivedMessageData = messageData;
        // Debug.Log(receivedMessageData.Content);
        // メッセージ表示の処理など
    }
}
