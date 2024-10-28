using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChatNotificationSound : MonoBehaviour
{
    // 通知音用のAudioSource
    public AudioSource notificationAudioSource;

// 通知音を再生する関数
    public void PlayNotificationSound()
    {
        if (notificationAudioSource != null)
        {
            notificationAudioSource.Play(); // 通知音を再生
        }
        else
        {
            Debug.LogWarning("通知音用のAudioSourceが設定されていません。");
        }
    }
}

