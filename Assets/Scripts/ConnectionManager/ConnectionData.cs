using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace CustomConnectionHandler {

    [CreateAssetMenu(menuName = "Connection Manager/Connection Data")]
    public class ConnectionData : ScriptableObject {
        public enum ConnectionTarget { Lobby, Dungeon }
        public enum ConnectionID { MainLobby, FirstDungeon, SecondDungeon }

        [Header("Only for visual feedback")]
        public string Name;
        [Space]
        [Header("ID referent to the connection (Each unique session should have a different one)")]
        public ConnectionID ID;
        [Header("Used to treat different connections between Shared lobby or Host Dungeon")]
        public ConnectionTarget Target;
        [Space]
        public int MaxClients = 20;
        public int SceneIndex;
    }
}
