using System.Diagnostics.Tracing;
using Fusion;
using UnityEngine;

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
            localPlayer.GetComponent<PlayerData>().PlayFabId = GameObject.Find("PlayFabId").GetComponent<PlayFabId>().playFabId;
            Debug.Log(localPlayer.GetComponent<PlayerData>().PlayFabId);
        }
    }
}
