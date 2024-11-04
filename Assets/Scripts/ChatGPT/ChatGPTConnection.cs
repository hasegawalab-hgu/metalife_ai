using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

// OpenAI GPTとの接続を管理するクラス
[Serializable]
public class ChatGPTConnection
{
    // OpenAI APIキー
    private readonly string _apiKey;
    // メッセージ履歴を保持するリスト
    public List<ChatGPTMessageModel> _messageList = new();

    // コンストラクタ：APIキーを設定し、初期メッセージを追加
    public ChatGPTConnection(string apikey, string prompt)
    {
        _apiKey = apikey;
        _messageList.Add(
            new ChatGPTMessageModel() {role = "user", content = prompt});
    }

    // メッセージモデル
    [Serializable]
    public class ChatGPTMessageModel
    {
        // メッセージの送信者の役割（ユーザーまたはシステム）
        public string role;
        // メッセージの内容
        public string content;
    }

    // ユーザーメッセージに対するOpenAI GPT APIリクエストを送信する非同期メソッド
    public async UniTask<ChatGPTResponseModel> RequestAsync(string userMessage)
    {
        // OpenAI GPT APIのエンドポイント
        var apiUrl = "https://api.openai.com/v1/chat/completions";

        // ユーザーメッセージをリストに追加
        _messageList.Add(new ChatGPTMessageModel {role = "user", content = userMessage});
        Debug.Log(_messageList.ToString());
        // OpenAI APIリクエストに必要なヘッダー情報
        var headers = new Dictionary<string, string>
        {
            {"Authorization", "Bearer " + _apiKey},
            {"Content-type", "application/json"}
        };

        // APIリクエストのオプション（モデルとメッセージリスト）
        var options = new ChatGPTCompletionRequestModel()
        {
            model = "gpt-3.5-turbo",
            messages = _messageList
        };
        var jsonOptions = JsonConvert.SerializeObject(options);
        Debug.Log(jsonOptions);

        // UnityWebRequestを使用してOpenAI GPT APIにリクエストを送信
        using var request = new UnityWebRequest(apiUrl, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonOptions)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        Debug.Log(request.downloadHandler.text);

        // リクエストヘッダーを設定
        foreach (var header in headers)
        {
            request.SetRequestHeader(header.Key, header.Value);
        }

        // リクエストを送信し、応答を待機
        await request.SendWebRequest();

        // リクエストの結果を処理
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
            throw new Exception();
        }
        else
        {
            var responseString = request.downloadHandler.text;
            var responseObject = JsonUtility.FromJson<ChatGPTResponseModel>(responseString);
            _messageList.Add(responseObject.choices[0].message);
            return responseObject;
        }
    }

    // OpenAI GPT APIリクエストのJSONモデル
    [Serializable]
    public class ChatGPTCompletionRequestModel
    {
        public string model; // 使用するモデル名
        public List<ChatGPTMessageModel> messages; // メッセージリスト
    }

    // OpenAI chat completions APIの応答モデル
    [System.Serializable]
    public class ChatGPTResponseModel
    {
        public string id; // 応答のID
        public string @object; // オブジェクトタイプ
        public int created; // 作成タイムスタンプ
        public Choice[] choices; // 応答選択肢
        public Usage usage; // 使用量情報

        [System.Serializable]
        public class Choice
        {
            public int index; // 選択肢のインデックス
            public ChatGPTMessageModel message; // メッセージ内容
            public string finish_reason; // 終了理由
        }

        [System.Serializable]
        public class Usage
        {
            public int prompt_tokens; // プロンプトに使用されたトークン数
            public int completion_tokens; // 完成に使用されたトークン数
            public int total_tokens; // 合計使用トークン数
        }
    }
}
