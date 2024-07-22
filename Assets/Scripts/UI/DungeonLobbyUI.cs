using System;
using System.Linq;
using CustomConnectionHandler;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Interface {
    public class DungeonLobbyUI : UIScreen {
        [SerializeField] private TMP_Text _dungeonInfo;
        [SerializeField] private Button _connectButton;

        public void ShowDungeonLoby()
        {
            _dungeonInfo.text = "Connecting";
            _connectButton.interactable = false;
            Focus();
        }

        public void LoadDungeonLevel()
        {
            ConnectionManager.Instance.LoadDungeonLevel();
            Defocus();
        }

        public void ExitDungeonLobby()
        {
            _ = ConnectionManager.Instance.ShutdownRunner(ConnectionManager.Instance.GetDungeonConnection());
        }
        
        private void UpdateDungeonInfo(NetworkRunner runner, PlayerRef player)
        {
            if (player == runner.LocalPlayer)
                _connectButton.interactable = ConnectionManager.Instance.IsDungeonHost();

            var name = ConnectionManager.Instance.GetDungeonConnection().ActiveConnection.Name;
            int currentPlayers = ConnectionManager.Instance.GetDungeonConnection().Runner.ActivePlayers.Count();
            int maxPlayers = ConnectionManager.Instance.GetDungeonConnection().ActiveConnection.MaxClients;
            
            _dungeonInfo.text = $"{name} {currentPlayers}/{maxPlayers}";
        }

        private void CancelConnection(NetworkRunner runner, ShutdownReason reason)
        {
            Defocus();
        }

        private void OnEnable()
        {
            ConnectionManager.Instance.GetDungeonConnection().Callback.ActionOnPlayerJoined += UpdateDungeonInfo;
            ConnectionManager.Instance.GetDungeonConnection().Callback.ActionOnPlayerLeft += UpdateDungeonInfo;
            ConnectionManager.Instance.GetDungeonConnection().Callback.ActionOnShutdown += CancelConnection;
        }

        private void OnDisable()
        {
            ConnectionManager.Instance.GetDungeonConnection().Callback.ActionOnPlayerJoined -= UpdateDungeonInfo;
            ConnectionManager.Instance.GetDungeonConnection().Callback.ActionOnPlayerLeft -= UpdateDungeonInfo;
            ConnectionManager.Instance.GetDungeonConnection().Callback.ActionOnShutdown -= CancelConnection;
        }
    }
}
