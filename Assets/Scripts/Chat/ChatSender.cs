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
        Debug.Log(HasInputAuthority);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SendMessageRequest(string senderId, string receiverId, string channelId, string content, string timestamp)
    {
        if(HasStateAuthority)
        {
            RPC_SendMessage(PlayFabSettings.staticPlayer.PlayFabId, senderId, receiverId, channelId, content, timestamp);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SendMessage(string id, string senderId, string receiverId, string channelId, string content, string timestamp)
    {
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
            Debug.Log("message受信" + id);

            if(channelId == "DM")
            {
                string targetId = senderId == id ? receiverId : receiverId == id ? senderId : "";
                
                if(senderId == id)
                {
                    chatUIManager.UpdateChannelMessageData(id, messageData);
                }

                if(PlayFabData.CurrentMessageTarget == targetId)
                {
                    chatUIManager.DisplayMessage(messageData);
                }
            }
            else if(receiverId == "All")
            {
                if(senderId == id)
                {
                    chatUIManager.UpdateChannelMessageData(id, messageData);
                }

                if(PlayFabData.CurrentChannelId == channelId)
                {
                    chatUIManager.DisplayMessage(messageData);
                }

                // apiを読んでから追加
                // PlayFabData.DictChannelScripts[channelId].messageDatas.Add(messageData);
            }
        }
        
        // ローカルデータベースに保存する
        //receivedMessageData = messageData;
        // Debug.Log(receivedMessageData.Content);
        // メッセージ表示の処理など
    }
}
