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
    public string beforeSendText;

    public int UnReadMessageCount = 0;

    public string key;

    public List<MessageData> messageDatas = new List<MessageData>();
    private TMP_Text unReadText;

    void Start()
    {
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
        unReadText = GetComponentsInChildren<TMP_Text>()[1];
        GetComponent<Button>().onClick.AddListener(OnClickButton);
        GetSharedGroupData(true);

        UnReadMessageCount = messageDatas.Count - chatUIManager.DictReadMessageCount[key];
    }

    void Update()
    {
        unReadText.text = UnReadMessageCount.ToString();
    }

    void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(OnClickButton);
    }

    public void OnClickButton()
    {
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
        
        if(!string.IsNullOrEmpty(beforeSendText))
        {
            chatUIManager.inputField.text = beforeSendText;
        }
        
        PlayFabData.CurrentChannelId = "DM";
        PlayFabData.CurrentMessageTarget = myId;
        chatUIManager.text_channelName.text = "DM : " + myName;

        chatUIManager.DestroyChildren(chatUIManager.spawner_message.transform);
        foreach (var messageData in messageDatas)
        {
            chatUIManager.DisplayMessage(messageData);
        }

        chatUIManager.DictReadMessageCount[key] = messageDatas.Count;
        UnReadMessageCount = 0;
        UpdateUserData();
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
