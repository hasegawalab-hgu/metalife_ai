using UnityEngine;

namespace Interface {
    public class InterfaceManager : MonoBehaviour {
        public static InterfaceManager Instance;

        public ConnectionGateUI GateUI;
        public DungeonLobbyUI DungeonLobbyUI;
        public MessageUI MessageUI;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            } else if (Instance != this)
            {
                Destroy(this);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
        }

        public void ClearInterface()
        {
            UIScreen.activeScreen.Defocus();
        }
    }
}
