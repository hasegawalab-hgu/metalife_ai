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
    float _moveSpeed = 0.25f;
    float _moveAmount = 1.0f;
    bool _isMoving = false;
    Vector3 _currentDir;
    [Networked]
    private int currentInputType {get; set;}
    [Networked]
    private float animatorSpeed {get; set;}

    private List<Sprite> sprites = new List<Sprite>();
    private SpriteRenderer sr;

    [Networked]
    private int currentSpriteIndex {get; set;} = 1;

    private int count = 0;

    private Tilemap tilemap;
    // Animator animator;

    public override void Spawned()
    {
        //animator = GetComponent<Animator>();
        //animator.speed = 0f;
        _currentDir = new Vector3(0f, _moveAmount, 0f);
        lgm = GameObject.Find("LocalGameManager").GetComponent<LocalGameManager>();
        sprites = GetComponent<PlayerData>().sprites;
        sr = GetComponent<SpriteRenderer>();
        tilemap = GameObject.Find("background").GetComponent<Tilemap>(); // 壁のタイルマップ
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

    public override void Render()
    {
        if(sprites.Count != 0)
        {
            sr.sprite = sprites[currentSpriteIndex];
        }
        //animator.speed = animatorSpeed;
        //animator.SetInteger("Direction", (int)currentInputType);
    }

    private void OnMove(Vector3 dir, MyNetworkInput.InputType inputType)
    {
        currentInputType = (int)inputType;

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

        if(tilemap != null)
        {
            var cellPos = tilemap.WorldToCell(new Vector3(targetPos.x, targetPos.y, 0));
            if(tilemap.GetTile(cellPos) != null)
            {
                Debug.Log(tilemap.GetTile(cellPos));
                _isMoving = false;
                yield break;
            }
        }

        float seconds = 0;
        while ((targetPos - transform.position).sqrMagnitude != 0.0f)
        {
            seconds += Time.deltaTime;
            if(seconds > _moveSpeed * 2)
            {
                Debug.Log("歩行エラー");
                break;
            }

            if(count % 2 == 0)
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("OnCollition2D: " + collision.gameObject.name);
    }
}
