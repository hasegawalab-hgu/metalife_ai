using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SendChat : MonoBehaviour
{
    // OpenAI APIキー
    private readonly string openAIApiKey = "sk-QvR97rnhAZ2aexCzrFoiA57Gg9bU_bj4a7Bc0zktKTT3BlbkFJYGUQtQqfUyQdG1N1l2lcea1j3IEJ5VYJFzZpkPSPkA";
    // ユーザーの入力を受け取るためのInputField
    [SerializeField]
    private InputField inputField;
    // チャットメッセージを表示するコンテンツエリア
    [SerializeField]
    private GameObject content_obj;
    // チャットメッセージのプレハブオブジェクト
    [SerializeField]
    private GameObject chat_obj;

    // 送信ボタンが押されたときに呼び出されるメソッド
    public void OnClick()
    {
        // InputFieldからテキストを取得
        var text = inputField.GetComponent<InputField>().text;
        // メッセージを送信
        sendmessage(text);
        // InputFieldをクリア
        inputField.GetComponent<InputField>().text = "";
    }

    // メッセージを送信し、応答を取得する非同期メソッド
    private async void sendmessage(string text)
    {
        // OpenAI GPTとの接続を初期化
        var chatGPTConnection = new ChatGPTConnection(openAIApiKey);
	
        // ユーザーのメッセージを表示するオブジェクトを生成
        var sendObj = Instantiate(chat_obj, this.transform.position, Quaternion.identity);
        sendObj.GetComponent<Image>().color = new Color(0.6f, 1.0f, 0.1f, 0.3f);
        GameObject Child = sendObj.transform.GetChild(1).gameObject;
        Debug.Log(Child);
        Child.GetComponent<Text>().text = text;
        // 生成したオブジェクトをコンテンツエリアの子要素として追加
        sendObj.transform.SetParent(content_obj.transform, false);

        // OpenAI GPTにリクエストを送信し、応答を待つ
        var response = await chatGPTConnection.RequestAsync(text);
        // 応答があれば処理を行う
        if (response.choices != null && response.choices.Length > 0)
        {
            var choice = response.choices[0];
            Debug.Log("ChatGPT Response: " + choice.message.content);
            // GPTの応答を表示するオブジェクトを生成
            var responseObj = Instantiate(chat_obj, this.transform.position, Quaternion.identity);
            responseObj.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.3f);
            GameObject Child_responce = responseObj.transform.GetChild(1).gameObject;
            Child_responce.GetComponent<Text>().text = choice.message.content;
            // 応答オブジェクトをコンテンツエリアの子要素として追加
            responseObj.transform.SetParent(content_obj.transform, false);
        }
    }
}
