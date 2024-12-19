using System.Collections.Generic;
using Fusion;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using Newtonsoft.Json;
using System.Linq;
using System;
using ExitGames.Client.Photon;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR;

[Serializable]
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
    public string GetPlayFabId()
    {
        return PlayFabId;
    }

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
    public bool GetIsHost()
    {
        return IsHost;
    }

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
    public bool WasInTarget = false; // ローカルプレイヤーのTargetsに自分が入っていたかどうか
    public bool IsInTarget = false; // ローカルプレイヤーのTargetsに自分が入っているかどうか
    [Networked]
    public bool IsMainTarget {get; set;} = false; // ローカルプレイヤーのメインターゲットかどうか
    void SetIsMainTarget(bool value)
    {
        IsMainTarget = value;
    }
    private ChatManager chatManager;
    private ChatUIManager chatUIManager;
    private NetworkRunner runner;

    private LocalGameManager lgm;
    private PlayFabLogout logout;

    public GameObject simpleChatView;
    public GameObject reactionObj;
    private RectTransform svrt; // simpleChatViewのrecttransform
    private RectTransform rort; // reactionObjのrecttransform

    public int ReactionNum = -1;

    public Sprite ReactionSprite;

    public GameObject localPlayer;

    private Camera cam;

    public List<Sprite> sprites  {get; set;} = new List<Sprite>();
    public PlayerSpawner ps;
    private GameObject playerContainer;
    private ChatSender cs;
    const float TALKDIST = 3.0f;

    // Dictionary<string, Vector3> DictDistance = new Dictionary<string, Vector3>();
    public Material MyMat;
    GameObject MainLoby;
    GPTSendChat gsc;
    public bool CheckMeetingRoomEntry = false;

    [Networked]
    public bool isSitting {get; set;} = false;


    [SerializeField]
    public ChatGPTConnection chatGPTConnection;
    private PlayerMovement pm;

    public bool IsChatting = false;
    public bool IsAI = false;
    bool GetIsAI()
    {
        return IsAI;
    }

    private float deltaTime_API = 0;
    private float deltaTime_move = 0;
    private float interval_API = 15.0f;
    private float interval_move = 0.125f;
    public string CurrentContent = "";
    private int beforeTargetsCount = 0;

    [SerializeField]
    public Queue<int> Q_nextInputs = new Queue<int>();
    public Queue<int> Q_moveLog = new Queue<int>();

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
        GameObject cm = GameObject.Find("ConnectionManager");
        if(cm.transform.childCount > 0)
        {
            MainLoby = cm.transform.GetChild(0).gameObject;
        }
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

            PlayFabData.CurrentMessageTarget = this.PlayFabId;

            // stateInfo
            int now = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds + 3600 * 9; // 日本時間
            PlayFabData.DictPlayerStateInfos[this.PlayFabId] = new PlayerStateInfo(now, this.DisplayName, this.transform.position, pm.CurrentInputType, this.IsAI, this.IsChatting, "", this.isInputting, this.ReactionNum, this.Q_moveLog.ToArray());
            /*
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(now);
            PlayFabData.LoginStartTime = dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss");
            PlayFabData.DictLoginTime[PlayFabData.LoginStartTime] = PlayFabData.LoginTime.ToString();
            */
            LoadTexture();
            CheckDoubleLogin();
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
            if(PlayFabData.DictPlayerInfos.ContainsKey(PlayFabSettings.staticPlayer.PlayFabId))
            {
                string prompt = 
                "私のユーザーIDは" + PlayFabSettings.staticPlayer.PlayFabId + "で、ユーザー名は" + PlayFabData.DictPlayerInfos[PlayFabSettings.staticPlayer.PlayFabId].name + "です。つまり、私の名前は" + PlayFabData.DictPlayerInfos[PlayFabSettings.staticPlayer.PlayFabId].name + "です。" + 
                "今からいうことを絶対に忘れず、かつ、守ってください。\n" + GPTSendChat.Prompt;
                chatGPTConnection = new ChatGPTConnection(GPTSendChat.OpenAIApiKey, prompt);
            }
            Invoke("RemotePlayerTexture2Sprite", 1f);
            // 他ユーザーのテキストUIを設定
            Invoke("SetTextDisplayName", 2f); // すぐに実行すると反映されていないため1秒後に実行
        }
        /*
        if(PlayFabData.ChairOrder.Count == 0)
        {
            PlayFabData.ChairOrder.Add(this.PlayFabId);
        }

        for(int i = 0; i < PlayFabData.ChairOrder.Count; i++)
        {
            if(string.IsNullOrEmpty(PlayFabData.ChairOrder[i]))
            {
                Debug.Log(this.PlayFabId);
                PlayFabData.ChairOrder[i] = this.PlayFabId;
                break;
            }
            else
            {
                if(i == PlayFabData.ChairOrder.Count - 1)
                {
                    PlayFabData.ChairOrder.Add(this.PlayFabId);
                }
            }
        }
        */
        localPlayer = GameObject.Find("LocalPlayer");
        simpleChatView = Instantiate(chatUIManager.spawner_simple_message, new Vector3(0f,0f,0f), Quaternion.identity);
        simpleChatView.transform.SetParent(chatUIManager.Canvas.transform);
        PlayFabData.DictSpawnerSimpleMessage[this.PlayFabId] = simpleChatView.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        svrt = simpleChatView.GetComponent<RectTransform>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();

        reactionObj = Instantiate(chatUIManager.reaction_Pref, new Vector3(0f,0f,0f), Quaternion.identity);
        reactionObj.transform.SetParent(chatUIManager.Canvas.transform);
        rort = reactionObj.GetComponent<RectTransform>();
        ReactionSprite = reactionObj.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Sprite>();
        rort.gameObject.SetActive(false);

        if(cm != null && cm.transform.childCount > 0)
        {
            ps = cm.transform.GetChild(0).gameObject.GetComponent<PlayerSpawner>();
        }

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
        if(this.PlayFabId == PlayFabSettings.staticPlayer.PlayFabId)
        {
            PlayFabData.LoginTime += Time.deltaTime;
            PlayFabData.DictLoginTime[PlayFabData.LoginStartTime] = PlayFabData.LoginTime.ToString();
            if(PlayFabData.LoginTime > PlayFabData.SaveLoginTimeDuration)
            {
                int time = Mathf.FloorToInt(PlayFabData.LoginTime);
                int hours = time / 3600;
                int minutes = (time % 3600) / 60;
                int secs = time % 60;

                // フォーマット
                string timeStr = $"{hours:D2}:{minutes:D2}:{secs:D2}";
                PlayFabData.DictLoginTime[PlayFabData.LoginStartTime] = timeStr;
                // 保存
                var request = new UpdateUserDataRequest
                {
                    Data = new Dictionary<string, string>
                    {
                        {"Login", JsonConvert.SerializeObject(PlayFabData.DictLoginTime)},
                    }
                };
                PlayFabClientAPI.UpdateUserData(request, result => Debug.Log("ログイン時間更新成功"), error => {Debug.Log("ログイン時間の更新失敗" + error.GenerateErrorReport());});
                
                PlayFabData.SaveLoginTimeDuration += 30f;
            }
        }

        if (GetComponent<NetworkObject>().InputAuthority == default(PlayerRef))
        {
            IsAI = true;
        }
        else
        {
            IsAI = false;
        }

        if(svrt != null)
        {
            svrt.position = cam.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y, 0f));
        }

        if(rort != null)
        {
            rort.position = cam.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y, 0f));
            rort.anchoredPosition = new Vector2(rort.anchoredPosition.x + 100, rort.anchoredPosition.y + 50);
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
                if(!IsAI)
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

        if(transform.position.x * 2 % 2 == 0 && transform.position.y * 2 % 2 == 0)
        {
            if(PlayFabData.IsAI)
            {
                MoveAI();
            }
            Vector3 dist = transform.position;
            if(!PlayFabData.DictDistance.ContainsKey(this.PlayFabId))
            {
                PlayFabData.DictDistance[this.PlayFabId] = dist;
                return;
            }

            if(PlayFabData.DictDistance[this.PlayFabId] != dist)
            {
                PlayFabData.DictDistance[this.PlayFabId] = dist;
            }
            if(Math.Abs(transform.position.x - localPlayer.transform.position.x) <= TALKDIST && Math.Abs(transform.position.y - localPlayer.transform.position.y) <= TALKDIST)
            {
                IsInTarget = true;
            }
            else
            {
                IsInTarget = false;
            }
            /*
            if(IsHost)
            {
                List<string> mainTargets = new List<string>();
                for(int i = 0; i < playerContainer.transform.childCount; i++)
                {
                    PlayerData playerData = playerContainer.transform.GetChild(i).GetComponent<PlayerData>();
                    if(!playerData.IsAI)
                    {
                        for(int j = 0; j < playerData.Targets.Count; j++)
                        {
                            if(j == 0)
                            {
                                mainTargets.Add(playerData.Targets[j].Id);
                                PlayFabData.CurrentRoomPlayersRefs[playerData.Targets[j].Id].SetIsMainTarget(true);
                            }
                        }
                    }
                }
                for(int i = 0; i < playerContainer.transform.childCount; i++)
                {
                    PlayerData playerData = playerContainer.transform.GetChild(i).GetComponent<PlayerData>();
                    if(!mainTargets.Contains(playerData.PlayFabId))
                    {
                        playerData.SetIsMainTarget(false);
                    }
                }
            }
            */

            foreach(var pr in PlayFabData.DictDistance)
            {
                if(pr.Key != this.PlayFabId)
                {
                    if(Math.Abs(pr.Value.x - transform.position.x) <= TALKDIST && Math.Abs(pr.Value.y - transform.position.y) <= TALKDIST)
                    {
                        if(ContainsId(pr.Key) == null)
                        {
                            if(IsAI)
                            {
                                if(PlayFabData.CurrentRoomPlayersRefs[pr.Key].GetIsAI() == false)
                                {
                                    Targets.Add(new Distance(pr.Key, pr.Value));
                                }
                            }
                            else
                            {
                                Targets.Add(new Distance(pr.Key, pr.Value));
                            }
                        }
                    }
                    else
                    {
                        Distance d = ContainsId(pr.Key);
                        if(d != null)
                        {
                            if(IsAI)
                            {
                                if(PlayFabData.CurrentRoomPlayersRefs[pr.Key].GetIsAI() == false)
                                {
                                    Targets.Remove(d);
                                }
                            }
                            else
                            {
                                Targets.Remove(d);
                            }
                        }
                    }
                }
            }

            if(IsAI)
            {
                bool isChanged = false;
                if(Targets.Count > 0)
                {
                    foreach(var target in Targets)
                    {
                        PlayerData targetData = PlayFabData.CurrentRoomPlayersRefs[target.Id];
                        if(targetData.Targets.Count > 0)
                        {
                            if(targetData.Targets[0].Id == this.PlayFabId)
                            {
                                isChanged = true;
                                SetIsMainTarget(true);
                            }
                        }
                    }
                }
                else
                {
                    SetIsMainTarget(false);
                }

                if(isChanged == false)
                {
                    SetIsMainTarget(false);
                }
            }

            if(!HasInputAuthority)
            {
                if(WasInTarget != IsInTarget)
                {
                    if(IsInTarget)
                    {
                        SetShaderColor(Color.green);
                        SetShaderOutlineTickness(0.005f);
                        WasInTarget = true;
                    }
                    else
                    {
                        SetShaderOutlineTickness(0f);
                        WasInTarget = false;
                    }
                }
            }
        }

        if(this.PlayFabId != PlayFabSettings.staticPlayer.PlayFabId)
        {
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

        if(Targets.Count != 0)
        {
            chatUIManager.inputField.gameObject.SetActive(true);
        }
        else
        {
            chatUIManager.inputField.gameObject.SetActive(false);
            if(beforeTargetsCount != 0)
            {
                for(int i = 0;  i < playerContainer.transform.childCount; i++)
                {
                    var pd = playerContainer.transform.GetChild(i).gameObject.GetComponent<PlayerData>();
                    pd.SetShaderColor(Color.green);
                    pd.SetShaderOutlineTickness(0f);
                }
            }
        }

        beforeTargetsCount = Targets.Count;

        if(Targets.Count > 0)
        {
            for(int i = 0; i < playerContainer.transform.childCount; i++)
            {
                var playerData = playerContainer.transform.GetChild(i).GetComponent<PlayerData>();
                PlayFabData.CurrentRoomPlayersRefs[playerData.GetPlayFabId()].SetShaderColor(Color.green);
                PlayFabData.CurrentRoomPlayersRefs[playerData.GetPlayFabId()].SetShaderOutlineTickness(0f);
            }
            foreach(var target in Targets)
            {
                if(target.Id == Targets[0].Id)
                {
                    PlayFabData.CurrentRoomPlayersRefs[target.Id].SetShaderColor(Color.red);
                }
                else
                {
                    PlayFabData.CurrentRoomPlayersRefs[target.Id].SetShaderColor(Color.green);
                }
                PlayFabData.CurrentRoomPlayersRefs[target.Id].SetShaderOutlineTickness(0.005f);
            }
        }

        if(lgm.LocalGameState == LocalGameManager.GameState.Playing)
        {
             if(Targets.Count == 0)
            {
                if(PlayFabData.CurrentMessageTarget != this.PlayFabId)
                {
                    PlayFabData.CurrentMessageTarget = this.PlayFabId;
                    ClickDM();
                }
            }
            else
            {
                if(PlayFabData.CurrentMessageTarget != Targets[0].Id)
                {
                    PlayFabData.CurrentMessageTarget = Targets[0].Id;
                    ClickDM();
                }
            }
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
                for(int i = 0; i < Targets.Count; i++)
                {
                    //PlayFabData.CurrentRoomPlayersRefs[Targets[i].Id].SetIsMainTarget(false);
                    if(i == 0)
                    {
                        // Debug.Log(PlayFabData.DictPlayerInfos[Targets[i].Id].name);
                        // PlayFabData.CurrentRoomPlayersRefs[Targets[i].Id].SetIsMainTarget(true);
                    }
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
                for(int i = 0; i < Targets.Count; i++)
                {
                    // PlayFabData.CurrentRoomPlayersRefs[Targets[i].Id].SetIsMainTarget(false);
                    if(i == 0)
                    {
                        // Debug.Log(PlayFabData.DictPlayerInfos[Targets[i].Id].name);
                        // PlayFabData.CurrentRoomPlayersRefs[Targets[i].Id].SetIsMainTarget(true);
                    }
                }
                if(PlayFabData.CurrentMessageTarget != Targets[0].Id && lgm.LocalGameState == LocalGameManager.GameState.Playing)
                {
                    // Debug.Log(Targets.Count + "  " + PlayFabData.DictPlayerInfos[Targets[0].Id].name);
                    // ClickDM();
                }
            }
        }

        /*
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

        if(isChangeDMTaget & lgm.LocalGameState == LocalGameManager.GameState.Playing)
        {
            Debug.Log("aaa");
            ClickDM();
            isChangeDMTaget = false;
        }
        */
        
        
    }



    private void MoveAI()
    {
        // deltaTime_API += Time.deltaTime;
        deltaTime_move += Time.deltaTime;
        /*
        if(deltaTime_API >= interval_API)
        {
            RPC_AddStateInfoRequest();
            deltaTime_API = 0f;
        }
        */

        if (deltaTime_move >= interval_move && Q_nextInputs.Count > 0)
        {
            if(!IsAI)
            {
                Q_nextInputs.Clear();
                return;
            }
            /*
            bool isInTarget = false;
            foreach(var target in Targets)
            {
                if(!PlayFabData.CurrentRoomPlayersRefs[target.Id].IsAI)
                {
                    isInTarget = true;
                    break;
                }
            }
            */
            if(IsMainTarget || !localPlayer.GetComponent<PlayerData>().IsHost)
            {
                return;
            }
            int dir = Q_nextInputs.Dequeue();

            if (dir == 0)
            {
                pm.OnMove(new Vector3(0f, -pm._moveAmount, 0f), MyNetworkInput.InputType.BACKWARD);
            }
            if (dir == 1)
            {
                pm.OnMove(new Vector3(-pm._moveAmount, 0f, 0f), MyNetworkInput.InputType.LEFT);
            }
            if (dir == 2)
            {
                pm.OnMove(new Vector3(pm._moveAmount, 0f, 0f), MyNetworkInput.InputType.RIGHT);
            }
            if (dir == 3)
            {
                pm.OnMove(new Vector3(0f, pm._moveAmount, 0f), MyNetworkInput.InputType.FORWARD);
            }
            if(Q_nextInputs.Count > 0)
            {
                if(Q_nextInputs.Peek() == dir && pm.IsNearChairTile(transform.position, dir) && transform.position.x > 0)
                {
                    List<int> inputs = Q_nextInputs.ToList<int>();
                    int nextDir = Q_nextInputs.Dequeue();
                    if(nextDir == (int)MyNetworkInput.InputType.BACKWARD || nextDir == (int)MyNetworkInput.InputType.FORWARD)
                    {
                        inputs.Insert(0, (int)MyNetworkInput.InputType.RIGHT);
                        inputs.Insert(0, (int)MyNetworkInput.InputType.RIGHT);
                        inputs.Insert(0, nextDir);
                        inputs.Insert(0, nextDir);
                        inputs.Insert(0, nextDir);
                        inputs.Insert(0, (int)MyNetworkInput.InputType.LEFT);
                        inputs.Insert(0, (int)MyNetworkInput.InputType.LEFT);
                        inputs.Insert(0, nextDir);
                    }
                    else
                    {
                        inputs.Insert(0, (int)MyNetworkInput.InputType.BACKWARD);
                        inputs.Insert(0, (int)MyNetworkInput.InputType.BACKWARD);
                        inputs.Insert(0, nextDir);
                        inputs.Insert(0, nextDir);
                        inputs.Insert(0, nextDir);
                        inputs.Insert(0, (int)MyNetworkInput.InputType.FORWARD);
                        inputs.Insert(0, (int)MyNetworkInput.InputType.FORWARD);
                        inputs.Insert(0, nextDir);
                    }

                    Q_nextInputs.Clear();
                    foreach(var input in inputs)
                    {
                        Q_nextInputs.Enqueue(input);
                    }
                }
            }

            deltaTime_move = 0f;
        }
    }

    [Rpc(RpcSources.All,RpcTargets.StateAuthority)]
    private async void RPC_AddStateInfoRequest()
    {
        if (HasStateAuthority && IsHost && !string.IsNullOrEmpty(this.DisplayName))
        {
            /*
            foreach (var player in PlayFabData.CurrentRoomPlayersRefs)
            {
                player.Value.RPC_AddStateInfo();
            }
            */

            for (int i = 0; i < playerContainer.transform.childCount; i++)
            {
                var playerData = playerContainer.transform.GetChild(i).GetComponent<PlayerData>();
                if(!playerData.IsAI)
                {
                    // Debug.Log(playerContainer.transform.GetChild(i).name);
                    playerData.UpdateListStateInfo(PlayFabData.DictListPlayerStateInfo, 30);
                }
                else 
                {
                    playerData.UpdateListStateInfo(PlayFabData.DictListLatestPlayerStateInfo, 5);
                }
            }

            string jsonData = JsonConvert.SerializeObject(PlayFabData.DictListPlayerStateInfo);
            var request = new UpdateSharedGroupDataRequest
            {
                SharedGroupId = PlayFabData.CurrentSharedGroupId,
                Data = new Dictionary<string, string> { {"DictListPlayerStateInfo", jsonData}}
            };
            PlayFabClientAPI.UpdateSharedGroupData(request, _ => Debug.Log("プレイヤー情報更新成功"), _ => Debug.Log("プレイヤー情報更新失敗"));

            // gpt送信
            string prompt = 
                "私のidは" + PlayFabSettings.staticPlayer.PlayFabId + "で、名前は" + GameObject.Find("LocalPlayer").GetComponent<PlayerData>().DisplayName + "です。" + 
                "あなたは2D世界のAIプレイヤーです。上下左右の向きと現在の座標を持ち、移動する時は必ずその方向に方向転換した後、1マスずつ動きます。つまり、移動する方向をすでに向いている時はその方向に移動できますが、移動する方向と別の方向を向いている場合はその方向を向いてから移動しなければいけません" + 
                "あなた以外にもプレイヤーがおり、それぞれコミュニケーションをとっています。ただし、各プレイヤーは3マス以内に近づかないと会話することができません。また、他のプレイヤーの中にはAIも混じっています。" +
                "あなたにはこれから全てのプレイヤーの行動の履歴が送られます。" + 
                "具体的には各プレイヤーごとに time:現在時刻（日本時刻のunixtime）、 id:プレイヤーを識別するid、name: プレイヤーの名前、 pos: プレイヤーのボジション（正数ではない場合はdirの方向に移動中）、 dir: プレイヤーが向いている方向（動かない:-1、 下:0、 左:1、 右:2、 上:3）、 IsAI: プレイヤーがAIかどうか、 IsChatting: 発言中かどうか、 content: 発言内容、 IsInputting: これから発言する内容を入力中かどうか、 reactionNum:押したリアクションボタンの種類（-1: 無し、 0: グッド、 1: 笑顔、 2: ハート、 3: 泣、 4: 怒り、 5: 考え中）、 moveLog: 移動のログ（dirの方向と同じ） という情報が与えられます。" +
                "あなたはこれらの情報をもとに各AIプレイヤーの次の行動を30個ずつバラバラに生成してもらいます。位置を移動するか、その場で待機するかを常に行ってください。必要のない方向転換ばかり行わないでください。" +
                "以下はマップを（#:床、 .:壁、 *:会議室、 +:PC）で表した文字列の情報です。改行で次の行を表しています。左上端の座標は(-25, 19)、右上端の座標は(18, 19)、左下端の座標は(-25, -5)、右下端の座標は(18, -5)です。壁やPCに衝突し続けないように生成してください。また、会議室には絶対に入らないでください。\n" +
                "............................................\n" +
                ".**************.###########################.\n" +
                ".**************.###########################.\n" +
                ".******++******.###########################.\n" +
                ".*****+**+*****.###########################.\n" +
                ".*****+**+*****######+###+###+###+###+#####.\n" +
                ".*****+**+*****############################.\n" +
                ".*****+**+*****############################.\n" +
                ".******++******.###########################.\n" +
                ".**************.#####+###+###+###+###+#####.\n" +
                ".**************.###########################.\n" +
                ".**************.###########################.\n" +
                "................###########################.\n" +
                ".**************.###########################.\n" +
                ".**************.###########################.\n" +
                ".**************.###########################.\n" +
                ".******++******############################.\n" +
                ".*****+**+*****############################.\n" +
                ".*****+**+*****############################.\n" +
                ".*****+**+*****.###########################.\n" +
                ".*****+**+*****.###########################.\n" +
                ".******++******.###########################.\n" +
                ".**************.###########################.\n" +
                ".**************.###########################.\n" +
                "............................................\n" +
                // "私（" + PlayFabSettings.staticPlayer.PlayFabId + "）の周りにAIが集まるように行動を生成してください。もしも私の周りに集まったと判断した時は-1の要素を送信し続けてください。" + 
                "プレイヤーhogehoge1234が生成した向きが0,2,1,3,3,2,1,2,0,1,-1,-1,-1,-1,-1,0,2,1,3,3,2,1,2,0,1,-1,-1,-1,-1,-1、プレイヤーtesttest1234が生成した向きが-1,-1,-1,-1,-1,1,2,0,3,3,0,1,3,0,1,-1,-1,-1,-1,-1,1,2,0,3,3,0,1,3,0,1の場合は形式は以下の通りです。基本的に座標の履歴とmoveLogを参考にしてその人と同じ位置で同じ行動をするようにしたり、その人がよく話す人の近くに行くように行動を生成してください。また、適切な回答を生成できないと判断した場合はその理由を述べてください。その時は過去の履歴の座標と同じ位置に行き、moveLogと同じ行動をしてください。" +
                "{" +
                "\"hogehoge1234\": [0,2,1,3,3,2,1,2,0,1,-1,-1,-1,-1,-1,0,2,1,3,3,2,1,2,0,1,-1,-1,-1,-1,-1]," +
                "\"testtest1234\": [-1,-1,-1,-1,-1,1,2,0,3,3,0,1,3,0,1,-1,-1,-1,-1,-1,1,2,0,3,3,0,1,3,0,1]" +
                "}" +
                "\n";
            ChatGPTConnection chatGPTConnection = new ChatGPTConnection(apikey: GPTSendChat.OpenAIApiKey, prompt: prompt);
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            string data = JsonConvert.SerializeObject(PlayFabData.DictListPlayerStateInfo, settings);
            string latestData = JsonConvert.SerializeObject(PlayFabData.DictListLatestPlayerStateInfo, settings);
            string str =
                "これはonlineの時の行動履歴です。" + data + "\n" +
                "これは直近のAIの行動です。" + latestData;
            var response = await chatGPTConnection.RequestAsync(str);
            // 応答があれば処理を行う
            if (response.choices != null && response.choices.Length > 0)
            {
                var choice = response.choices[0];

                Debug.Log("ChatGPT Response: " + choice.message.content);
                Dictionary<string, List<int>> result = new Dictionary<string, List<int>>();
                string output = choice.message.content;
                int startIndex = output.IndexOf('{');
                int endIndex = output.IndexOf('}');
                string resultStr = (startIndex >= 0 && endIndex > startIndex) ? output.Substring(startIndex, endIndex - startIndex + 1) : "";
                if (!string.IsNullOrEmpty(resultStr))
                {
                    result = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(resultStr);
                }

                if (result != null)
                {
                    foreach (var ai in result)
                    {
                        var q = PlayFabData.CurrentRoomPlayersRefs[ai.Key].Q_nextInputs;
                        q.Clear();
                        for (int i = 0; i < ai.Value.Count; i++)
                        {
                            q.Enqueue(ai.Value[i]);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("失敗");
            }
        }
    }
    /*
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
                    }
                }
            }
            int now = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds + 3600 * 9; // 日本時間
            PlayFabData.DictPlayerStateInfos[this.PlayFabId] = new PlayerStateInfo (now, this.DisplayName, this.transform.position, pm.CurrentInputType, IsAI, IsChatting, content, this.isInputting);
        }
    }
    */

    private void SetShaderColor(Color color)
    {
        MyMat.SetColor("_OutlineColor", color);
    }

    private void SetShaderOutlineTickness(float value)
    {
        MyMat.SetFloat("_OutlineThickness", value);
    }

    public void ClickDM()
    {
        if(Targets.Count > 0)
        {
            if(PlayFabData.DictDMScripts.ContainsKey(Targets[0].Id))
            {
                PlayFabData.DictDMScripts[Targets[0].Id].OnClickButton();
                chatUIManager.text_targets.text = "To : " + string.Join(", ", PlayFabData.DictPlayerInfos[Targets[0].Id].name);
            }
        }
        else
        {
            if(PlayFabData.DictDMScripts.ContainsKey(this.PlayFabId))
            {
                PlayFabData.DictDMScripts[this.PlayFabId].OnClickButton();
            }
            chatUIManager.text_targets.text = "";
        }
    }

    public void UpdateListStateInfo(Dictionary<string, List<PlayerStateInfo>> targetList, int maxLength)
    {
        if(!targetList.ContainsKey(this.PlayFabId))
        {
            targetList[this.PlayFabId] = new List<PlayerStateInfo>();
        }

        while(true)
        {
            if(targetList[this.PlayFabId].Count >= maxLength)
            {
                targetList[this.PlayFabId].RemoveAt(0);
            }
            else
            {
                break;
            }
        }

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
                }
            }
        }
        int now = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds + 3600 * 9; // 日本時間（unixtime）
        targetList[this.PlayFabId].Add(new PlayerStateInfo (now, this.DisplayName, this.transform.position, pm.CurrentInputType, IsAI, IsChatting, content, this.isInputting, this.ReactionNum, this.Q_moveLog.ToArray()));
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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SendReactionRequest(string playFabId, int reactionNum)
    {
        if(HasStateAuthority)
        {
            RPC_SendReaction(playFabId, reactionNum);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SendReaction(string playFabId, int reactionNum)
    {
        if(this.PlayFabId == playFabId && this.ReactionNum < 0)
        {
            this.ReactionNum = reactionNum;
            rort.gameObject.SetActive(true);
            Image image = rort.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Image>();
            image.color = new Color(255, 255, 255, 255);
            image.sprite = chatUIManager.reactions[reactionNum];
            Invoke("DeleteReaction", 5f);
        }
    }

    private void DeleteReaction()
    {
        ReactionNum = -1;
        Image image = rort.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Image>();
        image.sprite = null;
        reactionObj.gameObject.SetActive(false);
    }

    private void CheckDoubleLogin()
    {
        if(PlayFabData.CurrentRoomPlayersRefs.ContainsKey(this.PlayFabId))
        {
            // Debug.LogError("このアカウントは別の端末でログインしています。");
            if(GameObject.Find(this.PlayFabId))
            {
                Debug.Log("double login");
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
        if (reactionObj != null)
        {
            Destroy(reactionObj.gameObject);
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
/*
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
*/
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

/*
    private void UpdatePlayerStateInfo()
    {
        if (PlayFabData.ListDictPlayerStateInfo.Count == 0)
        {
            return;
        }

        string jsonData = JsonConvert.SerializeObject(PlayFabData.ListDictPlayerStateInfo);

        var request = new UpdateSharedGroupDataRequest
        {
            SharedGroupId = PlayFabData.CurrentSharedGroupId,
            Data = new Dictionary<string, string> {{"PlayerStateInfos", jsonData}},
            Permission = UserDataPermission.Public
        };
        PlayFabClientAPI.UpdateSharedGroupData(request, 
            result => 
                {
                    Debug.Log("Players更新成功");
                },
            e => e.GenerateErrorReport());
    }
*/
    public Distance ContainsId(string id)
    {
        return Targets.Find(distance => distance.Id == id);
    }
}
