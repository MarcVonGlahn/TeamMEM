using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IKBone
{
    public Transform boneTransform;
    [Space]
    public Vector3 minRotation; // Minimum rotation limits for X, Y, Z axes
    public Vector3 maxRotation; // Maximum rotation limits for X, Y, Z axes
    [Space]
    public bool affectedByPull = false;
    public Transform pullTransform;
    [Range(0, 1)]
    public float pullTransformHeightAffect = 0.5f; 
}
