using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureControl : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] ProceduralAnimationController procAnimController;
    [SerializeField] FollowPath followPathControl;
    [SerializeField] CreaturePropertiesHelper creaturePropertiesHelper;
    [Header("Overall Creature Attributes")]
    [SerializeField] bool startMovingOnStart = true;
    [SerializeField] SO_Moveset moveset;
    

    private void Awake()
    {
        if (startMovingOnStart)
        {
            followPathControl.SetShouldMove(true);
            followPathControl.SetWalkingSpeed(moveset.WalkingSpeed);
        }

        creaturePropertiesHelper.SetCreatureProperties(moveset);
    }


    public SO_Moveset GetMoveset()
    {
        return moveset;
    }
}
