using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class ProceduralAnimationController : MonoBehaviour
{
    [Header("Leg Controllers")]
    [SerializeField] List<FabrikIK> ikControllers;
    [Header("Additional Bones")]
    [SerializeField] Transform rootBone;
    [Header("Animation Settings")]
    [SerializeField] SO_Moveset moveset;
    [SerializeField] FollowPath followPath;
    [Header("Head")]
    [SerializeField] Transform headBone;


    private float _rootBoneHeightAdjust = 0f;
    private float _rootBoneTimer = 0f;

    void Awake()
    {
        foreach (FabrikIK ik in ikControllers)
        {
            ik.SetupIKController(moveset.StepDuration, moveset.StepHeight, moveset.StepLengthTreshhold, moveset.StepMakeCurve, moveset.StepHeightCurve);
        }

        switch (moveset.WalkingStyle)
        {
            case WalkingStyle.Biped:
                break;
            case WalkingStyle.Triped:
                break;
            case WalkingStyle.Quadriped:
                SetupLegPairs(ikControllers[0], ikControllers[1]);
                SetupLegPairs(ikControllers[2], ikControllers[3]);
                SetupSameSideLegs(ikControllers[0], ikControllers[2]);
                SetupSameSideLegs(ikControllers[1], ikControllers[3]);
                break;
        }
    }


    private void Update()
    {
        RootBoneHeightAdjustment();
        AngleRootbone();
        AngleHead();
    }


    private void RootBoneHeightAdjustment()
    {
        _rootBoneHeightAdjust = moveset.GetRootHeightAdjust(_rootBoneTimer);

        _rootBoneTimer += Time.deltaTime;

        if (_rootBoneTimer > moveset.RootLevelChangeDuration)
            _rootBoneTimer = 0;

    }


    private void AngleRootbone()
    {
        switch (moveset.WalkingStyle)
        {
            case WalkingStyle.Biped:
                break;
            case WalkingStyle.Triped:
                break;
            case WalkingStyle.Quadriped:
                QuadripedPositionRootbone();
                QuadripedAngleRootbone();
                break;
        }
    }

    private void SetupLegPairs(FabrikIK boneA, FabrikIK boneB)
    {
        boneA.SetOpposingBone(boneB);
        boneB.SetOpposingBone(boneA);
    }


    private void SetupSameSideLegs(FabrikIK boneA, FabrikIK boneB)
    {
        boneA.SetSameSideBone(boneB);
        boneB.SetSameSideBone(boneA);
    }



    private void QuadripedPositionRootbone()
    {
        float averageLegBoneHeight = 0;

        foreach (var ik in ikControllers)
        {
            averageLegBoneHeight += ik.GetPlantLegTargetPosition().y;
        }

        averageLegBoneHeight /= 4;

        Vector3 newRootBonePos = new Vector3(transform.position.x, rootBone.transform.position.y, transform.position.z);

        newRootBonePos.y = averageLegBoneHeight + moveset.RootBoneHeight + _rootBoneHeightAdjust;

        rootBone.transform.position = Vector3.MoveTowards(rootBone.transform.position, newRootBonePos, Time.deltaTime * 5f);
    }


    private void QuadripedAngleRootbone()
    {
        rootBone.rotation = transform.rotation;
        return;

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
            Debug.Log("catch");
        }

        rootBone.rotation = Quaternion.Lerp(rootBone.rotation, Quaternion.LookRotation(averageVector), moveset.BodyRotationSpeed * Time.deltaTime);
    }


    private void AngleHead()
    {
        if (headBone == null)
            return;

        if (followPath == null)
            return;

        Vector3 adjustedLookDirection = new Vector3(followPath.ToNextWaypoint.x, 0, followPath.ToNextWaypoint.z);

        Quaternion targetRotation = Quaternion.LookRotation(Vector3.down, adjustedLookDirection);

        headBone.transform.rotation = Quaternion.Slerp(headBone.transform.rotation, targetRotation, moveset.HeadRotationSpeed * Time.deltaTime);
    }


    private void OnDrawGizmos()
    {
        if(rootBone == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(rootBone.transform.position, rootBone.transform.position + rootBone.transform.forward * 3f);
    }
}
