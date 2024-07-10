using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using System;
using PlayFab.EconomyModels;


public class PlayFabChatManager : MonoBehaviour
{
    GameObject localPlayer;
    PlayerData pd;

    string sharedGroupId = "hasegawa_lab_2024";
    void Start()
    {
        localPlayer = GameObject.Find("LocalPlayer");
        //pd = localPlayer.GetComponent<PlayerData>();
    }
    public void SaveMessageToUserData()
    {

    }
}
