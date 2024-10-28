using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;


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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
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
        // 受け取った時の処理
        if (senderId == PlayFabSettings.staticPlayer.PlayFabId || 
        (channelId == "DM" && receiverId == PlayFabSettings.staticPlayer.PlayFabId) || 
        (receiverId == "All" && PlayFabData.CurrentRoomChannels[channelId].MemberIds.Contains(PlayFabSettings.staticPlayer.PlayFabId)))
        {
            string key = ""; // 共有グループデータのキーとして利用

            var messageData = new MessageData
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                ChannelId = channelId,
                Content = content,
                Timestamp = timestamp
            };
            Debug.Log("message受信, sender: " + senderId);

            // 簡易チャットの表示
            chatUIManager.DisplaySimpleMessage(messageData);

            if(channelId == "DM") // DM
            {
                string targetId = senderId == PlayFabSettings.staticPlayer.PlayFabId ? receiverId : receiverId == PlayFabSettings.staticPlayer.PlayFabId ? senderId : "";
                PlayFabData.DictDMScripts[targetId].messageDatas.Add(messageData); // ローカルのメッセージデータを保存


                // playfab共有データのキーを決定、key: id+id (Compare()で比較して若い方が先)    
                int result = string.Compare(messageData.SenderId, messageData.ReceiverId);
                key = result == -1 ? messageData.SenderId + "+" + messageData.ReceiverId : result == 1 ? messageData.ReceiverId + "+" + messageData.SenderId : messageData.SenderId;

                if((senderId == PlayFabSettings.staticPlayer.PlayFabId & PlayFabData.CurrentMessageTarget == targetId) || // 送信者が送信先のDMを開いている時
                    (senderId != PlayFabSettings.staticPlayer.PlayFabId & PlayFabData.CurrentMessageTarget == senderId)   // 受信者が受信先のDMを開いている時
                )
                {
                    chatUIManager.DisplayMessage(messageData);
                    if(!string.IsNullOrEmpty(key))
                    {
                        chatUIManager.DictReadMessageCount[key] = PlayFabData.DictDMScripts[targetId].messageDatas.Count; // 既読数を更新
                    }
                }
                else
                {
                    if(senderId != PlayFabSettings.staticPlayer.PlayFabId)
                    {
                        PlayFabData.DictDMScripts[senderId].UnReadMessageCount++;
                    }
                }
            }
            else if(receiverId == "All") // Channel
            {
                key = channelId;
                PlayFabData.DictChannelScripts[channelId].messageDatas.Add(messageData); // ローカルのメッセージデータを保存

                if(PlayFabData.CurrentChannelId == channelId)
                {
                    chatUIManager.DisplayMessage(messageData);
                    chatUIManager.DictReadMessageCount[channelId] = PlayFabData.DictChannelScripts[messageData.ChannelId].messageDatas.Count; // 既読数を更新
                }
                else
                {
                    PlayFabData.DictChannelScripts[channelId].UnReadMessageCount++;
                }
            }

            // 送信者であれば既読数とデータベースを更新
            if(!string.IsNullOrEmpty(key) & senderId == PlayFabSettings.staticPlayer.PlayFabId)
            {
                if(receiverId == "All")
                {
                    chatUIManager.DictReadMessageCount[key] = PlayFabData.DictChannelScripts[key].messageDatas.Count; // 既読数を更新
                }
                else
                {
                    chatUIManager.DictReadMessageCount[key] = PlayFabData.DictDMScripts[receiverId].messageDatas.Count; // 既読数を更新
                }
                chatUIManager.UpdateUserData(); // 既読数のデータベースを更新
                chatUIManager.UpdateChannelMessageData(key, messageData); // メッセージデータのデータベースを更新
            }
        }
        
        // ローカルデータベースに保存する
        //receivedMessageData = messageData;
        // Debug.Log(receivedMessageData.Content);
        // メッセージ表示の処理など
    }
}