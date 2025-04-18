using UnityEngine;
using Fusion;
using PlayFab.ClientModels;

public class SharedLobbySpawner : NetworkBehaviour {
    
    [SerializeField] private NetworkPrefabRef _character;

    public override void Spawned()
    {
        NetworkObject localPlayer = this.Runner.Spawn(_character, Vector3.zero, inputAuthority: this.Runner.LocalPlayer);
    }
}
