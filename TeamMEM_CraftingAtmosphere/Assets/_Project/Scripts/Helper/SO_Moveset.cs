using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum WalkingStyle
{
    Biped,
    Triped,
    Quadriped
}

public enum Corruption
{
    Yes,
    No
}

[CreateAssetMenu(fileName = "Moveset_", menuName = "Custom/Custom Moveset")]
public class SO_Moveset : ScriptableObject
{
    [Header("Movement Properties")]
    [Tooltip("Start at (0,0), end at (1,1)")]
    public AnimationCurve StepMakeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Start at (0,0), end at (1,0)")]
    public AnimationCurve StepHeightCurve;
    [Space]
    public float RootBoneHeight = 1.5f;
    [Space]
    public float StepDuration = 1.0f;
    public float StepHeight = 1.0f;
    public float StepLengthTreshhold = 2.5f;
    [Space]
    public float HeadRotationSpeed = 1.5f;
    public float BodyRotationSpeed = 1.0f;
    [Space]
    public float WalkingSpeed = 1.0f;
    [Space]
    public AnimationCurve RootBoneAnimationCurve;
    public float RootLevelChangeAmount = 0.1f;
    public float RootLevelChangeDuration = 1.0f;

    [Space]
    [Space]
    [Header("Creature Properties")]
    public string CreatureName = "Example";
    public string CreatureBiologicalName = "Examplaris Randomis";
    public string CreatureID = "1";
    public float Height = 1.5f;
    public WalkingStyle WalkingStyle = WalkingStyle.Biped;
    public Corruption Corruption = Corruption.No;


    public float GetRootHeightAdjust(float timerVal)
    {
        return RootBoneAnimationCurve.Evaluate(timerVal / RootLevelChangeDuration) * RootLevelChangeAmount;
    }
}
