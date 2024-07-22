using Fusion;
using UnityEngine;
using System.Collections;

public class PlayerMovement : NetworkBehaviour
{
    float _moveSpeed = 0.5f;
    float _moveAmount = 1.0f;
    bool _isMoving = false;
    Vector3 _currentDir;

    public override void Spawned()
    {
        _currentDir = new Vector3(0f, _moveAmount, 0f);
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority == false)
        {
            Debug.Log("false");
            return;
        }
        /*
        if (_isMoving == true)
        {
            Move(_currentDir);
        }
        */
        if (GetInput<MyNetworkInput>(out var input))
        {
            if (_isMoving == false)
            {
                if (input.IsDown(MyNetworkInput.BUTTON_FORWARD))
                {
                    OnMove(new Vector3(0f, _moveAmount, 0f));
                }
                if (input.IsDown(MyNetworkInput.BUTTON_BACKWARD))
                {
                    OnMove(new Vector3(0f, -_moveAmount, 0f));
                }
                if (input.IsDown(MyNetworkInput.BUTTON_RIGHT))
                {
                    OnMove(new Vector3(_moveAmount, 0f, 0f));
                }
                if (input.IsDown(MyNetworkInput.BUTTON_LEFT))
                {
                    OnMove(new Vector3(-_moveAmount, 0f, 0f));
                }
            }
            
        }
    }

    private void OnMove(Vector3 dir)
    {
        if (dir == _currentDir)
        {
            StartCoroutine(Move(dir));
        }
        else
        {
            _currentDir = dir;

        }
    }

    IEnumerator Move(Vector3 dir)
    {
        _isMoving = true;
        Vector3 targetPos = transform.position + dir;

        while ((targetPos - transform.position).sqrMagnitude != 0.0f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, _moveSpeed);
            yield return null;
        }

        transform.position = targetPos;
        _isMoving = false;
    }
}
