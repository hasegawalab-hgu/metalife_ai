using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;
    [SerializeField]
    private TMP_Text outputField;
    private ClientChatListener chatListener;


    private void Start()
    {
        // ClientChatListenerスクリプトのインスタンスを取得
        chatListener = GetComponent<ClientChatListener>();
    }

    public void SendMessageToPublicCannel()
    {
        if (chatListener != null && chatListener.chatClient != null)
        {
            string message = inputField.text;
            chatListener.chatClient.PublishMessage(chatListener.defaultChannel, message);
            inputField.text = ""; // 入力フィールドをクリア
        }
    }
}
