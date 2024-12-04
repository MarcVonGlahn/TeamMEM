using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    [SerializeField] Texture _texture;
    [SerializeField] MeshRenderer _renderer;

    private Vector3 _initPosition;
    private Quaternion _initRotation;

    private void Awake()
    {
        _initPosition = transform.position;
        _initRotation = transform.rotation;

        var mat = new Material(_renderer.material);
        mat.mainTexture = _texture;

        _renderer.material = mat;
    }


    public void DoPickUp(Vector3 toPickUpPosition, Quaternion toPickUpRotation, float duration)
    {
        transform.DOMove(toPickUpPosition, duration);
        transform.DORotateQuaternion(toPickUpRotation, duration);

        AudioManager.instance.Play("PickUpPaper");
    }


    public void DoPutDown(float duration)
    {
        transform.DOMove(_initPosition, duration);
        transform.DORotateQuaternion(_initRotation, duration);
    }
}
