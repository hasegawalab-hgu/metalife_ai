using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    private LoadSprites ls;
    public Texture2D selectedTexture;
    public Image selectedImage;
    private ChatUIManager chatUIManager;
    private PlayerData localPlayerData;

    void Start()
    {
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
        ls = GetComponent<LoadSprites>();
        ls.LoadAllImages(ls.Paths[ls.index]);
    }

    public void OnClickDecideButton()
    {
        if(localPlayerData == null)
        {
            localPlayerData = GameObject.Find("LocalPlayer").GetComponent<PlayerData>();
        }

        PlayFabData.MyTexture = selectedTexture;
        PlayFabData.MyTexturePath = ls.Paths[ls.index] + selectedTexture.name;
        PlayFabData.DictPlayerInfos[PlayFabSettings.staticPlayer.PlayFabId].texturePath = ls.Paths[ls.index] + selectedTexture.name;

        if(PlayFabData.CurrentRoomPlayersRefs.ContainsKey(PlayFabSettings.staticPlayer.PlayFabId))
        {
            localPlayerData.LoadTexture();
            localPlayerData.RPC_Texture2SpriteRequest();
        }
        // UpdateUserData();
        UpdatePlayerInfos();
        chatUIManager.OnClickCharacterButton();
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

    private void UpdateUserData()
    {
        if(PlayFabData.MyTexturePath == null)
        {
            return;
        }

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>(){{"CharacterPath", PlayFabData.MyTexturePath},}
            
        };
        PlayFabClientAPI.UpdateUserData(request, _ => Debug.Log("pathの更新成功"), _ => Debug.Log("pathの更新失敗"));
    }

    public void OnClickLeftButton()
    {
        if(ls.index == 0)
        {
            ls.index = ls.Paths.Count - 1;
        }
        else
        {
            ls.index--;
        }
        ls.LoadAllImages(ls.Paths[ls.index]);
    }

    public void OnClickRightButton()
    {
        if(ls.index == ls.Paths.Count - 1)
        {
            ls.index = 0;
        }
        else
        {
            ls.index++;
        }
        ls.LoadAllImages(ls.Paths[ls.index]);
    }
}
