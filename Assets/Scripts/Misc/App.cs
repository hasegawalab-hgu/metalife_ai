using System;
using CustomConnectionHandler;
using Fusion;
using Interface;
using UnityEngine;

/// <summary>
/// The main purpose of this class is to perform networked operations that the ConnectionManager.cs can't because it's a monobehaviour.
/// </summary>
public class App : NetworkBehaviour
{
    public override void Spawned()
    {
        // Avoid destroying on loading because dungeons doesn't load the scene right away.
        // This object will be automatically destroyed when the runner shutdown.
        Debug.Log("spauwnd");
        DontDestroyOnLoad(this);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ShutdownRunner(ConnectionData.ConnectionTarget target)
    {
        //InterfaceManager.Instance.ClearInterface();
        var connection = target == ConnectionData.ConnectionTarget.Lobby ? ConnectionManager.Instance.GetLobbyConnection() : ConnectionManager.Instance.GetDungeonConnection();
        if (connection.IsRunning)
            connection.Runner.Shutdown();
    }

    
    public static implicit operator App(NetworkObject v)
    {
        throw new NotImplementedException();
    }
    
}
