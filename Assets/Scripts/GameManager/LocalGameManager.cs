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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(LocalGameState == GameState.Playing)
            {
                LocalGameState = GameState.ChatAndSettings;
                Debug.Log(LocalGameState);
            }
            else if(LocalGameState == GameState.ChatAndSettings)
            {
                LocalGameState = GameState.Playing;
            }
        }
    }
}
