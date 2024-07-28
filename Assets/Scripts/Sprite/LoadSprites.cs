using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class LoadSprites : MonoBehaviour
{
    public Image imagePrefab; // 表示用のImageプレハブ
    public GameObject spawner;
    public List<string> Paths = new List<string>(){"Modern/", "Fantasy/", "Monster/", "Nekoninn/", "Animal/"};
    public int index = 0;
    public TMP_Text typeText; // 種類名（フォルダー名）

    private void DestroyChildren(Transform root)
    {
        foreach(Transform child in root.transform)
        {
            Destroy(child.gameObject);
        }   
    }

    public void LoadAllImages(string folderPath)
    {
        DestroyChildren(spawner.transform);
        // フォルダ内のすべての.pngファイルを取得
        Object[] objects = Resources.LoadAll(folderPath, typeof(Texture2D));
        typeText.text = folderPath.Substring(0, folderPath.Length - 1);
        
        foreach (Texture2D texture in objects)
        {
            StartCoroutine(LoadImage(texture));
        }
    }

    private IEnumerator LoadImage(Texture2D texture)
    {
        // // ファイルをバイト配列として読み込み
        // byte[] fileData = File.ReadAllBytes(filePath);
        
        // // Texture2Dにロード
        // Texture2D texture = new Texture2D(2, 2);
        // texture.LoadImage(fileData);

        // Texture2DをSpriteに変換
        Sprite sprite = Sprite.Create(texture, new Rect(32, 96, 32, 32), new Vector2(0.5f, 0.5f));

        // 新しいImageオブジェクトを作成し、コンテナに配置
        Image newImage = Instantiate(imagePrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
        newImage.transform.SetParent(spawner.transform);
        newImage.sprite = sprite;
        newImage.GetComponent<CharacterTexture>().texture = texture;
        // newImage.transform.gameObject.name = filePath.Substring(folderPath.Length, filePath.Length - folderPath.Length - 4);
        
        yield return null;
    }
}
