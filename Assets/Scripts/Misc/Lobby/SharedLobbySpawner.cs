using UnityEngine;
using Fusion;

public class SharedLobbySpawner : NetworkBehaviour {
    
    [SerializeField] private NetworkPrefabRef _character;

    public override void Spawned()
    {
        this.Runner.Spawn(_character, Vector3.zero, inputAuthority: this.Runner.LocalPlayer);
    }
}
