using Fusion;
using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;
using ExitGames.Client.Photon.StructWrapping;
using Unity.Collections;
using System;
using UnityEngine.XR;

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
    Animator animator;

    public override void Spawned()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f;
        _currentDir = new Vector3(0f, _moveAmount, 0f);
        lgm = GameObject.Find("LocalGameManager").GetComponent<LocalGameManager>();
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
        animator.speed = animatorSpeed;
        animator.SetInteger("Direction", (int)currentInputType);
    }

    private void OnMove(Vector3 dir, MyNetworkInput.InputType inputType)
    {
        currentInputType = (int)inputType;

        if (dir == _currentDir)
        {
            // animator.SetInteger("Direction", (int)inputType);
            StartCoroutine(Move(dir));
        }
        else
        {
            _currentDir = dir;
        }
    }

    IEnumerator Move(Vector3 dir)
    {
        Vector3 targetPos = transform.position + dir;

        while ((targetPos - transform.position).sqrMagnitude != 0.0f)
        {
            animatorSpeed = 2f;
            _isMoving = true;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, _moveSpeed);
            yield return null;
        }
        animatorSpeed = 0f;
        transform.position = targetPos;
        _isMoving = false;
    }
}
