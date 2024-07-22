using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerPrefabs : NetworkBehaviour
{
    public NetworkPrefabRef[] playerPrefabs;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
