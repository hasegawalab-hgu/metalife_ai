using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using PlayFab;

public class ReactionButton : MonoBehaviour 
{
    public int ReactionNum;
    private PlayerData lpd;

    void Update()
    {
        if(lpd == null && GameObject.Find("LocalPlayer") != null)
        {
            lpd = GameObject.Find("LocalPlayer").GetComponent<PlayerData>();
        }
    }

    public void OnClickReactionButton()
    {
        lpd.RPC_SendReactionRequest(lpd.PlayFabId, ReactionNum);
    }
}
