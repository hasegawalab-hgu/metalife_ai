using Fusion;
using UnityEngine;

public interface IInteractable {
    public void Interact(NetworkRunner runner, PlayerRef player);
}
