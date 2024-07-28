using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    private LoadSprites ls;
    public Texture2D selectedTexture;
    public Image selectedImage;

    void Start()
    {
        ls = GetComponent<LoadSprites>();
    }

    public void OnClickDecideButton()
    {
        
    }
}
