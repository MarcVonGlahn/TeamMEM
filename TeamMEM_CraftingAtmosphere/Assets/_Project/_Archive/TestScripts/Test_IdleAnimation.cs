using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Test_IdleAnimation : MonoBehaviour
{
    public Transform rootBone;
    public AnimationCurve animCurve;

    public float duration = 3.0f;


    private float _timer = 0;

    private Vector3 _initPosition;

    private void Start()
    {
        _initPosition = rootBone.position;
    }

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;

        if(_timer > duration)
        {
            _timer = 0;
            return;
        }
        
        float lerp = _timer / duration;
        float eval = animCurve.Evaluate(lerp);

        rootBone.position = new Vector3(_initPosition.x, _initPosition.y + eval, _initPosition.z);
    }
}
