using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class ProceduralAnimationController : MonoBehaviour
{
    [Header("Leg Controllers")]
    [SerializeField] List<FabrikIK> ikControllers;
    [Header("Rootbone Settings")]
    [SerializeField] Transform rootBone;
    [SerializeField] LayerMask floorLayerMask;
    [Header("Animation Settings")]
    [SerializeField] CreatureControl creatureControl;
    [SerializeField] FollowPath followPath;
    [Header("Head")]
    [SerializeField] Transform headBone;


    private float _rootBoneHeightAdjust = 0f;
    private float _rootBoneTimer = 0f;


    private Vector3 _floorHitRootbone = Vector3.zero;

    private Quaternion _initRootboneRotation = Quaternion.identity;

    void Awake()
    {
        foreach (FabrikIK ik in ikControllers)
        {
            ik.SetupIKController(creatureControl.GetMoveset().StepDuration, creatureControl.GetMoveset().StepHeight, creatureControl.GetMoveset().StepLengthTreshhold, creatureControl.GetMoveset().StepMakeCurve, creatureControl.GetMoveset().StepHeightCurve);
        }

        switch (creatureControl.GetMoveset().WalkingStyle)
        {
            case WalkingStyle.Biped:
                SetupLegPairs(ikControllers[0], ikControllers[1]);
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

        _initRootboneRotation = rootBone.rotation;
    }


    private void Update()
    {
        RootBoneHeightAdjustment();
        AngleAndPositionRootbone();
        AngleHead();
    }


    private void RootBoneHeightAdjustment()
    {
        _rootBoneHeightAdjust = creatureControl.GetMoveset().GetRootHeightAdjust(_rootBoneTimer);

        _rootBoneTimer += Time.deltaTime;

        if (_rootBoneTimer > creatureControl.GetMoveset().RootLevelChangeDuration)
            _rootBoneTimer = 0;

    }


    private void AngleAndPositionRootbone()
    {
        PositionRootbone();
        AngleRootbone();
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



    private void PositionRootbone()
    {
        // Raycast down, and position rootbone straight up from there
        Ray ray = new Ray(rootBone.transform.position, Vector3.down);

        if(Physics.Raycast(ray, out RaycastHit hitInfo, 10f, floorLayerMask))
        {
            // Debugging Purposes to draw a Gizmo
            _floorHitRootbone = hitInfo.point;

            // Init the newRootBone Position
            Vector3 newRootBonePos = transform.position;

            // Set the height of the transform relative to the raycasted floor, keeps all child transforms in correct height as well
            newRootBonePos.y = hitInfo.point.y;
            transform.position = newRootBonePos;

            // Smoothly move the rootbone to desired height, also account for rootbone movement animation
            newRootBonePos.y = hitInfo.point.y + creatureControl.GetMoveset().RootBoneHeight + _rootBoneHeightAdjust;
            rootBone.transform.position = Vector3.MoveTowards(rootBone.transform.position, newRootBonePos, Time.deltaTime * 5f);
        }
    }


    private void AngleRootbone()
    {
        // Rotation is taken care of in "FollowPath"-Script
        // Rootbone copies the transform rotation, which doesn't account for rotating "up" or "down"
        // rootBone.rotation = _initRootboneRotation * transform.rotation;
        return;
    }


    private void AngleHead()
    {
        if (headBone == null)
            return;

        if (followPath == null)
            return;

        Vector3 adjustedLookDirection = new Vector3(followPath.ToNextWaypoint.x, 0, followPath.ToNextWaypoint.z);

        Quaternion targetRotation = Quaternion.LookRotation(Vector3.down, adjustedLookDirection);

        headBone.transform.rotation = Quaternion.Slerp(headBone.transform.rotation, targetRotation, creatureControl.GetMoveset().HeadRotationSpeed * Time.deltaTime);
    }


    private void OnDrawGizmos()
    {
        if(rootBone == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(rootBone.transform.position, rootBone.transform.position + rootBone.transform.forward * 3f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(rootBone.transform.position, rootBone.transform.position + Vector3.down * 10f);
        Gizmos.DrawSphere(_floorHitRootbone, 0.2f);
    }
}
