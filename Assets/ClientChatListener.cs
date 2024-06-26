using System.Collections;
using System.Collections.Generic;
using Photon.Chat;
using TMPro;
using UnityEngine;

public class ClientChatListener : MonoBehaviour, IChatClientListener
{
    public ChatClient chatClient;
    [SerializeField]
    private const string chatAppId = "a5beccbf-8495-4eb9-9e29-a9104f0b5a01";
    public string defaultChannel = "general";

    [SerializeField]
    private TMP_Text outputField;

    private void Start()
    {
        // ChatClientの初期化
        chatClient = new ChatClient(this);
        chatClient.Connect(chatAppId, "2.17.0", new AuthenticationValues("test"));
    }

    void Update()
    {
        if (chatClient != null)
        {
            chatClient.Service(); // サービスを呼び出してイベントを処理
        }
    }

    #region IChatClientListener implementation

    public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message)
    {
        Debug.Log($"DebugReturn: {message}");
    }

    public void OnDisconnected()
    {
        Debug.Log("Disconnected from chat.");
    }

    public void OnConnected()
    {
        Debug.Log("Connected to chat.");

        // デフォルトのチャットチャンネルに参加
        chatClient.Subscribe(new string[] { defaultChannel });
    }

    public void OnChatStateChange(ChatState state)
    {
        Debug.Log($"Chat state changed: {state}");
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        outputField.SetText(messages[0].ToString());
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        Debug.Log($"Private message from {sender} in {channelName}: {message}");
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log($"Subscribed to channels: {string.Join(", ", channels)}");
    }

    public void OnUnsubscribed(string[] channels)
    {
        Debug.Log($"Unsubscribed from channels: {string.Join(", ", channels)}");
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        Debug.Log($"Status update from {user}: {status}");
    }

    public void OnUserSubscribed(string channel, string user)
    {
        Debug.Log($"{user} subscribed to {channel}");
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        Debug.Log($"{user} unsubscribed from {channel}");
    }

    #endregion
}
