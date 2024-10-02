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
    [SerializeField] List<FabrikIK> ikControllers;
    [Header("Additional Bones")]
    [SerializeField] Transform rootBone;
    [Header("Animation Settings")]
    [SerializeField] float bodyRotationSpeed = 1.0f;
    [SerializeField] FollowPath followPath;
    [Header("Step Settings")]
    [Tooltip("The faster the step speed, the faster the step")]
    [SerializeField] private float stepSpeed = 0.05f;
    [SerializeField] private float maxStepDuration = 1.0f;
    [SerializeField] private float stepHeight = 0.5f;
    [SerializeField] private CustomAnimationCurve_Collection curves;
    [SerializeField] private CustomAnimationCurveType stepAnimCurveType;
    [Space]
    [SerializeField] private WalkingStyle walkingStyle;
    [Header("Head")]
    [SerializeField] Transform headBone;

    // Start is called before the first frame update
    void Awake()
    {
        AnimationCurve generalCurve = curves.GetAnimationCurve(stepAnimCurveType);

        foreach (FabrikIK ik in ikControllers)
        {
            ik.SetupIKController(stepSpeed, maxStepDuration, stepHeight, generalCurve);
        }

        switch (walkingStyle)
        {
            case WalkingStyle.Biped:
                break;
            case WalkingStyle.Triped:
                break;
            case WalkingStyle.Quadriped:
                SetupLegPairs(ikControllers[0], ikControllers[1]);
                SetupLegPairs(ikControllers[2], ikControllers[3]);
                break;
        }
    }


    private void Update()
    {
        AngleRootbone();
        AngleHead();
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


    private void SetupLegPairs(FabrikIK boneA, FabrikIK boneB)
    {
        boneA.SetOpposingBone(boneB);
        boneB.SetOpposingBone(boneA);
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
            targetRotation = Quaternion.LookRotation(rootBone.forward);
        }

        rootBone.rotation = Quaternion.Lerp(rootBone.rotation, Quaternion.LookRotation(averageVector), bodyRotationSpeed * Time.deltaTime);
    }


    private void AngleHead()
    {
        if (followPath == null)
            return;

        Vector3 adjustedLookDirection = new Vector3(followPath.ToNextWaypoint.x, 0, followPath.ToNextWaypoint.z);

        Quaternion targetRotation = Quaternion.LookRotation(Vector3.down, adjustedLookDirection);

        headBone.transform.rotation = Quaternion.Slerp(headBone.transform.rotation, targetRotation, bodyRotationSpeed * Time.deltaTime);
    }
}
