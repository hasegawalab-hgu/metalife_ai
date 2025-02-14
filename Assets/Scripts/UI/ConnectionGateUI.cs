using System.Security.Cryptography.X509Certificates;
using CustomConnectionHandler;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Interface {
    public class ConnectionGateUI : UIScreen {
        [SerializeField] private Button _connectButton;
        private ConnectionData _data;
        
        public void ShowGate(ConnectionData data)
        {
            // Don't show gate UI if the DungeonLobbyUI is active
            if (InterfaceManager.Instance.DungeonLobbyUI.gameObject.activeInHierarchy) return;
            
            _data = data;
            Focus();
        }

        public void ConnectToSession()
        {
            _ = ConnectionManager.Instance.ConnectToRunner(_data);
            
            if (_data.Target == ConnectionData.ConnectionTarget.Dungeon)
                InterfaceManager.Instance.DungeonLobbyUI.ShowDungeonLoby();
            
            Defocus();
        }
    }
}
