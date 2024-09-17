using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Test_IdleAnimation : MonoBehaviour
{
    public bool doIdle = true;
    public bool doWalk = false;

    public Transform rootBone;

    [Header("Idle")]
    public AnimationCurve animCurveIdle;
    public float durationIdle = 3.0f;

    [Header("Walk")]
    public float walkSpeed = 1.0f;


    private float _timer = 0;

    private Vector3 _initPosition;

    private void Start()
    {
        _initPosition = rootBone.position;
    }


    void Update()
    {
        if (doIdle)
        {
            DoIdle();
            return;
        }

        if (doWalk)
        {
            DoWalk();
        }
    }


    private void DoIdle()
    {
        _timer += Time.deltaTime;

        if (_timer > durationIdle)
        {
            _timer = 0;
            return;
        }

        float lerp = _timer / durationIdle;
        float eval = animCurveIdle.Evaluate(lerp);

        rootBone.position = new Vector3(_initPosition.x, _initPosition.y + eval, _initPosition.z);
    }

    private void DoWalk()
    {
        transform.Translate(0, 0, walkSpeed *  Time.deltaTime);
    }
}
