using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using Newtonsoft.Json;
using System.Linq;


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

    private bool isInputting = false;

    private TMP_Text inputtingText;

    private bool isOnline;

    public List<string> Targets = new List<string>();
    
    private ChatManager chatManager;
    private ChatUIManager chatUIManager;
    private NetworkRunner runner;

    private LocalGameManager lgm;

    private GameObject simpleChatView;
    private RectTransform rt; // simpleChatViewのrecttransform

    private GameObject localPlayer;

    private Camera cam;

    
    public List<Sprite> sprites  {get; set;} = new List<Sprite>();

    private void Start()
    {   
        chatManager = GameObject.Find("ChatManager").GetComponent<ChatManager>();
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
        lgm = GameObject.Find("LocalGameManager").GetComponent<LocalGameManager>();

        
        if(Object.HasInputAuthority)
        {
            // Invoke("GetPlayerCombinedInfo", 1f); // すぐに実行すると反映されていないため1秒後に実行
            isOnline = true;
            
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
            SetUserData();
            chatManager.chatSender = GetComponent<ChatSender>();

            LoadTexture();
            Invoke("CheckDoubleLogin", 0.5f);
        }
        else
        {
            PlayFabData.CurrentRoomPlayersRefs[this.PlayFabId] = this;
            Debug.Log("start" + DisplayName);
            
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
        //Debug.Log(PlayFabData.DictDMScripts[this.PlayFabId]);
        Invoke("AddDictDMScripts", 1.5f);
        Invoke("ClickDM", 1f);
    }

    public void Update()
    {
        rt.position = cam.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y, 0f));

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
                if(inputtingText == null)
                {
                    inputtingText = Instantiate(chatUIManager.text_simple_messagePref, new Vector3(0f, 0f, 0f), Quaternion.identity);
                    inputtingText.transform.SetParent(PlayFabData.DictSpawnerSimpleMessage[this.PlayFabId].transform);
                    inputtingText.GetComponent<TMP_Text>().text = "入力中...";
                    inputtingText.color = new Color32(255, 255, 255, 180);
                }
            }

            isInputting = IsInputting;
        }
        


        /// 以降ローカルプレイヤー
        if(!Object.HasStateAuthority)
        {
            return;
        }

        if(chatUIManager.inputField.isFocused && !string.IsNullOrEmpty(chatUIManager.inputField.text))
        {
            IsInputting = true;
        }
        else
        {
            IsInputting = false;
        }

        foreach(var pr in PlayFabData.CurrentRoomPlayersRefs)
        {
            if(pr.Key != this.PlayFabId)
            {
                if(pr.Value == null)
                {
                    if(Targets.Contains(pr.Key) & lgm.LocalGameState == LocalGameManager.GameState.Playing)
                    {
                        Targets.Remove(pr.Key);
                        ClickDM();
                        if(Targets.Count == 0)
                        {
                            chatUIManager.inputField.gameObject.SetActive(false);
                        }
                    }
                    break;
                }

                Vector3 dist = new Vector3(Mathf.Abs(pr.Value.transform.position.x - this.transform.position.x), Mathf.Abs(pr.Value.transform.position.y - this.transform.position.y), 0f);
                if(dist.x < 3.0f & dist.y < 3.0f)
                {
                    if(!Targets.Contains(pr.Key) & lgm.LocalGameState == LocalGameManager.GameState.Playing)
                    {
                        Targets.Add(pr.Key);
                        ClickDM();
                        chatUIManager.inputField.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if(Targets.Contains(pr.Key) & lgm.LocalGameState == LocalGameManager.GameState.Playing)
                    {
                        Targets.Remove(pr.Key);
                        ClickDM();
                        if(Targets.Count == 0)
                        {
                            chatUIManager.inputField.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        if(PlayFabData.CurrentMessageTarget != this.PlayFabId & Targets.Count == 0 & lgm.LocalGameState == LocalGameManager.GameState.Playing)
        {
            ClickDM();
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

    public void ClickDM()
    {
        if(Targets.Count > 0)
        {
            // string t = "";
            if(PlayFabData.DictDMScripts.ContainsKey(Targets[0]))
            {
                PlayFabData.DictDMScripts[Targets[0]].OnClickButton();
            }
            chatUIManager.text_targets.text = "To : " + string.Join(", ", PlayFabData.CurrentRoomPlayers[Targets[0]]);
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


    public void LoadTexture()
    {
        if(!string.IsNullOrEmpty(PlayFabData.MyTexturePath))
        {
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
            int rand = Random.Range(0, paths.Count);
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
            GameObject.Find("Logout").GetComponent<PlayFabLogout>().OnClickLogout();
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
            if(PlayFabData.DictDMScripts[this.PlayFabId].playerInstance == null)
            {
                PlayFabData.DictDMScripts[this.PlayFabId].playerInstance = this.gameObject;
            }
        }
        else
        {
            Invoke("AddDictDMScripts", 1.0f);
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
        Debug.Log("despauwnd");
        base.Despawned(runner, true);
        if (Object.HasInputAuthority)
        {
            isOnline = false;
            PlayFabData.Initialize();
            Debug.Log("despauwnd" + Object);
        }
    }

    private void OnDestroy()
    {
        isOnline = false;
        Destroy(simpleChatView.gameObject);
        // SetUserData();
    }

    void OnExitButtonClicked()
    {

    }

    void OnApplicationQuit()
    {
        isOnline = false;
        // StartCoroutine(SetUserData());
        // SetUserData();

    }

    private void SetUserData()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>{{"IsOnline", isOnline.ToString()}, },
            Permission = UserDataPermission.Public
        };
        PlayFabClientAPI.UpdateUserData(request, 
            _ => 
            {
                Debug.Log("IsOnline変更成功");
                if (isOnline == false)
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
}
