using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// playFabIdをFusionに受け渡すだけのクラス
/// </summary>
public class PlayFabId : MonoBehaviour
{
    public string playFabId;

    private void Start() {
        DontDestroyOnLoad(this);
    }
}
