using System;
using TMPro;
using UnityEngine;
public class MessageUI : UIScreen {
    public TMP_Text Text;

    private float _closeDelay = 1f;

    public void ShowMessage(string text)
    {
        Text.text = text;
        Focus();
    }

    public void CloseMessagePanel()
    {
        if (_closeDelay > 0) return;
        
        Defocus();
    }

    private void Update()
    {
        if (_closeDelay > 0)
            _closeDelay -= Time.deltaTime;
    }
}
