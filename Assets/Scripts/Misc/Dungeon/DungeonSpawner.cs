using System;
using System.Collections.Generic;
using CustomConnectionHandler;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef _character;

    private Dictionary<PlayerRef, NetworkObject> _playerAvatars;

    public override void Spawned()
    {
        if (Object.HasStateAuthority == false) return;
        _playerAvatars = new Dictionary<PlayerRef, NetworkObject>();
        
        Vector3 pos = Vector3.zero;
        foreach (var playerRef in Runner.ActivePlayers)
        {
            pos = Random.insideUnitSphere * 3;
            pos.y = 2;
            _playerAvatars.Add(playerRef, Runner.Spawn(_character, pos, inputAuthority: playerRef));
        }
        
        OnEnable();
    }

    private void DespawnPlayerCharacter(NetworkRunner runner, PlayerRef player)
    {
        runner.Despawn(_playerAvatars[player]);
        _playerAvatars.Remove(player);
    }

    private void OnEnable()
    {
        if (Object == false || Object.HasStateAuthority == false) return;
        ConnectionManager.Instance.GetDungeonConnection().Callback.ActionOnPlayerLeft += DespawnPlayerCharacter;
    }

    private void OnDisable()
    {        
        if (Object == false || Object.HasStateAuthority == false) return;
        ConnectionManager.Instance.GetDungeonConnection().Callback.ActionOnPlayerLeft -= DespawnPlayerCharacter;
    }
}
