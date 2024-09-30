using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureControl : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] ProceduralAnimationController procAnimController;
    [SerializeField] FollowPath followPathControl;
    [Header("Overall Creature Attributes")]
    [SerializeField] bool startMovingOnStart = true;
    [SerializeField] float walkingSpeed;

    private void Awake()
    {
        if (startMovingOnStart)
        {
            followPathControl.SetShouldMove(true);
        }
    }
}
