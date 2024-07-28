using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
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

    void Start()
    {
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
        ls = GetComponent<LoadSprites>();
        ls.LoadAllImages(ls.Paths[ls.index]);
    }

    public void OnClickDecideButton()
    {
        PlayFabData.MyTexture = selectedTexture;
        PlayFabData.MyTexturePath = ls.Paths[ls.index] + selectedTexture.name;
        Debug.Log(PlayFabData.MyTexturePath);
        if(PlayFabData.CurrentRoomPlayersRefs.ContainsKey(PlayFabSettings.staticPlayer.PlayFabId))
        {
            PlayFabData.CurrentRoomPlayersRefs[PlayFabSettings.staticPlayer.PlayFabId].LoadTexture();
            PlayFabData.CurrentRoomPlayersRefs[PlayFabSettings.staticPlayer.PlayFabId].RPC_Texture2SpriteRequest();
        }
        UpdateUserData();

        chatUIManager.OnClickCharacterButton();
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
