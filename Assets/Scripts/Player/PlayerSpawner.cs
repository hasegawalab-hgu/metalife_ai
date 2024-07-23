using System.Diagnostics.Tracing;
using Fusion;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public NetworkPrefabRef PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            if(PlayerPrefab == default(NetworkPrefabRef)) // PlayerPrefabは何も設定されていない場合、nullにならない
            {
                PlayerPrefab = GameObject.Find("PlayerPrefabs").GetComponent<PlayerPrefabs>().playerPrefabs[0];
            }
            var localPlayer = Runner.Spawn(prefabRef:PlayerPrefab, new Vector3(0, 0, 0), inputAuthority: this.Runner.LocalPlayer);
            localPlayer.GetComponent<PlayerData>().PlayFabId = PlayFabSettings.staticPlayer.PlayFabId;
            localPlayer.name = "LocalPlayer";
            GameObject.Find("Main Camera").transform.SetParent(localPlayer.transform);
            Debug.Log(localPlayer.GetComponent<PlayerData>().PlayFabId);
        }
        else
        {

        }
    }
}
