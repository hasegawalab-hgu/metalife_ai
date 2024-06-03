using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using static CustomConnectionHandler.ConnectionData.ConnectionTarget;

namespace CustomConnectionHandler {
    public class ConnectionManager : MonoBehaviour {

        public static ConnectionManager Instance;
        
        public bool IsDungeonHost() => _dungeonConnection.IsRunning && _dungeonConnection.Runner.IsServer;
        public ConnectionContainer GetDungeonConnection() => _dungeonConnection;
        public ConnectionContainer GetLobbyConnection() => _lobbyConnection;

        [SerializeField] private App _app;
        [SerializeField] private ConnectionData _defaultLobby;
        
        private ConnectionContainer _lobbyConnection = new ConnectionContainer();
        private ConnectionContainer _dungeonConnection = new ConnectionContainer();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }else if (Instance != this)
            {
                Destroy(this);
                return;
            }
            
            DontDestroyOnLoad(this);
        }

        public void LoadDungeonLevel()
        {
            if (!_dungeonConnection.IsRunning) return;

            _dungeonConnection.App.RPC_ShutdownRunner(Lobby);

            if (_dungeonConnection.Runner.IsServer)
            {
                _dungeonConnection.Runner.SessionInfo.IsOpen = false;
                _dungeonConnection.Runner.LoadScene(SceneRef.FromIndex(_dungeonConnection.ActiveConnection.SceneIndex));
            }
        }

        public async Task ShutdownRunner(ConnectionContainer container)
        {
            if (container.IsRunning)
                await container.Runner.Shutdown();
        }

        public async Task ConnectToRunner(ConnectionData connectionData, Action<NetworkRunner> onInitialized = default, Action<ShutdownReason> onFailed = default)
        {
            //Get correct connection reference.
            var connection = connectionData.Target == Lobby ? _lobbyConnection : _dungeonConnection;
            connection.ActiveConnection = connectionData;

            var gameMode = connectionData.Target == Lobby ? GameMode.Shared : GameMode.AutoHostOrClient;
            var sceneInfo = new NetworkSceneInfo();
            if (connectionData.Target == Lobby)
            {
                sceneInfo.AddSceneRef(SceneRef.FromIndex(connectionData.SceneIndex));
            }
            var sessionProperties = new Dictionary<string, SessionProperty>()
            { { "ID", (int)connectionData.ID } };

            if (connection.Runner == default)
            {
                var child = new GameObject(connection.ActiveConnection.ID.ToString());
                child.transform.SetParent(transform);
                connection.Runner = child.AddComponent<NetworkRunner>();
            }

            if (connection.Callback == default)
                connection.Callback = new ConnectionCallbacks();

            if (connectionData.Target == Dungeon)
                connection.Callback.ActionOnShutdown += OnDungeonShutdown;

            if (connection.IsRunning)
            {
                Debug.Log("Shutdown");
                await connection.Runner.Shutdown();
            }

            if (connectionData.Target == Lobby && _dungeonConnection.IsRunning) // Shutdown Dungeon runner if going back to a lobby
                await _dungeonConnection.Runner.Shutdown();

            connection.Runner.AddCallbacks(connection.Callback);

            onInitialized += async runner =>
            {
                Debug.Log(runner);
                if (runner.IsServer || runner.IsSharedModeMasterClient)
                {
                    await runner.SpawnAsync(_app);
                }
            };


            var startResult = await connection.Runner.StartGame(new StartGameArgs()
            { 
                GameMode = gameMode,
                SessionProperties = sessionProperties,
                EnableClientSessionCreation = true,
                Scene = sceneInfo, PlayerCount = connectionData.MaxClients,
                OnGameStarted = onInitialized,
                SceneManager = connection.Runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
            
            if (!startResult.Ok)
                onFailed?.Invoke(startResult.ShutdownReason);
        }
        
        private void OnDungeonShutdown(NetworkRunner runner, ShutdownReason reason)
        {
            if (reason == ShutdownReason.DisconnectedByPluginLogic)
            {
                _ = ConnectToRunner(_defaultLobby);
            }
        }
    }
}
