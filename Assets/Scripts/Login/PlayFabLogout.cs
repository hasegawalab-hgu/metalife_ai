using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ExperimentationModels;

public class PlayFabLogout : MonoBehaviour
{
    // NetworkRunnerを持っているオブジェクト
    public GameObject mainLobby; 
    void Start()
    {
        mainLobby = GameObject.Find("MainLobby");
    }

    public void OnClickLogout()
    {
        // ログイン情報を削除する
        PlayFabSettings.staticPlayer.ForgetAllCredentials();
        PlayFabData.Islogouted = true;
        PlayFabData.Initialize();
        mainLobby.transform.parent = null; // DontDestroyOnLoadから抜ける
        SceneManager.MoveGameObjectToScene(mainLobby, SceneManager.GetActiveScene());
        SceneManager.LoadScene("Menu");
    }
}
