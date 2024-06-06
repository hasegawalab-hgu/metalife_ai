using Fusion;
using Interface;
using UnityEngine;
using UnityEngine.Serialization;
public class DialogInteraction : SimulationBehaviour, IInteractable {
    [TextArea]
    [SerializeField] private string _DialogText;
    public void Interact(NetworkRunner runner, PlayerRef player)
    {
        // Dialog should only do something for the local player, but the input detection (interact detection)
        // need to be networked because of other interactions possible with other objects.
        if (player != runner.LocalPlayer) return;
        
        InterfaceManager.Instance.MessageUI.ShowMessage(_DialogText);
    }
}
