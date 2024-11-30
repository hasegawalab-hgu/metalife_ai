using Fusion;
using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;
using ExitGames.Client.Photon.StructWrapping;
using Unity.Collections;
using System;
using UnityEngine.XR;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Rendering;
using UnityEngine.Tilemaps;
using TMPro;

public class PlayerMovement : NetworkBehaviour
{
    private LocalGameManager lgm;
    private ChatUIManager chatUIManager;
    float _moveSpeed = 0.25f;
    public float _moveAmount = 1.0f;
    bool _isMoving = false;
    Vector3 _currentDir;
    [Networked]
    public int CurrentInputType {get; set;}
    [Networked]
    private float animatorSpeed {get; set;}

    private List<Sprite> sprites = new List<Sprite>();
    private SpriteRenderer sr;

    [Networked]
    private int currentSpriteIndex {get; set;} = 1;

    private int count = 0;

    private Tilemap backgroundTilemap;  //背景のタイルマップ
    private Tilemap touchableObjectsTilemap;  //衝突判定のあるタイルマップ
    private Tilemap officeGroundTilemap;  //会議室タイルマップ
    private Tilemap chairTilemap;  //座れるタイルマップ用
    private PlayerData playerData;  //クラスのフィールドとして定義

    

    // Animator animator;

    public override void Spawned()
    {
        //animator = GetComponent<Animator>();
        //animator.speed = 0f;
        _currentDir = new Vector3(0f, _moveAmount, 0f);
        lgm = GameObject.Find("LocalGameManager").GetComponent<LocalGameManager>();
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
        sprites = GetComponent<PlayerData>().sprites;
        sr = GetComponent<SpriteRenderer>();
        // タイルマップを取得
        backgroundTilemap = GameObject.Find("background").GetComponent<Tilemap>(); 
        touchableObjectsTilemap = GameObject.Find("TouchableObjects").GetComponent<Tilemap>();
        officeGroundTilemap = GameObject.Find("OfficeGround").GetComponent<Tilemap>();  // 会議室用タイルマップを取得
        chairTilemap = GameObject.Find("ChairTilemap").GetComponent<Tilemap>(); // 座れるタイルマップ
        playerData = GetComponent<PlayerData>(); // PlayerData インスタンスを取得
    }

    
    public override void FixedUpdateNetwork()
    {

        if (lgm.LocalGameState == LocalGameManager.GameState.ChatAndSettings)
        {
            return;
        }

        if (HasStateAuthority == false)
        {
            Debug.Log("HasStateAuthority: false");
            return;
        }

        if (GetInput<MyNetworkInput>(out var input))
        {
            // 座っている場合、移動を無効にする
            if (playerData.isSitting)
            {
                Debug.Log("座っているので移動できません");
                return;
            }

            if (_isMoving == false)
            {
                
                if (input.IsDown(MyNetworkInput.InputType.FORWARD))
                {
                    OnMove(new Vector3(0f, _moveAmount, 0f), MyNetworkInput.InputType.FORWARD);
                }
                if (input.IsDown(MyNetworkInput.InputType.BACKWARD))
                {
                    OnMove(new Vector3(0f, -_moveAmount, 0f), MyNetworkInput.InputType.BACKWARD);
                }
                if (input.IsDown(MyNetworkInput.InputType.RIGHT))
                {
                    OnMove(new Vector3(_moveAmount, 0f, 0f), MyNetworkInput.InputType.RIGHT);
                }
                if (input.IsDown(MyNetworkInput.InputType.LEFT))
                {
                    OnMove(new Vector3(-_moveAmount, 0f, 0f), MyNetworkInput.InputType.LEFT);
                }
                
            }
        }
    }

    void Update()
    {
        // "E"キーを押したときに座る/立つ処理を呼び出す
        if (Input.GetKeyDown(KeyCode.E))
        {
            TrySitOnChair();
        }

        
    }


    // 椅子に座る/立つ操作
    public void TrySitOnChair()
    {
        if (playerData.isSitting)
        {
            playerData.isSitting = false; // 立つ
            Debug.Log("立ち上がった");
        }
        else
        {
            if (IsNearChairTile())
            {
                playerData.isSitting = true;  // 座る
                Debug.Log("椅子に座った");
            }
            else
            {
                //Debug.Log("椅子の近くにいません。座れません。");
            }
        }
    }

    // 現在位置の1マス以内にChairTileがあるか判定
    private bool IsNearChairTile()
    {
        // 現在のセル位置を取得
        Vector3Int currentCellPos = chairTilemap.WorldToCell(transform.position);

        // 上下左右の相対位置
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(0, 1, 0),  // 上
            new Vector3Int(0, -1, 0), // 下
            new Vector3Int(1, 0, 0),  // 右
            new Vector3Int(-1, 0, 0)  // 左
        };

        // 周囲のセルをチェック
        foreach (var direction in directions)
        {
            Vector3Int neighborCellPos = currentCellPos + direction;
            var chairTile = chairTilemap.GetTile<Tile>(neighborCellPos);

            if (chairTile != null)
            {
                return true; // 椅子タイルが見つかったらtrueを返す
            }
        }

        // 周囲に椅子タイルがなかった場合
        return false;
    }



    public void CheckMeetingRoomEntry(Vector3 position)
    {
        Vector3Int cellPos = officeGroundTilemap.WorldToCell(position);
        Debug.Log($"現在位置のセル座標: {cellPos}");
        var meetingTile = officeGroundTilemap.GetTile<Tile>(cellPos);
        if (meetingTile != null)
        {
            playerData.CheckMeetingRoomEntry = true;
            Debug.Log("会議室に入室しました。");
        }
            else
            {
                playerData.CheckMeetingRoomEntry = false;
                Debug.Log("会議室外です。");
            }
    }

    public override void Render()
    {
        if(sprites.Count != 0)
        {
            sr.sprite = sprites[currentSpriteIndex];
        }
        //animator.speed = animatorSpeed;
        //animator.SetInteger("Direction", (int)currentInputType);
    }

    public void OnMove(Vector3 dir, MyNetworkInput.InputType inputType)
    {
        CurrentInputType = (int)inputType;

        if(chatUIManager.inputField.isFocused)
        {
            return;
        }

        if(sprites.Count == 0)
        {
            return;
        }

        if (dir == _currentDir)
        {
            count++;
            // animator.SetInteger("Direction", (int)inputType);
            StartCoroutine(Move(dir, inputType));
        }
        else
        {
            count = 0;
            _currentDir = dir;
            currentSpriteIndex = (int)inputType * 3 + 1;
        }
    }

    IEnumerator Move(Vector3 dir, MyNetworkInput.InputType inputType)
    {
        Vector3 targetPos = transform.position + dir;

        // 2つのタイルマップを順番に確認する
        if (CheckCollisionWithTilemaps(targetPos))
        {
            _isMoving = false; // 移動を中止
            yield break;       // コルーチンを終了
        }

        float seconds = 0;
        while ((targetPos - transform.position).sqrMagnitude != 0.0f)
        {
            seconds += Time.deltaTime;
            if (seconds > _moveSpeed * 2)
            {
                Debug.Log("歩行エラー");
                break;
            }

            if (count % 2 == 0)
            {
                currentSpriteIndex = (int)inputType * 3;
            }
            else
            {
                currentSpriteIndex = (int)inputType * 3 + 2;
            }
            animatorSpeed = 2f;
            _isMoving = true;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, _moveSpeed);
            yield return null;
        }

        currentSpriteIndex = (int)inputType * 3 + 1;
        animatorSpeed = 0f;
        transform.position = targetPos;
        _isMoving = false;
    }

    // タイルマップでの衝突を確認するメソッド
    private bool CheckCollisionWithTilemaps(Vector3 targetPos)
    {
        // プレイヤーの目的座標をタイルマップのセル座標に変換
        Vector3Int cellPos = backgroundTilemap.WorldToCell(new Vector3(targetPos.x, targetPos.y, 0));

        // 背景タイルマップの衝突判定
        var backgroundTile = backgroundTilemap.GetTile<Tile>(cellPos);
        if (backgroundTile != null) // ここで "background"の名前を確認
        {
            return true;
        }

        // タッチ可能オブジェクトタイルマップの衝突判定
        var touchableTile = touchableObjectsTilemap.GetTile<Tile>(cellPos);
        if (touchableTile != null) // ここで "TouchableObjects" の名前を確認
        {
            return true;
        }

        // どちらのタイルマップにも衝突していない場合
        return false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("OnCollition2D: " + collision.gameObject.name);
    }
}
