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
    private PlayerData pd;
    private SpriteRenderer sr;

    [Networked]
    private int currentSpriteIndex {get; set;} = 1;

    private int count = 0;

    private Tilemap backgroundTilemap;
    private Tilemap touchableObjectsTilemap; 
    private Tilemap officeGroundTilemap;

    private int stateInfoUpdateCount = 0;
    private const int stateInfoUpdateMaxCount = 5;
    // Animator animator;

    public override void Spawned()
    {
        //animator = GetComponent<Animator>();
        //animator.speed = 0f;
        _currentDir = new Vector3(0f, _moveAmount, 0f);
        lgm = GameObject.Find("LocalGameManager").GetComponent<LocalGameManager>();
        chatUIManager = GameObject.Find("ChatManager").GetComponent<ChatUIManager>();
        pd = GetComponent<PlayerData>();
        sprites = GetComponent<PlayerData>().sprites;
        sr = GetComponent<SpriteRenderer>();
        // 3つのタイルマップを取得
        backgroundTilemap = GameObject.Find("background").GetComponent<Tilemap>(); 
        touchableObjectsTilemap = GameObject.Find("TouchableObjects").GetComponent<Tilemap>();
        officeGroundTilemap = GameObject.Find("OfficeGround").GetComponent<Tilemap>();  // 会議室用タイルマップを取得
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

    private void CheckMeetingRoomEntry(Vector3 position)
    {
        Vector3Int cellPos = officeGroundTilemap.WorldToCell(position);
        var meetingTile = officeGroundTilemap.GetTile<Tile>(cellPos);

        if (meetingTile != null)
        {
            //Debug.Log("会議室に入室しました。");
            // 会議室に入った際に他の処理を行う場合、ここに追加可能
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

        if(!pd.IsAI && chatUIManager.inputField.isFocused)
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
        /*
        stateInfoUpdateCount++;
        if(!pd.IsAI && stateInfoUpdateCount == stateInfoUpdateMaxCount)
        {
            pd.UpdateListStateInfo();
            stateInfoUpdateCount = 0;
        }
        */
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
