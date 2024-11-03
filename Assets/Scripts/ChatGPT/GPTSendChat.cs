using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GPTSendChat : MonoBehaviour
{
    private ChatUIManager chatUIManager;
    // OpenAI APIキー
    private readonly string openAIApiKey = "";
    // ユーザーの入力を受け取るためのInputField
    // [SerializeField]
    public TMP_InputField inputField;
    // チャットメッセージを表示するコンテンツエリア
    // [SerializeField]
    // public GameObject content_obj;
    // チャットメッセージのプレハブオブジェクト
    // [SerializeField]
    public GameObject chat_obj;
    // public string message {get;} = "";

    void Start()
    {
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
    }

    // 送信ボタンが押されたときに呼び出されるメソッド
    public void SendMessageRequest(string text, GameObject content_obj)
    {
        // InputFieldからテキストを取得
        // var text = inputField.GetComponent<InputField>().text;
        // メッセージを送信
        SendMessage(text, content_obj);
        // InputFieldをクリア
        // inputField.GetComponent<InputField>().text = "";
    }

    // メッセージを送信し、応答を取得する非同期メソッド
    private async void SendMessage(string text, GameObject content_obj)
    {
        // OpenAI GPTとの接続を初期化
        var chatGPTConnection = new ChatGPTConnection(openAIApiKey);
	
        // ユーザーのメッセージを表示するオブジェクトを生成
        var waitObj = Instantiate(chat_obj, new Vector3(0f, 0f, 0f), Quaternion.identity);
        waitObj.GetComponent<TMP_Text>().text = "入力中...";
        waitObj.GetComponent<TMP_Text>().color = Color.white;
        // //sendObj.GetComponent<Image>().color = new Color(0.6f, 1.0f, 0.1f, 0.3f);
        // // GameObject Child = sendObj.transform.GetChild(1).gameObject;
        // // Debug.Log(Child);
        // // Child.GetComponent<Text>().text = text;
        // // 生成したオブジェクトをコンテンツエリアの子要素として追加
        waitObj.transform.SetParent(content_obj.transform, false);

        // OpenAI GPTにリクエストを送信し、応答を待つ
        var response = await chatGPTConnection.RequestAsync(text);
        GameObject responseObj = null;
        // 応答があれば処理を行う
        if (response.choices != null && response.choices.Length > 0)
        {
            Destroy(waitObj);
            var choice = response.choices[0];
            Debug.Log("ChatGPT Response: " + choice.message.content);
            // GPTの応答を表示するオブジェクトを生成
            responseObj = Instantiate(chat_obj, new Vector3(0f, 0f, 0f), Quaternion.identity);
            // responseObj.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.3f);
            //GameObject Child_responce = responseObj.transform.GetChild(1).gameObject;
            responseObj.GetComponent<TMP_Text>().text = choice.message.content;
            // 応答オブジェクトをコンテンツエリアの子要素として追加
            responseObj.transform.SetParent(content_obj.transform, false);
        }
        else
        {
            Destroy(waitObj);
            responseObj.GetComponent<TMP_Text>().text = "";
        }

        if(responseObj != null)
        {
            chatUIManager.StartCoroutine(chatUIManager.DeleteSimpleMessage(7.0f, responseObj.gameObject));
        }
    }
}
