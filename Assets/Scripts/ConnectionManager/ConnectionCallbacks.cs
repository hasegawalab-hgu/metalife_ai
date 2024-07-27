using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace CustomConnectionHandler {
    public class ConnectionCallbacks : INetworkRunnerCallbacks {
        public Action<NetworkRunner, PlayerRef> ActionOnPlayerJoined;
        public Action<NetworkRunner, PlayerRef> ActionOnPlayerLeft;
        public Action<NetworkRunner, ShutdownReason> ActionOnShutdown;

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"{player} Joined");
            ActionOnPlayerJoined?.Invoke(runner, player);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            ActionOnPlayerLeft?.Invoke(runner, player);
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            ActionOnShutdown?.Invoke(runner, shutdownReason);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var inputs = new MyNetworkInput();

            if (Input.GetKey(KeyCode.W)) {
                inputs.Buttons.Set(MyNetworkInput.InputType.FORWARD, true);
            }

            if (Input.GetKey(KeyCode.S)) {
                inputs.Buttons.Set(MyNetworkInput.InputType.BACKWARD, true);
            }

            if (Input.GetKey(KeyCode.A)) {
                inputs.Buttons.Set(MyNetworkInput.InputType.LEFT, true);
            }

            if (Input.GetKey(KeyCode.D)) {
                inputs.Buttons.Set(MyNetworkInput.InputType.RIGHT, true);
            }
            /*
            if (Input.GetKey(KeyCode.Space)) {
                inputs.Buttons.Set(MyNetworkInput.BUTTON_JUMP, true);
            }
            
            if (Input.GetKey(KeyCode.E)) {
                inputs.Buttons.Set(MyNetworkInput.BUTTON_ACTION1, true);
            }
            
            if (Input.GetMouseButton(0)) {
                inputs.Buttons.Set(MyNetworkInput.BUTTON_FIRE, true);
            }
            */
            input.Set(inputs);
        }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
        public void OnConnectedToServer(NetworkRunner runner) {}
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {}
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

        public void OnSceneLoadDone(NetworkRunner runner) {}
        public void OnSceneLoadStart(NetworkRunner runner) {}
    }
}
