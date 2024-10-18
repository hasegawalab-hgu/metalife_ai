using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class LocalGameManager : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        ChatAndSettings,
    }

    public GameState LocalGameState = GameState.ChatAndSettings;

    private ChatUIManager chatUIManager;

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
}
