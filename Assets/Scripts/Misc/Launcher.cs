using System;
using CustomConnectionHandler;
using Fusion;
using TMPro;
using UnityEngine;

public class Launcher : MonoBehaviour {
    [SerializeField] private ConnectionData _initialConnection;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private GameObject _launchButton;

    public void Launch()
    {
        _text.text = "Connecting to Lobby";
        _text.gameObject.SetActive(true);
        _launchButton.SetActive(false);
        _ = ConnectionManager.Instance.ConnectToRunner(_initialConnection, onFailed: OnConnectionFailed);
    }

    private void OnConnectionFailed(ShutdownReason reason)
    {
        _text.text = $"Failed: {reason}";
        _launchButton.SetActive(true);
    }
}
