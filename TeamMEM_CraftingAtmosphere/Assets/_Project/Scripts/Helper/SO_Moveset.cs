using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum WalkingStyle
{
    Biped,
    Triped,
    Quadriped
}

[CreateAssetMenu(fileName = "Moveset_", menuName = "Custom/Custom Moveset")]
public class SO_Moveset : ScriptableObject
{
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
    public WalkingStyle WalkingStyle = WalkingStyle.Biped;
}
