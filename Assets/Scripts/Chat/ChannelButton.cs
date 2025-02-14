using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using TMPro;

public class ChannelButton : MonoBehaviour
{
    public ChannelData channelData; // 生成される時に割り当てられる
    public string myName;
    public string myId;

    public GameObject playerInstance;

    private ChatUIManager chatUIManager;
    private LocalGameManager lgm;
    public string beforeSendText;

    public int UnReadMessageCount = 0;

    public string key;

    public List<MessageData> messageDatas = new List<MessageData>();
    private Image image;
    private TMP_Text unReadText;


    void Start()
    {
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
        lgm = GameObject.Find("LocalGameManager").GetComponent<LocalGameManager>();
        unReadText = GetComponentsInChildren<TMP_Text>()[1];
        image = GetComponentsInChildren<Image>()[1];
        GetComponent<Button>().onClick.AddListener(OnClickButton);
        GetSharedGroupData(true);
    }

    void Update()
    {
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
        chatUIManager.DisplayedMessageCount = 0;
        chatUIManager.isDisplayedUnReadMessage = false;

        /*
        if(PlayFabData.CurrentChannelId == "DM")
        {
            PlayFabData.DictDMScripts[PlayFabData.CurrentMessageTarget].beforeSendText = chatUIManager.inputField.text;
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
        if(PlayFabData.CurrentRoomChannels.ContainsKey(channelData.ChannelId))
        {
            if (PlayFabData.CurrentRoomChannels[channelData.ChannelId].ChannelType == "Public")
            {
                chatUIManager.text_channelName.text = "# " + channelData.ChannelName;
            }
            else
            {
                chatUIManager.text_channelName.text = channelData.ChannelName;
            }
        }
        PlayFabData.CurrentChannelId = channelData.ChannelId;
        PlayFabData.CurrentMessageTarget = "All";

        chatUIManager.DestroyChildren(chatUIManager.spawner_message.transform);
        
        foreach(MessageData messagedata in messageDatas)
        {
            chatUIManager.DisplayMessage(messagedata);
            chatUIManager.scrollRect.verticalNormalizedPosition = 0f; // スクロールバーを一番下まで下げる
        }
        if(lgm.LocalGameState == LocalGameManager.GameState.ChatAndSettings)
        {
            chatUIManager.DictReadMessageCount[channelData.ChannelId] = messageDatas.Count;
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
                if (result.Data.ContainsKey(channelData.ChannelId))
                {
                    messageDatas = JsonConvert.DeserializeObject<List<MessageData>>(result.Data[channelData.ChannelId].Value);
                    UnReadMessageCount = messageDatas.Count - chatUIManager.DictReadMessageCount[channelData.ChannelId];

                    if(PlayFabData.CurrentChannelId == channelData.ChannelId && calledByStart)
                    {
                        OnClickButton();
                        // chatUIManager.scrollRect.verticalNormalizedPosition = 0; // scrollviewを一番下にする
                    }
                }
            }, 
            e => e.GenerateErrorReport()
        );
    }
}
