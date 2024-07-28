using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using ExitGames.Client.Photon.StructWrapping;

public class LoadSprites : MonoBehaviour
{
    public string folderPath = "Assets/Character/pipoya_textures/現代系/"; // フォルダのパス
    public Image imagePrefab; // 表示用のImageプレハブ
    public GameObject spawner;

    void Start()
    {
        LoadAllImages();
    }

    void LoadAllImages()
    {
        // フォルダ内のすべての.pngファイルを取得
        string[] filePaths = Directory.GetFiles(folderPath, "*.png");
        
        foreach (string filePath in filePaths)
        {
            StartCoroutine(LoadImage(filePath));
        }
    }

    IEnumerator LoadImage(string filePath)
    {
        // ファイルをバイト配列として読み込み
        byte[] fileData = File.ReadAllBytes(filePath);
        
        // Texture2Dにロード
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);

        // Texture2DをSpriteに変換
        Sprite sprite = Sprite.Create(texture, new Rect(32, 96, 32, 32), new Vector2(0.5f, 0.5f));

        // 新しいImageオブジェクトを作成し、コンテナに配置
        Image newImage = Instantiate(imagePrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
        newImage.transform.SetParent(spawner.transform);
        newImage.sprite = sprite;
        newImage.GetComponent<CharacterTexture>().texture = texture;
        newImage.transform.gameObject.name = filePath.Substring(folderPath.Length, filePath.Length - folderPath.Length - 4);
        
        yield return null;
    }
}
