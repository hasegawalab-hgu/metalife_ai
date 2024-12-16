using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LocalGameManager : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        ChatAndSettings,
    }

    public GameState LocalGameState = GameState.ChatAndSettings;

    private ChatUIManager chatUIManager;

    private void Awake()
    {
        GetPCPos();
    }

    private void Start()
    {
        Invoke("Initialize", 0.01f);
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(LocalGameState == GameState.Playing)
            {
                LocalGameState = GameState.ChatAndSettings;
                chatUIManager.Invoke("MoveToBottom", 0.05f);
            }
            else if(LocalGameState == GameState.ChatAndSettings)
            {
                LocalGameState = GameState.Playing;
            }
        }
    }

    void Initialize()
    {
        LocalGameState = GameState.Playing;
    }

    private void SetChairPos()
    {
        PlayFabData.ChairPos.Add(new Vector3(19f, 28f, 0f));
        PlayFabData.ChairPos.Add(new Vector3(15f, 28f, 0f));
        PlayFabData.ChairPos.Add(new Vector3(11f, 28f, 0f));
        PlayFabData.ChairPos.Add(new Vector3(7f, 28f, 0f));
        PlayFabData.ChairPos.Add(new Vector3(3f, 28f, 0f));
        PlayFabData.ChairPos.Add(new Vector3(19f, 24f, 0f));
        PlayFabData.ChairPos.Add(new Vector3(15f, 24f, 0f));
        PlayFabData.ChairPos.Add(new Vector3(11f, 24f, 0f));
        PlayFabData.ChairPos.Add(new Vector3(7f, 24f, 0f));
        PlayFabData.ChairPos.Add(new Vector3(3f, 24f, 0f));
    }

    public void GetPCPos()
    {
        var tilemap = GameObject.Find("ChairTilemap").GetComponent<Tilemap>();
        var bound = tilemap.cellBounds;

    for (int y = bound.min.y; y < bound.max.y; y++)
    {
        for (int x = bound.min.x; x < bound.max.x; x++)
        {
            var tile = tilemap.GetTile<Tile>(new Vector3Int(x, y, 0));
            if(tile != null && x > -2)
            {
                Debug.Log(x + " " + y);
                PlayFabData.ChairPos.Add(new Vector3(x, y + 1f, 0f));
            }
        }
    }
    }
}
