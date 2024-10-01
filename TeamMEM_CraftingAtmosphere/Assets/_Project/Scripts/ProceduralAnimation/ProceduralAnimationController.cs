using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum WalkingStyle
{
    Biped,
    Triped,
    Quadriped
}


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
    [Space]
    [SerializeField] private WalkingStyle walkingStyle;

    // Start is called before the first frame update
    void Awake()
    {
        AnimationCurve generalCurve = curves.GetAnimationCurve(stepAnimCurveType);

        foreach (MultiBoneIK ik in ikControllers)
        {
            ik.SetupIKController(stepSpeed, maxStepDuration, stepHeight, generalCurve);
        }
    }


    private void Update()
    {
        AngleRootbone();
    }


    private void AngleRootbone()
    {
        switch (walkingStyle)
        {
            case WalkingStyle.Biped:
                break;
            case WalkingStyle.Triped:
                break;
            case WalkingStyle.Quadriped:
                QuadripedAngleRootbone();
                break;
        }
    }


    private void QuadripedAngleRootbone()
    {
        Vector3 rightVector = ikControllers[0].GetPlantLegTargetPosition() - ikControllers[2].GetPlantLegTargetPosition();
        Vector3 leftVector = ikControllers[1].GetPlantLegTargetPosition() - ikControllers[3].GetPlantLegTargetPosition();

        Vector3 averageVector = ((rightVector + leftVector) / 2.0f).normalized;

        Quaternion targetRotation;
        try
        {
            targetRotation = Quaternion.LookRotation(averageVector);
        }
        catch
        {
            targetRotation = Quaternion.LookRotation(rootbone.forward);
        }

        rootbone.rotation = Quaternion.Lerp(rootbone.rotation, Quaternion.LookRotation(averageVector), 0.5f * Time.deltaTime);
    }
}
