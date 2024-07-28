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

    private bool isOnline;
    
    private ChatManager chatManager;
    private NetworkRunner runner;

    
    public List<Sprite> sprites  {get; set;} = new List<Sprite>();

    private void Start()
    {   
        chatManager = GameObject.Find("ChatManager").GetComponent<ChatManager>();
        
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
            Invoke("CheckDoubleLogin", 0.1f);
        }
        else
        {
            Debug.Log(texturePath);

            PlayFabData.CurrentRoomPlayersRefs[this.PlayFabId] = this;
            Debug.Log("start" + DisplayName);
            
            Invoke("RemotePlayerTexture2Sprite", 1f);
            // 他ユーザーのテキストUIを設定
            Invoke("SetTextDisplayName", 2f); // すぐに実行すると反映されていないため1秒後に実行
        }
        //Debug.Log(PlayFabData.DictDMScripts[this.PlayFabId]);
        Invoke("AddDictDMScripts", 1.5f);
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
        if(PlayFabData.DictDMScripts.ContainsKey(this.PlayFabId) & PlayFabData.DictDMScripts[this.PlayFabId].playerInstance == null)
        {
            PlayFabData.DictDMScripts[this.PlayFabId].playerInstance = this.gameObject;
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
