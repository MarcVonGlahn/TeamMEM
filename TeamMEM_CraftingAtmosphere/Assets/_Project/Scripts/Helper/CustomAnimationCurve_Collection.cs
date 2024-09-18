using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CustomAnimationCurveType
{
    Linear,
    SmoothInOut,
    ReverseQuadraticCurve,
    HeavyStepCurve
}

[CreateAssetMenu(fileName = "Custom Animation Curve Collection", menuName = "Custom/Custom Animation Curve Collection")]
public class CustomAnimationCurve_Collection : ScriptableObject
{
    public List<CustomAnimationCurve> animationCurves;

    public AnimationCurve GetAnimationCurve(CustomAnimationCurveType type)
    {
        return animationCurves.Find(x => x.type == type).animCurve;
    }
}


[System.Serializable]
public class CustomAnimationCurve
{
    public CustomAnimationCurveType type;
    public AnimationCurve animCurve;
}
