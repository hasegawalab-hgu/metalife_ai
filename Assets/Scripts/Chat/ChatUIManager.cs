using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;
using Newtonsoft.Json;
using UnityEngine.EventSystems;

public class ChatUIManager : MonoBehaviour
{
    private ChatManager chatManager;
    private LocalGameManager lgm;
    public Dictionary<string, int> DictReadMessageCount = new Dictionary<string, int>();
    public int DisplayedMessageCount = 0;
    public bool isDisplayedUnReadMessage = false; // 「ここから未読メッセージ」を表示したかどうか

    public GameObject Canvas;

    // chat画面のUI
    [SerializeField]
    private GameObject ChatAndSettingUI;
    [SerializeField]
    private GameObject ChatUI;
    [SerializeField]
    private GameObject SelectCharacterUI;
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
    public GameObject spawner_message; // 一覧
    [SerializeField]
    public GameObject spawner_simple_message; // 簡易チャット
    [SerializeField]
    private TMP_Text text_messagePref; // prefab 一覧
    [SerializeField]
    public TMP_Text text_simple_messagePref; // prefab 簡易チャット
    [SerializeField]
    private TMP_Text text_senderPref; // prefab
    [SerializeField]
    private TMP_Text text_timestampPref; // prefab
    [SerializeField]
    private TMP_Text text_UnReadPref; // prefab 文字記入済み
    [SerializeField]
    public TMP_Text text_channelName;
    [SerializeField]
    public ScrollRect scrollRect; // scrollview
    [SerializeField]
    public ContentSizeFitter csf; // contentのcontentsizefilter
    [SerializeField]
    public Button button_selectCharacter;
    [SerializeField]
    public TMP_Text text_targets;

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

    void Awake()
    {
        DictReadMessageCount = PlayFabData.DictReadMessageCount;
        chatManager = GetComponent<ChatManager>();
        lgm = GameObject.Find("LocalGameManager").GetComponent<LocalGameManager>();
        csf = spawner_message.GetComponent<ContentSizeFitter>();
        inputField.onValidateInput += ValidateInput;
        text_channelName.text = "# general"; // generalなので#をつける
        DisplayChannelTargets();
        DisplayDMTargets();
    }

    void Update()
    {
        if(lgm.LocalGameState == LocalGameManager.GameState.Playing)
        {
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
            }
            CheckPressEnter();
        }

        //Debug.Log(LocalGameManager.LocalGameState);
        // UIを表示、非表示する処理
        if(lgm.LocalGameState == LocalGameManager.GameState.Playing)
        {
            ChatAndSettingUI.SetActive(false);
        }
        else if(lgm.LocalGameState == LocalGameManager.GameState.ChatAndSettings)
        {
            ChatAndSettingUI.SetActive(true);
        }
    }

    private void CheckPressEnter()
    {
        //  IME日本語入力変換を使用しいるかどうかの判断
        if(!(inputField.text.EndsWith("\n") || inputField.text.EndsWith("\r\n")))
        {
            return;
        }

        if((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && (string.IsNullOrEmpty(inputField.text) || inputField.text.Trim() == ""))
        {
            inputField.DeactivateInputField();
            EventSystem.current.SetSelectedGameObject(null);
            inputField.text = "";
            return;
        }

        // InputFieldがアクティブでEnterキーが押されたときの処理
        if (inputField.isFocused && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            // Shift + Enterの場合は新しい行を挿入、IMEがオンの時は送信しない
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            {
                OnClickSubmitButton();
            }
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
            // OnClickSubmitButton();
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

        foreach(var player in PlayFabData.DictPlayerInfos.Values)
        {
            var obj = Instantiate(button_channelTarget, new Vector3(0f, 0f, 0f), Quaternion.identity);
            obj.name = player.name;
            obj.transform.SetParent(spawner_DM.transform);
            obj.GetComponentInChildren<TMP_Text>().text = player.name;
            DMButton script = obj.gameObject.AddComponent<DMButton>();
            script.myId = player.id;
            script.myName = player.name;

            int result = string.Compare(PlayFabSettings.staticPlayer.PlayFabId, script.myId);
            script.key = result == -1 ? PlayFabSettings.staticPlayer.PlayFabId + "+" + script.myId : result == 1 ? script.myId + "+" + PlayFabSettings.staticPlayer.PlayFabId : script.myId;

            if(!DictReadMessageCount.ContainsKey(script.key))
            {
                DictReadMessageCount.Add(script.key, 0); // 0個しか既読していないという意味(すべて未読)
            }
            if(!PlayFabData.DictDMScripts.ContainsKey(player.id))
            {
                PlayFabData.DictDMScripts.Add(player.id, script);
            }
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

        foreach(var player in PlayFabData.DictPlayerInfos.Values)
        {
            // 自分以外表示
            if(player.id != PlayFabSettings.staticPlayer.PlayFabId)
            {
                var obj = Instantiate(toggle_member, new Vector3(0f, 0f, 0f), Quaternion.identity);
                obj.name = player.name;
                obj.transform.SetParent(spawner_members.transform);
                MemberToggle script = obj.gameObject.AddComponent<MemberToggle>();
                script.myName = player.name;
                script.myId = player.id;
                obj.GetComponentInChildren<Text>().text = player.name;

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

    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        // エスケープキーが押されたら、その入力を無視する
        if (addedChar == '\u001B') // '\u001B'はESCキー
        {
            return '\0'; // 入力無効にするため、ヌル文字を返す
        }

        // 他の入力はそのまま受け付ける
        return addedChar;
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
                    chatManager.chatSender.RPC_SendMessageRequest(PlayFabSettings.staticPlayer.PlayFabId, receiverId, PlayFabData.CurrentChannelId, text, result.Time.AddHours(9d).ToString("yyyy/MM/dd HH:mm:ss:ffff"));
                },
                _ => Debug.Log("時間取得失敗")
            );
        }
    }

    public void DisplayMessage(MessageData messageData)
    {
        List<MessageData> messageDatas = new List<MessageData>();
        int readMessageCount = 0;
        if(messageData.ChannelId == "DM") // DM
        {
            int result = string.Compare(messageData.SenderId, messageData.ReceiverId);
            string DMScriptsKey = messageData.SenderId == PlayFabSettings.staticPlayer.PlayFabId ? messageData.ReceiverId : messageData.SenderId;
            string readMessageKey = result == -1 ? messageData.SenderId + "+" + messageData.ReceiverId : result == 1 ? messageData.ReceiverId + "+" + messageData.SenderId : messageData.SenderId;
            messageDatas = PlayFabData.DictDMScripts[DMScriptsKey].messageDatas;

            if(PlayFabData.DictReadMessageCount.ContainsKey(readMessageKey))
            {
                readMessageCount = PlayFabData.DictReadMessageCount[readMessageKey];
            }
        }
        else // Channel
        {
            messageDatas = PlayFabData.DictChannelScripts[messageData.ChannelId].messageDatas;

            readMessageCount = PlayFabData.DictReadMessageCount[messageData.ChannelId];
        }

        // ここから未読メーセージの表示
        
        // Debug.Log(DisplayedMessageCount + " ........... " + readMessageCount);
        
        if(messageData.SenderId != PlayFabSettings.staticPlayer.PlayFabId & DisplayedMessageCount == readMessageCount & isDisplayedUnReadMessage == false)
        {
            var unReadObj = Instantiate(text_UnReadPref, new Vector3(0f, 0f, 0f), Quaternion.identity);
            unReadObj.transform.SetParent(spawner_message.transform);
            isDisplayedUnReadMessage = true;
        }
        

        // 前のメッセージと日付が違うなら日付を表示
        if(DisplayedMessageCount == 0 || messageDatas[DisplayedMessageCount - 1].Timestamp.Substring(0,10) != messageData.Timestamp.Substring(0,10))
        {
            var timestampObj = Instantiate(text_timestampPref ,new Vector3(0f, 0f, 0f), Quaternion.identity);
            timestampObj.transform.SetParent(spawner_message.transform);
            timestampObj.GetComponent<TMP_Text>().text = messageData.Timestamp.Substring(0,10);
        }

        // 前のメッセージと送信者が違うなら送信者を表示
        if(DisplayedMessageCount == 0 || messageDatas[DisplayedMessageCount - 1].SenderId != messageData.SenderId)
        {
            var senderObj = Instantiate(text_senderPref, new Vector3(0f, 0f, 0f), Quaternion.identity);
            senderObj.transform.SetParent(spawner_message.transform);
            if(PlayFabData.DictPlayerInfos.ContainsKey(messageData.SenderId))
            {
                senderObj.GetComponent<TMP_Text>().text = PlayFabData.DictPlayerInfos[messageData.SenderId].name;
            }
            else
            {
                senderObj.GetComponent<TMP_Text>().text = "新規プレイヤー";
            }
        }

        var obj = Instantiate(text_messagePref, new Vector3(0f, 0f, 0f), Quaternion.identity);
        obj.transform.SetParent(spawner_message.transform);
        obj.GetComponent<TMP_Text>().text = messageData.Content;
        obj.GetComponentsInChildren<TMP_Text>()[1].text = messageData.Timestamp.Substring(0,19);

        csf.SetLayoutVertical(); // 高さ調整を無理やりさせる、スクロールバーを一番下に下げるために先に変更させておく
        Invoke("MoveToBottom", 0.05f); // contentのcsfが高さ計算をするのに時間がかかるため少し待ってから一番下にする

        DisplayedMessageCount++;
    }

    // 簡易チャット
    public void DisplaySimpleMessage(MessageData messageData)
    {
        var smObj = Instantiate(text_simple_messagePref, new Vector3(0f, 0f, 0f), Quaternion.identity);
        smObj.transform.SetParent(PlayFabData.DictSpawnerSimpleMessage[messageData.SenderId].transform);
        smObj.text = messageData.Content;

        StartCoroutine(DeleteSimpleMessage(7.0f, smObj.gameObject));
    }

    IEnumerator DeleteSimpleMessage(float delay, GameObject messageObj)
    {
        yield return new WaitForSeconds(delay);
        Destroy(messageObj.gameObject);
    }


    // スクロールバーを一番下まで移動させる
    public void MoveToBottom()
    {
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void OnClickCharacterButton()
    {
        TMP_Text button_text = button_selectCharacter.GetComponentInChildren<TMP_Text>();
        if(button_text.text == "Return")
        {
            ChatUI.SetActive(true);
            SelectCharacterUI.SetActive(false);
            button_text.text = "Character";
        }
        else
        {
            ChatUI.SetActive(false);
            SelectCharacterUI.SetActive(true);
            button_text.text = "Return";
        }
    }
}
