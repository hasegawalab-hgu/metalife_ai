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

public class ChatUIManager : MonoBehaviour
{
    private ChatManager chatManager;

    // chat画面のUI
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
        chatManager = GetComponent<ChatManager>();
        text_channelName.text = "# " + PlayFabData.CurrentRoomChannels[PlayFabData.CurrentChannelId].ChannelName; // generalなので#をつける
        DisplayChannelTargets();
        DisplayDMTargets();
    }
    
    public void DestroyChildren(Transform root)
    {
        foreach(Transform child in root.transform)
        {
            Destroy(child.gameObject);
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
                PlayFabData.DictChannelScripts.Add(value.ChannelId, obj.GetComponent<ChannelButton>());
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

            PlayFabData.DictDMScripts.Add(player.Key, script);
        }
    }

    public void OnClickSubmit()
    {

    }

    public void OnClickReturn()
    {
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

    /*
    public List<ChannelData> GetChannelMessageData(string channelId)
    {
        var request = new GetSharedGroupDataRequest
        {
            SharedGroupId = PlayFabData.CurrentSharedGroupId,
            Keys = new List<string>(){channelId}
        };
        PlayFabClientAPI.GetSharedGroupData(request, 
            result => 
            {
                return JsonConvert.DeserializeObject<List<ChannelData>>(result.Data[channelId].Value);
            },
            e => 
        );
        return new List<ChannelData>();
    }
    */
    
    public void UpdateChannelMessageData(MessageData messageData)
    {
        List<MessageData> Datas = PlayFabData.DictChannelScripts[messageData.ChannelId].messageDatas;
        Datas.Add(messageData);
        string jsonData = JsonConvert.SerializeObject(Datas);
        var request = new UpdateSharedGroupDataRequest
        {
            SharedGroupId = PlayFabData.CurrentSharedGroupId,
            Data = new Dictionary<string, string> { { messageData.ChannelId,  jsonData} },
            Permission = UserDataPermission.Public
        };
        PlayFabClientAPI.UpdateSharedGroupData(request, _ => Debug.Log("共有グループデータの変更成功"), _ => Debug.Log("共有グループデータの変更失敗"));
    }

    public void OnClickSubmitButton()
    {
        if(!string.IsNullOrEmpty(inputField.text))
        {
            string receiverId = PlayFabData.CurrentMessageTarget;

            PlayFabClientAPI.GetTime(new GetTimeRequest(), 
                result =>
                {
                    chatManager.chatSender.RPC_SendMessageRequest(PlayFabSettings.staticPlayer.PlayFabId, receiverId, PlayFabData.CurrentChannelId, inputField.text, result.Time.AddHours(9d).ToString());
                    inputField.text = "";
                },
                _ => Debug.Log("時間取得失敗")
            );
        }
    }

    public void DisplayMessage(MessageData messageData)
    {
        var obj = Instantiate(text_messagePref, new Vector3(0f, 0f, 0f), Quaternion.identity);
        obj.transform.SetParent(spawner_message.transform);
        obj.GetComponent<TMP_Text>().text = messageData.Content;
    }
}
