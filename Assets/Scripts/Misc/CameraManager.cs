using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    private Transform _target;

    private void LateUpdate()
    {
        if (!_target) return;

        float speed = Vector3.Distance(_target.position, transform.position);

        transform.position = Vector3.MoveTowards(transform.position, _target.position, speed * Time.deltaTime);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}
