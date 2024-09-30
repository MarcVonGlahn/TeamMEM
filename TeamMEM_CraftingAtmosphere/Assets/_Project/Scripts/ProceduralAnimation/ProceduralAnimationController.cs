using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralAnimationController : MonoBehaviour
{
    [Header("Leg Controllers")]
    [SerializeField] List<MultiBoneIK> ikControllers;
    [Header("Additional Bones")]
    [SerializeField] Transform rootbone;
    [Header("Step Settings")]
    [Tooltip("The faster the step speed, the faster the step")]
    [SerializeField] private float stepSpeed = 0.05f;
    [SerializeField] private float maxStepDuration = 1.0f;
    [SerializeField] private float stepHeight = 0.5f;
    [SerializeField] private CustomAnimationCurve_Collection curves;
    [SerializeField] private CustomAnimationCurveType stepAnimCurveType;

    // Start is called before the first frame update
    void Awake()
    {
        AnimationCurve generalCurve = curves.GetAnimationCurve(stepAnimCurveType);

        foreach (MultiBoneIK ik in ikControllers)
        {
            ik.SetupIKController(stepSpeed, maxStepDuration, stepHeight, generalCurve);
        }
    }
}
