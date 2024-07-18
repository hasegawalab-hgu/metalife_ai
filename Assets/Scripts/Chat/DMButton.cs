using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DMButton : MonoBehaviour
{
    public string myName;
    public string myId;

    public GameObject playerInstance;

    private ChatUIManager chatUIManager;

    private List<MessageData> masageDatas = new List<MessageData>();

    void Start()
    {
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
        GetComponent<Button>().onClick.AddListener(OnClickButton);
    }

    public void OnClickButton()
    {
        Debug.Log("onclick");
        PlayFabData.CurrentChannelId = null;
        chatUIManager.text_channelName.text = "DM : " + myName;
    }
}
