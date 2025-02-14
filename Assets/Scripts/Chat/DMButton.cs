using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using UnityEditor;
using Newtonsoft.Json;
using TMPro;

public class DMButton : MonoBehaviour
{
    public string myName;
    public string myId;

    public GameObject playerInstance;

    private ChatUIManager chatUIManager;
    private LocalGameManager lgm;
    public string beforeSendText;
    public PlayerData pd;

    public int UnReadMessageCount = 0;

    public string key;
    private Image outline;
    private Image image;
    public List<MessageData> messageDatas = new List<MessageData>();
    private TMP_Text text;
    private TMP_Text unReadText;
    private Color initialColorOutline;
    private Color initialColorText;
    

    void Start()
    {
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
        lgm = GameObject.Find("LocalGameManager").GetComponent<LocalGameManager>();
        text = GetComponentsInChildren<TMP_Text>()[0];
        unReadText = GetComponentsInChildren<TMP_Text>()[1];
        outline = GetComponentsInChildren<Image>()[0];
        image = GetComponentsInChildren<Image>()[1];
        GetComponent<Button>().onClick.AddListener(OnClickButton);
        GetSharedGroupData(true);
        initialColorOutline = outline.color;
        initialColorText = text.color;
    }

    void Update()
    {
        if(pd == null)
        {
            // Debug.Log("null");
            /*
            if(playerInstance != null)
            {
                pd = playerInstance.GetComponent<PlayerData>();
            }
            else
            {
                if(PlayFabData.CurrentRoomPlayersRefs.ContainsKey(myId))
                {
                    playerInstance = GameObject.Find(myId);
                }
                pd = playerInstance.GetComponent<PlayerData>();
            }
            */
        }
        else
        {
            if(pd.IsOnline)
            {
                // text.color = Color.green;
                //outline.color = Color.green;
                if(text.color != Color.green)
                {
                    // text.color = Color.green;
                }
            }
            else
            {
                if(text.color != initialColorText)
                {
                    text.color = initialColorText;
                }
                //outline.color = initialColorOutline;
            }
        }

        if(UnReadMessageCount <= 0)
        {
            image.gameObject.SetActive(false);
        }
        else
        {
            image.gameObject.SetActive(true);
            unReadText.text = UnReadMessageCount.ToString();
        }
    }

    void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(OnClickButton);
    }

    public void OnClickButton()
    {
        if(chatUIManager == null)
        {
            return;
        }
        //Debug.Log("onclick! " + this.myName);
        
        chatUIManager.DisplayedMessageCount = 0;
        chatUIManager.isDisplayedUnReadMessage = false;

        /*
        if(PlayFabData.CurrentChannelId == "DM")
        {
            PlayFabData.DictDMScripts[myId].beforeSendText = chatUIManager.inputField.text;
            chatUIManager.inputField.text = "";
        }
        else if(PlayFabData.CurrentMessageTarget == "All")
        {
            PlayFabData.DictChannelScripts[PlayFabData.CurrentChannelId].beforeSendText = chatUIManager.inputField.text;
            chatUIManager.inputField.text = "";
        }
        
        if(!string.IsNullOrEmpty(beforeSendText) & lgm.LocalGameState == LocalGameManager.GameState.ChatAndSettings)
        {
            chatUIManager.inputField.text = beforeSendText;
        }
        */

        PlayFabData.CurrentChannelId = "DM";
        PlayFabData.CurrentMessageTarget = myId;
        chatUIManager.text_channelName.text = "DM : " + myName;

        if(chatUIManager.spawner_message != null)
        {
            chatUIManager.DestroyChildren(chatUIManager.spawner_message.transform);
        }
        foreach (var messageData in messageDatas)
        {
            chatUIManager.DisplayMessage(messageData);
        }

        if(lgm.LocalGameState == LocalGameManager.GameState.ChatAndSettings)
        {
            chatUIManager.DictReadMessageCount[key] = messageDatas.Count;

            if(UnReadMessageCount != 0)
            {
                UpdateUserData();
            }
            UnReadMessageCount = 0;
        }
    }

    private void UpdateUserData()
    {   
        string jsonData = JsonConvert.SerializeObject(chatUIManager.DictReadMessageCount);
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>(){ {"DictReadMessageCount", jsonData} }
        };
        PlayFabClientAPI.UpdateUserData(request, _ => Debug.Log("DictReadMessageCount更新成功"), e => e.GenerateErrorReport());
    }

    private void GetSharedGroupData(bool calledByStart)
    {
        var request = new GetSharedGroupDataRequest
        {
            SharedGroupId = PlayFabData.CurrentSharedGroupId,
        };

        PlayFabClientAPI.GetSharedGroupData(request, 
            result => 
            {
                if (!string.IsNullOrEmpty(key) & result.Data.ContainsKey(key))
                {
                    messageDatas = JsonConvert.DeserializeObject<List<MessageData>>(result.Data[key].Value);
                    UnReadMessageCount = messageDatas.Count - chatUIManager.DictReadMessageCount[key];

                    if(PlayFabData.CurrentMessageTarget == myId && calledByStart)
                    {
                        OnClickButton();
                    }
                }
            }, 
            e => e.GenerateErrorReport()
        );
    }
}
