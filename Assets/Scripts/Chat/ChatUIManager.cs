using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine.Networking.PlayerConnection;
using Newtonsoft.Json;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.SocialPlatforms;

public class ChatUIManager : MonoBehaviour
{
    private ChatManager chatManager;
    private LocalGameManager lgm;
    [SerializeField]
    public Dictionary<string, int> DictReadMessageCount = new Dictionary<string, int>();

    // chat画面のUI
    [SerializeField]
    private GameObject ChatUI;
    [SerializeField]
    private GameObject spawner_channel;
    [SerializeField]
    private Button button_channelTarget; // prefab
    [SerializeField]
    private GameObject spawner_DM;
    [SerializeField]
    private Button button_DMTarget; // prefab
    [SerializeField]
    public TMP_InputField inputField;
    [SerializeField]
    private Button button_submit;
    [SerializeField]
    public GameObject spawner_message;
    [SerializeField]
    private TMP_Text text_messagePref; // prefab
    [SerializeField]
    public TMP_Text text_channelName;
    [SerializeField]
    public ScrollRect scrollRect; // scrollview
    [SerializeField]
    public ContentSizeFitter csf; // contentのcontentsizefilter

    // channel作成時に使用
    [SerializeField]
    private Button button_moveCreateChannel;
    [SerializeField]
    private TMP_InputField channelName;
    [SerializeField]
    private List<Toggle> addMemberToggles;
    [SerializeField]
    private Button button_return;
    [SerializeField]
    private Button button_create;
    [SerializeField]
    private GameObject panel_CHDM;
    [SerializeField]
    private GameObject panel_createChannel;
    [SerializeField]
    private GameObject spawner_members;
    [SerializeField]
    private Toggle toggle_member; // prefab
    [SerializeField]
    private Button button_addAllmembers;
    [SerializeField]
    private GameObject panel_Members;
    [SerializeField]
    private TMP_Dropdown dd_channelType;

    void Start()
    {
        DictReadMessageCount = PlayFabData.DictReadMessageCount;
        chatManager = GetComponent<ChatManager>();
        lgm = GameObject.Find("LocalGameManager").GetComponent<LocalGameManager>();
        csf = spawner_message.GetComponent<ContentSizeFitter>();
        text_channelName.text = "# " + PlayFabData.CurrentRoomChannels[PlayFabData.CurrentChannelId].ChannelName; // generalなので#をつける
        DisplayChannelTargets();
        DisplayDMTargets();
    }

    void Update()
    {
        // InputFieldがアクティブでEnterキーが押されたときの処理
        if (inputField.isFocused && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            // ただし、Shift + Enterの場合は新しい行を挿入
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            {
                OnClickSubmitButton();
            }
        }
        //Debug.Log(LocalGameManager.LocalGameState);
        // UIを表示、非表示する処理
        if(lgm.LocalGameState == LocalGameManager.GameState.Playing)
        {
            ChatUI.SetActive(false);
        }
        else if(lgm.LocalGameState == LocalGameManager.GameState.ChatAndSettings)
        {
            ChatUI.SetActive(true);
        }
    }
    
    public void DestroyChildren(Transform root)
    {
        foreach(Transform child in root.transform)
        {
            Destroy(child.gameObject);
        }   
    }

    // inputFieldの編集終了（Ctrl + Enter or Command + Enter）
    public void OnEditEnd()
    {
        // ctrl + enter   or   command + enter でsubmit

        if(Input.GetKeyDown(KeyCode.Return))
        {
            OnClickSubmitButton();
        }
    }

    public void DisplayChannelTargets()
    {
        DestroyChildren(spawner_channel.transform);

        foreach(ChannelData value in PlayFabData.CurrentRoomChannels.Values)
        {
            // 自分がメンバーに含まれているチャンネルだけ表示
            if(value.MemberIds.Contains(PlayFabSettings.staticPlayer.PlayFabId))
            {
                var obj = Instantiate(button_channelTarget, new Vector3(0f, 0f, 0f), Quaternion.identity);
                obj.name = value.ChannelName;
                obj.transform.SetParent(spawner_channel.transform);
                TMP_Text tx = obj.GetComponentInChildren<TMP_Text>();
                if (value.ChannelType == "Public")
                {
                    tx.text = "# " + value.ChannelName;
                }
                else
                {
                    tx.text = value.ChannelName;
                }
                obj.gameObject.AddComponent<ChannelButton>().channelData = value;
                if (!PlayFabData.DictChannelScripts.ContainsKey(value.ChannelId))
                {
                    PlayFabData.DictChannelScripts.Add(value.ChannelId, obj.GetComponent<ChannelButton>());
                }
            }

            if(!DictReadMessageCount.ContainsKey(value.ChannelId))
            {
                DictReadMessageCount.Add(value.ChannelId, 0); // 0個しか既読していないという意味(すべて未読)
            }
        }
    }

    public void DisplayDMTargets()
    {
        DestroyChildren(spawner_DM.transform);

        foreach(var player in PlayFabData.CurrentRoomPlayers)
        {
            var obj = Instantiate(button_channelTarget, new Vector3(0f, 0f, 0f), Quaternion.identity);
            obj.name = player.Value;
            obj.transform.SetParent(spawner_DM.transform);
            obj.GetComponentInChildren<TMP_Text>().text = player.Value;
            DMButton script = obj.gameObject.AddComponent<DMButton>();
            script.myId = player.Key;
            script.myName = player.Value;

            int result = string.Compare(PlayFabSettings.staticPlayer.PlayFabId, script.myId);
            if(result == 0)
            {
                script.key = script.myId;
            }
            else if(result == -1)
            {
                script.key = PlayFabSettings.staticPlayer.PlayFabId + "+" + script.myId;
            }
            else if(result == 1)
            {
                script.key = script.myId + "+" + PlayFabSettings.staticPlayer.PlayFabId;
            }

            if(!DictReadMessageCount.ContainsKey(script.key))
            {
                DictReadMessageCount.Add(script.key, 0); // 0個しか既読していないという意味(すべて未読)
            }
            PlayFabData.DictDMScripts.Add(player.Key, script);
        }
    }

    // returnボタン
    public void OnClickReturn()
    {
        addMemberToggles = new List<Toggle>();
        panel_createChannel.SetActive(false);
        panel_CHDM.SetActive(true);

        channelName.text = "";
        DestroyChildren(spawner_members.transform);
    }

    // Createボタン
    public void OnClickCreate()
    {
        if(string.IsNullOrEmpty(channelName.text))
        {
            Debug.Log("チャンネル名を入力してください");
            //channelName.color = Color.red;
        }
        else
        {
            List<string> ids = new List<string>(){PlayFabSettings.staticPlayer.PlayFabId}; // 自分のidは先にリストに入れる
            foreach(var toggle in addMemberToggles)
            {
                if(toggle.isOn)
                {
                    ids.Add(toggle.GetComponent<MemberToggle>().myId);
                }
            }

            int rand = Random.Range(0, 10000);
            chatManager.CreatChannel(channelName.text + rand.ToString(), channelName.text, ids, dd_channelType.captionText.text);
        }
    }

    // +ボタン
    public void OnClickCreateChannel()
    {
        panel_CHDM.SetActive(false);
        panel_createChannel.SetActive(true);

        foreach(var member in PlayFabData.CurrentRoomPlayers)
        {
            // 自分以外表示
            if(member.Key != PlayFabSettings.staticPlayer.PlayFabId)
            {
                var obj = Instantiate(toggle_member, new Vector3(0f, 0f, 0f), Quaternion.identity);
                obj.name = member.Value;
                obj.transform.SetParent(spawner_members.transform);
                MemberToggle script = obj.gameObject.AddComponent<MemberToggle>();
                script.myName = member.Value;
                script.myId = member.Key;
                obj.GetComponentInChildren<Text>().text = member.Value;

                addMemberToggles.Add(obj);
            }
        }
    }

    // 全員をonにする
    public void OnClickAddAllMembers()
    {
        foreach(var toggle in addMemberToggles)
        {
            toggle.isOn = true;
        }
    }

    // channelType
    public void OnValueChangedChannelType(TMP_Dropdown dd)
    {
        Debug.Log("valueChanged: " + dd.value);
        if(dd.value == 0) // Private
        {
            panel_Members.SetActive(true);
        }
        else if(dd.value == 1) // Public
        {
            OnClickAddAllMembers();
            panel_Members.SetActive(false);
        }
    }
    
    public void UpdateChannelMessageData(string key, MessageData messageData)
    {
        List<MessageData> datas = new List<MessageData>();

        if(messageData.ChannelId == "DM")
        {
            datas = PlayFabData.DictDMScripts[messageData.ReceiverId].messageDatas; // 送信者のみがこのメソッドを読んでいるのでキーは受信者のID
        }
        else if (messageData.ReceiverId == "All")
        {
            datas = PlayFabData.DictChannelScripts[messageData.ChannelId].messageDatas;
        }
        
        if(!string.IsNullOrEmpty(key) && datas.Count != 0)
        {
            string jsonData = JsonConvert.SerializeObject(datas);
            var request = new UpdateSharedGroupDataRequest
            {
                SharedGroupId = PlayFabData.CurrentSharedGroupId,
                Data = new Dictionary<string, string> { { key,  jsonData} },
                Permission = UserDataPermission.Public
            };
            PlayFabClientAPI.UpdateSharedGroupData(request, 
                _ => 
                {
                    Debug.Log("共有グループデータの変更成功"); 
                    //UpdateUserData();
                }, 
                _ => Debug.Log("共有グループデータの変更失敗")
            );
        }
    }

    public void UpdateUserData()
    {   
        string jsonData = JsonConvert.SerializeObject(DictReadMessageCount);
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>(){ {"DictReadMessageCount", jsonData} }
        };
        PlayFabClientAPI.UpdateUserData(request, _ => Debug.Log("DictReadMessageCount更新成功"), e => e.GenerateErrorReport());
    }

    public void OnClickSubmitButton()
    {
        if(!string.IsNullOrEmpty(inputField.text))
        {
            string text = inputField.text;
            string receiverId = PlayFabData.CurrentMessageTarget;

            inputField.text = "";
            scrollRect.verticalNormalizedPosition = 0; // scrollviewを一番下にする

            PlayFabClientAPI.GetTime(new GetTimeRequest(), 
                result =>
                {
                    chatManager.chatSender.RPC_SendMessageRequest(PlayFabSettings.staticPlayer.PlayFabId, receiverId, PlayFabData.CurrentChannelId, text, result.Time.AddHours(9d).ToString());
                },
                _ => Debug.Log("時間取得失敗")
            );
        }
    }

    public void DisplayMessage(MessageData messageData)
    {
        var obj = Instantiate(text_messagePref, new Vector3(0f, 0f, 0f), Quaternion.identity);
        obj.transform.SetParent(spawner_message.transform);
        obj.GetComponent<TMP_Text>().text = PlayFabData.CurrentRoomPlayers[messageData.SenderId] + ": " + messageData.Content;
        csf.SetLayoutVertical(); // 高さ調整を無理やりさせる、スクロールバーを一番下に下げるために先に変更させておく
        Invoke("MoveToBottom", 0.1f); // contentのcsfが高さ計算をするのに時間がかかるため少し待ってから一番下にする
    }


    // スクロールバーを一番下まで移動させる
    public void MoveToBottom()
    {
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
