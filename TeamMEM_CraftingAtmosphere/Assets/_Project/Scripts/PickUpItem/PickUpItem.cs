using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    private Vector3 _initPosition;
    private Quaternion _initRotation;

    private void Awake()
    {
        _initPosition = transform.position;
        _initRotation = transform.rotation;
    }


    public void DoPickUp(Vector3 toPickUpPosition, Quaternion toPickUpRotation, float duration)
    {
        transform.DOMove(toPickUpPosition, duration);
        transform.DORotateQuaternion(toPickUpRotation, duration);
    }


    public void DoPutDown(float duration)
    {
        transform.DOMove(_initPosition, duration);
        transform.DORotateQuaternion(_initRotation, duration);
    }
}
