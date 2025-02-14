using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTexture : MonoBehaviour
{
    public Texture2D texture;
    
    public void OnClickButton()
    {
        Sprite sprite = Sprite.Create(texture, new Rect(32, 96, 32, 32), new Vector2(0.5f, 0.5f));
        Debug.Log("button");
        CharacterSelect cs = GameObject.Find("LoadSprites").GetComponent<CharacterSelect>();
        cs.selectedTexture = texture;
        cs.selectedImage.sprite = sprite;
    }
}
