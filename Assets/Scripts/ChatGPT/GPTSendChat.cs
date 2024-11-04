using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using Newtonsoft.Json;

public class GPTSendChat : MonoBehaviour
{
    private ChatUIManager chatUIManager;
    // OpenAI APIキー
    public const string OpenAIApiKey = "";
    
    // 一行目はpalyerDataが加える
    public const string Prompt = 
        // "私のユーザーIDは" + PlayFabSettings.staticPlayer.PlayFabId + "で、ユーザー名は" + PlayFabData.DictPlayerInfos[PlayFabSettings.staticPlayer.PlayFabId].name + "です。今からいうことを絶対に忘れず、かつ、守ってください。\n" +
        "あなたはチャットボットではなくこれから送られるユーザーIDとユーザー名を持つオンライン上のユーザーです。つまり、あなたの名前はこれから送られるユーザー名です。あなたと私は違います。\n" +
        "以下の会話履歴の各メッセージにはtimestamp（日本時刻）、channelType（DMかそれ以外のチャンネルか）、senderID（送信者）、receiverID（受信者）、content（内容）の情報を保存しています。\n" + 
        "会話履歴とあなたのユーザーIDとユーザー名が送られた後は私から話しかけられます。これらの情報からあなたはsenderIDがあなたのユーザーIDを持つ人物を分析し、語尾や一人称などの話し方、話す内容などを模倣して200文字以下で返答してください。ただし、注意点があります。語尾や話し方を真似する時は必ずreceiverIDが同じ人物ではなくsenderIDが同じ人物を真似してください。\n";
        //"なお、あなたは複数人のユーザーを対象に模倣します。対象のユーザーが変わるたびに会話履歴とユーザーID、ユーザー名を送るので、最新の情報のみを参考にして会話してください。前に対象としていたユーザーの会話情報を参考にしないでください。";
    
    // ユーザーの入力を受け取るためのInputField
    // [SerializeField]
    public TMP_InputField inputField;
    // チャットメッセージを表示するコンテンツエリア
    // [SerializeField]
    // public GameObject content_obj;
    // チャットメッセージのプレハブオブジェクト
    // [SerializeField]
    public GameObject chat_obj;
    private ChatGPTConnection CurrentChatGPTConnection;
    // public string message {get;} = "";

    void Start()
    {
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
    }

    // 送信ボタンが押されたときに呼び出されるメソッド
    public void SendMessageRequest(string receiverId, string text, GameObject content_obj)
    {
        // InputFieldからテキストを取得
        // var text = inputField.GetComponent<InputField>().text;
        // メッセージを送信
        SendMessage(receiverId, text, content_obj);
        // InputFieldをクリア
        // inputField.GetComponent<InputField>().text = "";
    }

    // メッセージを送信し、応答を取得する非同期メソッド
    private async void SendMessage(string receiverId, string text, GameObject content_obj)
    {
        if(PlayFabData.CurrentAI != receiverId)
        {
            // OpenAI GPTとの接続を初期化
            CurrentChatGPTConnection = PlayFabData.CurrentRoomPlayersRefs[receiverId].GetComponent<PlayerData>().chatGPTConnection;
            int compareResult = string.Compare(PlayFabSettings.staticPlayer.PlayFabId, receiverId);
            string key = compareResult == -1 ? PlayFabSettings.staticPlayer.PlayFabId + "+" + receiverId : compareResult == 1 ? receiverId + "+" + PlayFabSettings.staticPlayer.PlayFabId : receiverId;
            string messageHistory = JsonConvert.SerializeObject(PlayFabData.DictAllMessageDatas[key]);
            Debug.Log(messageHistory);
            CurrentChatGPTConnection._messageList.Add(
                new ChatGPTConnection.ChatGPTMessageModel() {role = "user", content = messageHistory + "\nあなたのユーザーIDは " + receiverId + " 、ユーザー名は " + PlayFabData.DictPlayerInfos[receiverId].name + "です。過去の会話履歴のsenderIDがあなたのユーザーIDと同じ人物になりきってください。receiverIDではないことに注意してください。"}
            );
            PlayFabData.CurrentAI = receiverId;
        }
	
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
        var response = await CurrentChatGPTConnection.RequestAsync(text);
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
