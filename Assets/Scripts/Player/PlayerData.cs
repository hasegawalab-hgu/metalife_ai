using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using Newtonsoft.Json;
using System.Linq;
using System;

public class Distance : IComparable<Distance>
{
    public string Id;
    public Vector3 Dist;

    public Distance(string id, Vector3 dist)
    {
        Id = id;
        Dist = dist;
    }

    // CompareToメソッドを実装する
    public int CompareTo(Distance other)
    {
        // Distの大きさで比較する（例: 距離の長さを基準にする場合）
        return Dist.magnitude.CompareTo(other.Dist.magnitude);
    }
}

public class PlayerData : NetworkBehaviour
{
    [Networked]
    public string PlayFabId {get; set;}

    public GetPlayerCombinedInfoRequestParams PlayerInfoParams;

    public TMP_Text TextDisplayName;
    [Networked]
    public string DisplayName { get; set;}

    [Networked]
    public string GraduationYear { get; set;}

    [Networked]
    public bool KeepLoginInfo { get; set;}
    [Networked]
    public string texturePath {get; set;} = ""; // 16文字しか入らないので注意
    [Networked]
    public string texturePath2 {get; set;} = ""; // texturePathの続き
    [Networked]
    public bool IsInputting { get; set;} = false;
    [Networked]
    public bool IsHost { get; set;} = false;

    private bool isInputting = false;
    public void SetIsInputting(bool isInputting)
    {
        IsInputting = isInputting;
    }
    public bool GetInputting()
    {
        return IsInputting;
    }

    public void SetIsChattingDelay(bool isChatting, float delay)
    {
        Invoke("SetIsChatting", delay);
    }

    private void SetIsChatting(bool isChatting)
    {
        IsChatting = isChatting;
    }

    private TMP_Text inputtingText;

    [Networked]
    public bool IsOnline { get; set;}

    public List<Distance> Targets = new List<Distance>();
    private ChatManager chatManager;
    private ChatUIManager chatUIManager;
    private NetworkRunner runner;

    private LocalGameManager lgm;
    private PlayFabLogout logout;

    public GameObject simpleChatView;
    private RectTransform rt; // simpleChatViewのrecttransform

    private GameObject localPlayer;

    private Camera cam;

    public List<Sprite> sprites  {get; set;} = new List<Sprite>();
    public PlayerSpawner ps;
    private GameObject playerContainer;
    private ChatSender cs;
    const float TALKDIST = 3.0f;
    public Material MyMat;
    GameObject MainLoby;
    GPTSendChat gsc;

    [SerializeField]
    public ChatGPTConnection chatGPTConnection;
    private PlayerMovement pm;

    public bool IsChatting = false;
    public bool IsAI = false;

    private float deltaTime;
    private float interval = 20.0f;
    public string CurrentContent = "";

    private void Awake()
    {
        playerContainer = GameObject.Find("Players");
        transform.SetParent(playerContainer.transform);
    }

    private void Start()
    {   
        if(this.PlayFabId != PlayFabSettings.staticPlayer.PlayFabId)
        {
            this.gameObject.name = this.PlayFabId;
        }
        playerContainer = GameObject.Find("Players");
        chatManager = GameObject.Find("ChatManager").GetComponent<ChatManager>();
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
        lgm = GameObject.Find("LocalGameManager").GetComponent<LocalGameManager>();
        logout = GameObject.Find("LocalGameManager").GetComponent<PlayFabLogout>();
        cs = GetComponent<ChatSender>();
        MainLoby = GameObject.Find("ConnectionManager").transform.GetChild(0).gameObject;
        transform.SetParent(playerContainer.transform);
        pm = GetComponent<PlayerMovement>();
        gsc = GameObject.Find("ChatGPT").GetComponent<GPTSendChat>();
        // Invoke("a", 0.1f);

        if(Object.HasInputAuthority)
        {
            // Invoke("GetPlayerCombinedInfo", 1f); // すぐに実行すると反映されていないため1秒後に実行
            IsOnline = true;
            
            DisplayName = PlayFabData.MyName;
            GraduationYear = PlayFabData.MyGraduationYear;
            if(PlayFabData.MyTexturePath.Length < 17)
            {
                texturePath = PlayFabData.MyTexturePath;
                texturePath2 = "";
            }
            else
            {
                texturePath = PlayFabData.MyTexturePath.Substring(0, 16);
                texturePath2 = PlayFabData.MyTexturePath.Substring(16);
            }

            // 自分のテキストUIを設定
            TextDisplayName.SetText(DisplayName);
            // SetUserData();
            chatManager.chatSender = GetComponent<ChatSender>();

            // stateInfo
            PlayFabData.DictPlayerStateInfos[this.PlayFabId] = new PlayerStateInfo(this.PlayFabId, this.DisplayName, this.transform.position, pm.CurrentInputType, this.IsAI, this.IsChatting, "", this.isInputting);

            LoadTexture();
            CheckDoubleLogin();
            // Invoke("CheckDoubleLogin", 0.5f);
        }
        else
        {
            PlayFabData.CurrentRoomPlayersRefs[this.PlayFabId] = this;
            if(PlayFabData.DictPlayerInfos.ContainsKey(this.PlayFabId))
            {
                DisplayName = PlayFabData.DictPlayerInfos[this.PlayFabId].name;
                if(PlayFabData.DictPlayerInfos[this.PlayFabId].texturePath.Length < 17)
                {
                    texturePath = PlayFabData.DictPlayerInfos[this.PlayFabId].texturePath;
                    texturePath2 = "";
                }
                else
                {
                    texturePath = PlayFabData.DictPlayerInfos[this.PlayFabId].texturePath.Substring(0, 16);
                    texturePath2 = PlayFabData.DictPlayerInfos[this.PlayFabId].texturePath.Substring(16);
                }
            }
            if (GetComponent<NetworkObject>().InputAuthority == default(PlayerRef))
            {
                IsAI = true;
            }
            string prompt = 
                "私のユーザーIDは" + PlayFabSettings.staticPlayer.PlayFabId + "で、ユーザー名は" + PlayFabData.DictPlayerInfos[PlayFabSettings.staticPlayer.PlayFabId].name + "です。つまり、私の名前は" + PlayFabData.DictPlayerInfos[PlayFabSettings.staticPlayer.PlayFabId].name + "です。" + 
                "今からいうことを絶対に忘れず、かつ、守ってください。\n" + GPTSendChat.Prompt;
            chatGPTConnection = new ChatGPTConnection(GPTSendChat.OpenAIApiKey, prompt);
            Invoke("RemotePlayerTexture2Sprite", 1f);
            // 他ユーザーのテキストUIを設定
            Invoke("SetTextDisplayName", 2f); // すぐに実行すると反映されていないため1秒後に実行
        }
        localPlayer = GameObject.Find("LocalPlayer");
        simpleChatView = Instantiate(chatUIManager.spawner_simple_message, new Vector3(0f,0f,0f), Quaternion.identity);
        simpleChatView.transform.SetParent(chatUIManager.Canvas.transform);
        PlayFabData.DictSpawnerSimpleMessage[this.PlayFabId] = simpleChatView.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        rt = simpleChatView.GetComponent<RectTransform>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();

        ps = GameObject.Find("ConnectionManager").transform.GetChild(0).gameObject.GetComponent<PlayerSpawner>();

        MyMat = GetComponent<Renderer>().material;
        // a();
        //Debug.Log(PlayFabData.DictDMScripts[this.PlayFabId]);
        Invoke("AddDictDMScripts", 1.5f);
        Invoke("ReDisplayDM", 2f);
        Invoke("ClickDM", 1f);
    }

    private void a()
    {
        var obj = Instantiate(chatUIManager.text_simple_messagePref, new Vector3(0f, 0f, 0f), Quaternion.identity);
        obj.transform.SetParent(simpleChatView.transform.GetChild(0).transform.GetChild(0).transform);
        obj.GetComponent<TMP_Text>().text = PlayFabData.CurrentRoomPlayersRefs.Count().ToString();
    }

    
    private void ReDisplayDM()
    {
        if(!PlayFabData.DictPlayerInfos.ContainsKey(this.PlayFabId))
        {
            Debug.Log("Add CurrentRoomPlayers: " + this.DisplayName);
            PlayFabData.DictPlayerInfos.Add(this.PlayFabId, new PlayerInfo {id = this.PlayFabId, name = this.DisplayName, texturePath = this.texturePath + this.texturePath2});
            if(PlayFabData.NewCreated)
            {
                UpdatePlayerInfos();
            }
            chatUIManager.DisplayDMTargets();
        }
    }

    public void Update()
    {
        if (GetComponent<NetworkObject>().InputAuthority == default(PlayerRef))
        {
            IsAI = true;
        }
        else
        {
            IsAI = false;
        }

        if(rt != null)
        {
            rt.position = cam.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y, 0f));
        }

        if(IsInputting != isInputting)
        {
            if(isInputting)
            {
                if(inputtingText != null)
                {
                    Destroy(inputtingText.transform.gameObject);
                    inputtingText = null;
                }
            }
            else
            {
                if(HasInputAuthority)
                {
                    if(inputtingText == null)
                    {
                        inputtingText = Instantiate(chatUIManager.text_simple_messagePref, new Vector3(0f, 0f, 0f), Quaternion.identity);
                        inputtingText.transform.SetParent(PlayFabData.DictSpawnerSimpleMessage[this.PlayFabId].transform);
                        inputtingText.GetComponent<TMP_Text>().text = "入力中...";
                        inputtingText.color = new Color32(255, 255, 255, 180);
                    }
                }
            }

            isInputting = IsInputting;
        }

        if(this.PlayFabId != PlayFabSettings.staticPlayer.PlayFabId)
        {
            if((transform.position.x - 0.5) * 2 % 2 == 0 && (transform.position.y - 0.5) * 2 % 2 == 0)
            {
                if(localPlayer == null)
                {
                    return;
                }
                Vector3 dist = new Vector3(Mathf.Abs(transform.position.x - localPlayer.transform.position.x), Mathf.Abs(transform.position.y - localPlayer.transform.position.y), 0f);
                
                if(!PlayFabData.DictDistance.ContainsKey(this.PlayFabId))
                {
                    PlayFabData.DictDistance[this.PlayFabId] = dist;
                    return;
                }

                if(PlayFabData.DictDistance[this.PlayFabId] != dist)
                {
                    PlayFabData.DictDistance[this.PlayFabId] = dist;
                }
            }
            return;
        }

        /// 以降ローカルプレイヤー

        if(chatUIManager.inputField.isFocused && !string.IsNullOrEmpty(chatUIManager.inputField.text))
        {
            IsInputting = true;
        }
        else
        {
            IsInputting = false;
        }

        if(Targets.Count == 0 && PlayFabData.CurrentMessageTarget != this.PlayFabId)
        {
            PlayFabData.CurrentMessageTarget = this.PlayFabId;
            ClickDM();
        }

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(Targets.Count > 1)
            {
                var firstElement = Targets[0];
                for (int i = 0; i < Targets.Count - 1; i++)
                {
                    Targets[i] = Targets[i + 1];
                }
                Targets[Targets.Count - 1] = firstElement;
                string test = "";
                for(int i=0; i< Targets.Count; i++)
                {
                    test += " " + PlayFabData.DictPlayerInfos[Targets[i].Id].name;
                }
                Debug.Log(test);
                if(PlayFabData.CurrentMessageTarget != Targets[0].Id && lgm.LocalGameState == LocalGameManager.GameState.Playing)
                {
                    // Debug.Log(Targets.Count + "  " + PlayFabData.DictPlayerInfos[Targets[0].Id].name);
                    ClickDM();
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            if(Targets.Count > 1)
            {
                var lastElement = Targets[Targets.Count - 1];
                Targets.RemoveAt(Targets.Count - 1);
                for (int i = Targets.Count - 1; i > 0; i--)
                {
                    Targets[i] = Targets[i - 1];
                }
                Targets[0] = lastElement;
                string test = "";
                for(int i=0; i< Targets.Count; i++)
                {
                    test += " " + PlayFabData.DictPlayerInfos[Targets[i].Id].name;
                }
                Debug.Log(test);
                if(PlayFabData.CurrentMessageTarget != Targets[0].Id && lgm.LocalGameState == LocalGameManager.GameState.Playing)
                {
                    // Debug.Log(Targets.Count + "  " + PlayFabData.DictPlayerInfos[Targets[0].Id].name);
                    ClickDM();
                }
            }
        }

        foreach(var pr in PlayFabData.DictDistance)
        {
            // Debug.Log(pr.Key + " " + pr.Value);
            if(pr.Key != PlayFabSettings.staticPlayer.PlayFabId)
            {
                if(Math.Abs(pr.Value.x) <= TALKDIST && Math.Abs(pr.Value.y) <= TALKDIST)
                {
                    if(ContainsId(pr.Key) == null && lgm.LocalGameState == LocalGameManager.GameState.Playing)
                    {
                        int before = Targets.Count;
                        Targets.Add(new Distance(pr.Key, pr.Value));
                        ClickDM();
                        // if(PlayFabData.DictPlayerInfos.ContainsKey(Targets[0].Id) && PlayFabData.CurrentMessageTarget != Targets[0].Id)
                        // {
                        //     ClickDM();
                        // }
                    }
                }
                else
                {
                    Distance dist = ContainsId(pr.Key);
                    if(dist != null)
                    {
                        Targets.Remove(dist);
                    }
                }
            }

            if(Targets.Count != 0)
            {
                if(PlayFabData.DictPlayerInfos.ContainsKey(Targets[0].Id) && PlayFabData.CurrentMessageTarget != Targets[0].Id && lgm.LocalGameState == LocalGameManager.GameState.Playing)
                {
                    ClickDM();
                }
                chatUIManager.inputField.gameObject.SetActive(true);
            }
            else
            {
                if(PlayFabData.CurrentMessageTarget != this.PlayFabId)
                {
                    ClickDM();
                }
                chatUIManager.inputField.gameObject.SetActive(false);
            }
        }

        if(PlayFabData.CurrentMessageTarget != this.PlayFabId & Targets.Count == 0 && lgm.LocalGameState == LocalGameManager.GameState.Playing)
        {
            if(Targets.Count != 0)
            {
                if(PlayFabData.DictPlayerInfos.ContainsKey(Targets[0].Id) && PlayFabData.CurrentMessageTarget != Targets[0].Id)
                {
                    ClickDM();
                }
            }
        }

        /*
        if(isChangeDMTaget & lgm.LocalGameState == LocalGameManager.GameState.Playing)
        {
            Debug.Log("aaa");
            ClickDM();
            isChangeDMTaget = false;
        }
        */
    }

    private void FixedUpdate()
    {
        deltaTime += Runner.DeltaTime;
        if(deltaTime >= interval)
        {
            RPC_AddStateInfoRequest();
            /*
            List<PlayerStateInfo> stateInfos = new List<PlayerStateInfo>();
            foreach (var player in PlayFabData.DictPlayerInfos)
            {
                stateInfos.Add(PlayFabData.DictPlayerInfos[player.Key]);
            }
            Debug.Log(stateInfos);
            */

            deltaTime = 0f;
        }
    }
    [Rpc(RpcSources.All,RpcTargets.StateAuthority)]
    private async void RPC_AddStateInfoRequest()
    {
        if (HasStateAuthority && IsHost && !string.IsNullOrEmpty(this.DisplayName))
        {
            foreach (var player in PlayFabData.CurrentRoomPlayersRefs)
            {
                player.Value.RPC_AddStateInfo();
            }

            // gpt送信
            string prompt = 
                "あなたは2D世界の住人です。上下左右の向きと現在の座標を持ち、移動する時は必ずその方向に方向転換した後、1マスずつ動きます。" + 
                "あなた以外にもプレイヤーがおり、それぞれコミュニケーションをとっています。ただし、各プレイヤーは3マス以内に近づかないと会話することができません。また、他のプレイヤーの中にはAIも混じっています。" +
                "あなたにはこれからあなた自身のidと他のプレイヤーを含めた行動の履歴が送られます。" + 
                "具体的には各プレイヤーごとに id:プレイヤーを識別するid、name: プレイヤーの名前、 pos: プレイヤーのボジション、 dir: プレイヤーが向いている方向（下:0、 左:1、 右:2、 上:3）、 IsAI: プレイヤーがAIかどうか、 IsChatting: 発言中かどうか、 content: 発言内容、 IsInputting: これから発言する内容を入力中かどうか  という情報が与えられます。" +
                "あなたはこれらの情報をもとに全てのAIプレイヤーの次の行動を生成してもらいます。ただし、複数のAIプレイヤーがいても一つの回答でまとめて送信してください。全てのAIプレイヤーの行動生成結果をまとめてから送信してください。まだ全員の結果を生成していないのにレスポンスを送らないでください。" +
                "プレイヤーhogehoge1234が生成した向きが0、プレイヤーtesttest1234が生成した向きが1の場合は形式は以下の通りです。これ以外に何も答えないでください。また、適切な回答を生成できないと判断した場合は空文字を英数字一字\"\"で返してください。" +
                "{" + 
                "id: \"hogehoge1234\" {dir: \"0\"}," +
                "id: \"testtest1234\" {dir: \"1\"}" + 
                "}";
            ChatGPTConnection chatGPTConnection = new ChatGPTConnection(apikey: GPTSendChat.OpenAIApiKey, prompt: prompt);
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            string data = JsonConvert.SerializeObject(PlayFabData.DictPlayerStateInfos, settings);
            var response = await chatGPTConnection.RequestAsync(data);
            // 応答があれば処理を行う
            if (response.choices != null && response.choices.Length > 0)
            {
                var choice = response.choices[0];
                Debug.Log("ChatGPT Response: " + choice.message.content);
            }
            else
            {
                Debug.Log("失敗");
            }
        }
    }

    // [Rpc(RpcSources.StateAuthority,RpcTargets.All)]
    private void RPC_AddStateInfo()
    {
        if(!string.IsNullOrEmpty(this.DisplayName))
        {
            string content = "";
            if (IsChatting)
            {
                GameObject contentObj = null;
                if(!IsInputting)
                {
                    if(simpleChatView.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.transform.childCount > 0)
                    {
                        contentObj = simpleChatView.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
                        content = contentObj.transform.GetChild(contentObj.transform.childCount - 1).gameObject.GetComponent<TMP_Text>().text;
                        Debug.Log("rpccccccccc " + this.DisplayName + " " + content);
                    }
                }
            }
            PlayFabData.DictPlayerStateInfos[this.PlayFabId] = new PlayerStateInfo (this.PlayFabId, this.DisplayName, this.transform.position, pm.CurrentInputType, IsAI, IsChatting, content, this.isInputting);
        }
    }

    public void ClickDM()
    {
        if(Targets.Count > 0)
        {
            foreach(var player in PlayFabData.CurrentRoomPlayersRefs)
            {
                if(ContainsId(player.Key) != null)
                {
                    player.Value.MyMat.SetFloat("_OutlineThickness", 0.005f);
                    if(Targets[0].Id == player.Key)
                    {
                        player.Value.MyMat.SetColor("_OutlineColor", Color.red);
                    }
                    else
                    {
                        player.Value.MyMat.SetColor("_OutlineColor", Color.green);
                    }
                }
                else
                {
                    player.Value.MyMat.SetFloat("_OutlineThickness", 0f);
                    player.Value.MyMat.SetColor("_OutlineColor", Color.green);
                }
            }
            if(PlayFabData.CurrentRoomPlayersRefs.ContainsKey(Targets[0].Id))
            {
                PlayFabData.CurrentRoomPlayersRefs[Targets[0].Id].MyMat.SetColor("_OutlineColor", Color.red);
            }
            // string t = "";
            
            if(PlayFabData.DictDMScripts.ContainsKey(Targets[0].Id))
            {
                Debug.Log(Targets[0].Id);
                PlayFabData.DictDMScripts[Targets[0].Id].OnClickButton();
                chatUIManager.text_targets.text = "To : " + string.Join(", ", PlayFabData.DictPlayerInfos[Targets[0].Id].name);
            }
        }
        else
        {
            foreach(PlayerData player in PlayFabData.CurrentRoomPlayersRefs.Values)
            {
                player.MyMat.SetFloat("_OutlineThickness", 0f);
                player.MyMat.SetColor("_OutlineColor", Color.green);
            }
            if(PlayFabData.DictDMScripts.ContainsKey(this.PlayFabId))
            {
                PlayFabData.DictDMScripts[this.PlayFabId].OnClickButton();
            }
            chatUIManager.text_targets.text = "";
        }
    }

    public void LoadTexture()
    {
        if(!string.IsNullOrEmpty(PlayFabData.MyTexturePath))
        {
            if(PlayFabData.MyTexturePath.Length < 17)
            {
                this.texturePath = PlayFabData.MyTexturePath;
                this.texturePath2 = "";
            }
            else
            {
                this.texturePath = PlayFabData.MyTexturePath.Substring(0, 16);
                this.texturePath2 = PlayFabData.MyTexturePath.Substring(16);
            }

            PlayFabData.MyTexture = Resources.Load(PlayFabData.MyTexturePath, typeof(Texture2D)) as Texture2D;
            Texture2Sprite(PlayFabData.MyTexture);
        }
        else
        {
            List<string> paths = new List<string>()
            {
                "Modern/制服1冬服_女_01", "Modern/制服1冬服_女_04", "Modern/制服1冬服_女_12", "Modern/制服1冬服_女_13", "Modern/制服1冬服_女_17", // 女子学生
                "Modern/制服1冬服_男_01", "Modern/制服1冬服_男_04", "Modern/制服1冬服_男_06", "Modern/制服1冬服_男_10", "Modern/制服1冬服_男_12"  // 男子学生
            };
            int rand = UnityEngine.Random.Range(0, paths.Count);
            if(string.IsNullOrEmpty(texturePath))
            {
                if(paths[rand].Length < 17)
                {
                    texturePath = paths[rand];
                    texturePath2 = "";
                }
                else
                {
                    texturePath = paths[rand].Substring(0, 16);
                    texturePath2 = paths[rand].Substring(16);
                }
            }
            PlayFabData.MyTexture = Resources.Load(paths[rand], typeof(Texture2D)) as Texture2D;
            Texture2Sprite(PlayFabData.MyTexture);
        }
    }

    // Invoke()で呼ぶためのメソッド
    private void RemotePlayerTexture2Sprite()
    {
        if(!string.IsNullOrEmpty(texturePath))
        {
            Texture2D texture = Resources.Load(texturePath + texturePath2, typeof(Texture2D)) as Texture2D;
            if(texture == null)
            {
                Debug.Log("texture null");
            }
            else
            {
                Texture2Sprite(texture);
            }
        }
        else
        {
            List<string> paths = new List<string>()
            {
                "Modern/制服1冬服_女_01", "Modern/制服1冬服_女_04", "Modern/制服1冬服_女_12", "Modern/制服1冬服_女_13", "Modern/制服1冬服_女_17", // 女子学生
                "Modern/制服1冬服_男_01", "Modern/制服1冬服_男_04", "Modern/制服1冬服_男_06", "Modern/制服1冬服_男_10", "Modern/制服1冬服_男_12"  // 男子学生
            };
            int rand = UnityEngine.Random.Range(0, paths.Count);
            if(paths[rand].Length < 17)
            {
                texturePath = paths[rand];
                texturePath2 = "";
            }
            else
            {
                texturePath = paths[rand].Substring(0, 16);
                texturePath2 = paths[rand].Substring(16);
            }
            Debug.Log("texturePath, null: " + texturePath);
            Invoke("RemotePlayerTexture2Sprite", 1f);
        }
    }

    private void Texture2Sprite(Texture2D texture)
    {
        if(texture != null)
        {
            sprites.Clear();
            sprites.Add(Sprite.Create(texture, new Rect(0, 96, 32, 32), new Vector2(0.5f, 0.5f), 32f)); // 正面歩き1
            sprites.Add(Sprite.Create(texture, new Rect(32, 96, 32, 32), new Vector2(0.5f, 0.5f), 32f)); // 正面
            sprites.Add(Sprite.Create(texture, new Rect(64, 96, 32, 32), new Vector2(0.5f, 0.5f), 32f)); // 正面歩き2

            sprites.Add(Sprite.Create(texture, new Rect(0, 64, 32, 32), new Vector2(0.5f, 0.5f), 32f)); // 左歩き1
            sprites.Add(Sprite.Create(texture, new Rect(32, 64, 32, 32), new Vector2(0.5f, 0.5f), 32f)); // 左
            sprites.Add(Sprite.Create(texture, new Rect(64, 64, 32, 32), new Vector2(0.5f, 0.5f), 32f)); // 左歩き2

            sprites.Add(Sprite.Create(texture, new Rect(0, 32, 32, 32), new Vector2(0.5f, 0.5f), 32f)); // 右歩き1
            sprites.Add(Sprite.Create(texture, new Rect(32, 32, 32, 32), new Vector2(0.5f, 0.5f), 32f)); // 右
            sprites.Add(Sprite.Create(texture, new Rect(64, 32, 32, 32), new Vector2(0.5f, 0.5f), 32f)); // 右歩き2

            sprites.Add(Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f)); // 後ろ歩き1
            sprites.Add(Sprite.Create(texture, new Rect(32, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f)); // 左
            sprites.Add(Sprite.Create(texture, new Rect(64, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f)); // 後ろ歩き2
            
            GetComponent<SpriteRenderer>().sprite = sprites[1]; // 最初のスプライトを正面にセット
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Texture2SpriteRequest()
    {
        if(HasStateAuthority)
        {
            RPC_Texture2Sprite();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Texture2Sprite()
    {
        Debug.Log(HasInputAuthority);
        if(!HasInputAuthority)
        {
            Invoke("RemotePlayerTexture2Sprite", 2f);
        }
    }

    private void CheckDoubleLogin()
    {
        if(PlayFabData.CurrentRoomPlayersRefs.ContainsKey(this.PlayFabId))
        {
            Debug.LogError("このアカウントは別の端末でログインしています。");
            if(IsOnline)
            {
                Debug.Log("online");
                // Runner.Despawn(PlayFabData.CurrentRoomPlayersRefs[this.PlayFabId].gameObject.GetComponent<NetworkObject>());
                // logout.OnClickLogout();
            }
        }
        else
        {
            PlayFabData.CurrentRoomPlayersRefs[this.PlayFabId] = this;
        }
    }

    private void AddDictDMScripts()
    {
        if(PlayFabData.DictDMScripts.ContainsKey(this.PlayFabId))
        {
            if(PlayFabData.DictDMScripts[this.PlayFabId].playerInstance == null || UnityEngine.Object.ReferenceEquals(PlayFabData.DictDMScripts[this.PlayFabId].playerInstance, null)) // missingに対応
            {
                PlayFabData.DictDMScripts[this.PlayFabId].playerInstance = this.gameObject;
                if(PlayFabData.DictDMScripts[this.PlayFabId].pd == null)
                {
                    PlayFabData.DictDMScripts[this.PlayFabId].pd = this;
                    Debug.Log("add"  + PlayFabData.DictDMScripts[this.PlayFabId].pd);
                }
            }
        }
        else
        {
            Invoke("AddDictDMScripts", 1.0f);
        }
        if(HasInputAuthority)
        {
            Debug.Log(PlayFabData.DictDMScripts[this.PlayFabId].pd + " "  + DisplayName);
        }
    }

    private void SetTextDisplayName()
    {
        TextDisplayName.text = DisplayName;
    }

    private void GetPlayerCombinedInfo()
    {
        var request = new GetPlayerCombinedInfoRequest{PlayFabId = PlayFabSettings.staticPlayer.PlayFabId, InfoRequestParameters = PlayerInfoParams};
        PlayFabClientAPI.GetPlayerCombinedInfo(request, OnGetPlayerCombinedInfoSuccess, error => {Debug.Log("PlayerCombinedInfoの取得に失敗");});
    }
    private void OnGetPlayerCombinedInfoSuccess(GetPlayerCombinedInfoResult result)
    {
        DisplayName = result.InfoResultPayload.UserData["DisplayName"].Value;
        GraduationYear = result.InfoResultPayload.UserData["GraduationYear"].Value;
        // 自分のテキストUIを設定
        TextDisplayName.SetText(DisplayName);
    }

    // プレイヤーが切断したときの処理
    public override void Despawned(NetworkRunner runner, bool hasState)
    {

        if(simpleChatView != null)
        {
            Destroy(simpleChatView.gameObject);
        }
        // if (PlayFabData.DictDMScripts.ContainsKey(this.PlayFabId))
        // {
        //     Debug.Log("jjjjjjjjjjj");
        //     PlayFabData.DictDMScripts[this.PlayFabId].playerInstance = null;
        // }
        if(GetComponent<NetworkObject>().InputAuthority == default(PlayerRef))
        {
            // Debug.Log("aaaaa" + this.DisplayName + "  " + hasState);
            base.Despawned(runner, true);
        }
        else
        {
            if(!HasInputAuthority)
            {
                GameObject nextTarget = null;
                for (int i = 0; i < playerContainer.transform.childCount; i++)
                {
                    GameObject target = playerContainer.transform.GetChild(i).gameObject;
                    NetworkObject networkObj = target.GetComponent<NetworkObject>();
                    if(networkObj.InputAuthority != default(PlayerRef) && !target.name.Equals(this.PlayFabId))
                    {
                        nextTarget = target;
                        break;
                    }
                }

                if(nextTarget != null)
                {
                    if(IsHost)
                    {
                        if(nextTarget == localPlayer)
                        {
                            localPlayer.GetComponent<PlayerData>().IsHost = true;
                            localPlayer.GetComponent<PlayerData>().RPC_Respawn(this.PlayFabId, transform.position);
                        }
                    }
                    else
                    {
                        Debug.Log("uuuuuuuuuuuu");
                        if(nextTarget == localPlayer)
                        {
                            localPlayer.GetComponent<PlayerData>().ps.SpawnAllAI(new List<string> (){this.PlayFabId}, new List<Vector3>(){transform.position});
                        }
                    }
                }
            }
            base.Despawned(runner, true);
        }
    }
/*
    private void ReSpawn()
    {
        Debug.Log("re");
        // GameObject players = GameObject.Find("Players");
        GameObject tmp = null;
        List<string> ids = new List<string>();
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < playerContainer.transform.childCount; i++)
        {
            GameObject target = playerContainer.transform.GetChild(i).gameObject;
            NetworkObject networkObj = target.GetComponent<NetworkObject>();
            PlayerData pd = target.GetComponent<PlayerData>();
            Debug.Log("zzzzzzz" + pd.PlayFabId);
            if(pd.IsOnline && pd.PlayFabId != this.PlayFabId)
            {
                // Debug.Log(pd.PlayFabId);
                tmp = target;
            }
            else
            {
                ids.Add(pd.PlayFabId);
                positions.Add(pd.transform.position);
            }
        }

        if(tmp != null)
        {
            MainLoby.GetComponent<PlayerSpawner>().SpawnAllAI(ids, positions);
            tmp.GetComponent<PlayerData>().IsHost = true;
        }
    }
*/
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_RespawnRequest()
    {
        Debug.Log(this.name + "  " + IsHost);
        Debug.Log("mmmmmmmmmmm");
        // RPC_Respawn();
        if(!IsHost)
        {
            Debug.Log("mmmmmmmmmmm");
            // RPC_Respawn();
        }
    }

    // [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void RPC_Respawn(string prevHostId, Vector3 prevHostPos)
    {
        Debug.Log("rpc_respawn");
        if(HasInputAuthority && IsHost)
        {
            List<string> ids = new List<string>();
            List<Vector3> positions = new List<Vector3>();
            for (int i = 0; i < playerContainer.transform.childCount; i++)
            {
                GameObject target = playerContainer.transform.GetChild(i).gameObject;
                NetworkObject networkObj = target.GetComponent<NetworkObject>();
                PlayerData pd = target.GetComponent<PlayerData>();
                if(networkObj.InputAuthority == default(PlayerRef))
                {
                    ids.Add(target.name);
                    positions.Add(pd.transform.position);
                }
            }
            ids.Add(prevHostId);
            positions.Add(prevHostPos);
            MainLoby.GetComponent<PlayerSpawner>().SpawnAllAI(ids, positions);
            // IsHost = true;
        }
    }
/*
    private IEnumerator ExecuteAfterDelay(NetworkObject Obj, Vector3 pos, float delay)
    {
        yield return new WaitForSeconds(delay); // 指定した時間だけ待機
        Debug.Log("yes");
        cs.TrySpawnObject(Obj, pos);
    }

    public override void Despawned()
    {
        // isOnline = false;
        // SetUserData();
    }
    */

    void OnExitButtonClicked()
    {

    }

    void OnApplicationQuit()
    {
        // IsOnline = false;
        // StartCoroutine(SetUserData());
        // SetUserData();

    }

    private void SetUserData()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>{{"IsOnline", IsOnline.ToString()}, },
            Permission = UserDataPermission.Public
        };
        PlayFabClientAPI.UpdateUserData(request, 
            _ => 
            {
                Debug.Log("IsOnline変更成功");
                if (IsOnline == false)
                {
                    PlayFabSettings.staticPlayer.ForgetAllCredentials();
                }
            }, 
            _=> 
            {
                Debug.Log("IsOnline変更失敗");
            }
        );
    }

    private void UpdatePlayerInfos()
    {
        if (PlayFabData.DictPlayerInfos.Count == 0)
        {
            return;
        }

        string jsonData = JsonConvert.SerializeObject(PlayFabData.DictPlayerInfos);

        var request = new UpdateSharedGroupDataRequest
        {
            SharedGroupId = PlayFabData.CurrentSharedGroupId,
            Data = new Dictionary<string, string> {{"Players", jsonData}},
            Permission = UserDataPermission.Public
        };
        PlayFabClientAPI.UpdateSharedGroupData(request, 
            result => 
                {
                    Debug.Log("Players更新成功");
                },
            e => e.GenerateErrorReport());
    }

    public Distance ContainsId(string id)
    {
        return Targets.Find(distance => distance.Id == id);
    }
}
