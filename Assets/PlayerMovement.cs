using Fusion;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority == false)
        {
            Debug.Log("false");
            return;
        }
        
    }
}
