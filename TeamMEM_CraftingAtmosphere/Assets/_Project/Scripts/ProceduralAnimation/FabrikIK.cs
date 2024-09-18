using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.XR;

public class MultiBoneIK : MonoBehaviour
{
    public bool showLog = false;
    public bool showGizmo = true;
    [Header("Foot Control Properties")]
    [SerializeField] private Transform target; // The target the IK should try to reach
    [SerializeField] private float targetHeight = 2f;
    [Header("IK Properties")]
    [SerializeField] private Transform[] bones; // Array of bones in the kinematic chain
    [SerializeField] private Transform footBone;
    [SerializeField] private int iterations = 10; // Number of iterations for refining the solution
    [SerializeField] private float tolerance = 0.01f; // How close to the target is considered "good enough"
    [Space]
    [Header("Raycasting")]
    [SerializeField] private LayerMask floorLayerMask;
    [SerializeField] private float targetLegHeight = 0.4f;
    [Header("Step Orientation")]
    [SerializeField] private Transform footTarget;
    [SerializeField] private float stepLengthThreshold = 1f;
    [Header("Animation Properties")]
    [Tooltip("The lower the step speed, the faster the step")]
    [SerializeField] private float stepSpeed = 0.05f;
    [SerializeField] private float stepHeight = 0.5f;
    [SerializeField] private CustomAnimationCurve_Collection curves;
    [SerializeField] private CustomAnimationCurveType stepAnimCurveType;


    private bool _isDoingWalkingAnim;

    private float _maxLegExtension;

    private RaycastHit raycastHitInfo_IK;
    private RaycastHit raycastHitInfo_Target;

    Vector3 _plantLegPos;
    Vector3 _plantLegTargetPos;
    Vector3 _targetLegPos;

    private float _distanceFromPlantedLeg = 0;


    private void Start()
    {
        RaycastPlantLegTarget(true);

        _plantLegPos = _plantLegTargetPos;

        _maxLegExtension = Vector3.Distance(bones[0].position, bones[1].position)
            + Vector3.Distance(bones[1].position, bones[2].position);
    }


    private void Update()
    {
        // Lock Target Position to Height above ground.
        target.position = new Vector3(target.position.x, targetHeight, target.position.z);
    }


    void LateUpdate()
    {
        RaycastStepCheckTarget();

        if (ShouldDoStep())
        {
            RaycastPlantLegTarget();
            StartCoroutine(MoveLeg_Routine());
        }

        SolveIK();
    }


    private void RaycastPlantLegTarget(bool isOnStart = false)
    {
        target.position = new Vector3(_targetLegPos.x, targetHeight, _targetLegPos.z);

        Ray ray = new Ray(target.position, Vector3.down);
        if(Physics.Raycast(ray, out raycastHitInfo_IK, 5f, floorLayerMask))
        {
            _plantLegTargetPos = raycastHitInfo_IK.point;

            _plantLegTargetPos = _plantLegTargetPos + raycastHitInfo_IK.normal.normalized * targetLegHeight;
        }

        if(!isOnStart) _isDoingWalkingAnim = true;
    }



    private void RaycastStepCheckTarget()
    {
        // Raycast for the step target
        Ray ray = new Ray(footTarget.position, Vector3.down);
        if (Physics.Raycast(ray, out raycastHitInfo_Target, 5f, floorLayerMask))
        {
            _targetLegPos = raycastHitInfo_Target.point;
            _distanceFromPlantedLeg = (_plantLegPos - _targetLegPos).sqrMagnitude;
        }
    }


    bool ShouldDoStep()
    {
        bool isLegFullyExtended =Vector3.Distance(bones[0].position, bones[bones.Length - 1].position) >= _maxLegExtension - 0.1f;

        bool isStepThresholdMet = _distanceFromPlantedLeg >= stepLengthThreshold;

        return (isLegFullyExtended || isStepThresholdMet) && !_isDoingWalkingAnim;
    }


    private IEnumerator MoveLeg_Routine()
    {
        // We ground the current and the target leg position, to get an accurate representation of "Step Progress" in essentially 2D, as in how much longer until we have reached the desired leg position, not taking height into account.
        Vector2 groundedCurrentLegPos = new Vector2(_plantLegPos.x, _plantLegPos.z);
        Vector2 groundedTargetPos = new Vector2(_plantLegTargetPos.x, _plantLegTargetPos.z);

        // Distance Measurements
        float currentDistanceToTarget = Vector3.Distance(groundedCurrentLegPos, groundedTargetPos);
        float distanceOnStepBegin = currentDistanceToTarget;

        while (currentDistanceToTarget >= 0.01f)
        {
            // We use the MoveTowards method to dynamically move the leg towards the target positon ...
            Vector3 newPlantPos = Vector3.MoveTowards(_plantLegPos, _plantLegTargetPos, stepSpeed);

            // ... while an Animation Curve controls how the step works height wise over distance.
            newPlantPos.y = _plantLegTargetPos.y + curves.GetAnimationCurve(stepAnimCurveType).Evaluate(1 - currentDistanceToTarget / distanceOnStepBegin) * stepHeight;

            _plantLegPos = newPlantPos;

            // Then adjust current 2D Leg Position and distance to target
            groundedCurrentLegPos = new Vector2(_plantLegPos.x, _plantLegPos.z);
            currentDistanceToTarget = Vector3.Distance(groundedCurrentLegPos, groundedTargetPos);

            yield return new WaitForEndOfFrame();
        }
        //Finally set _plantLegPos to target position once the threshold is met. Also set DoingWalkingAnim to false.
        _plantLegPos = _plantLegTargetPos;
        _isDoingWalkingAnim = false;

        yield return null;
    }


    #region IK Solver
    void SolveIK()
    {
        if (bones.Length == 0 || target == null)
            return;

        Vector3 tposition = target.position;

        bool breakSolving = false;

        for (int i = 0; i < iterations; i++)
        {
            for (int j = bones.Length - 1; j >= 0; j--)
            {
                Transform bone = bones[j];

                // Calculate the vector from the bone to the end effector
                Vector3 toEndEffector = bones[bones.Length - 1].position - bone.position;
                // Calculate the vector from the bone to the target
                Vector3 toTarget = _plantLegPos - bone.position;

                // Calculate the rotation to get from the end effector to the target
                Quaternion targetRotation = Quaternion.FromToRotation(toEndEffector, toTarget);

                bone.rotation = targetRotation * bone.rotation; // Multiplying 2 Quaternions results in a composition (Apply Quaternion A, and then B, without Gimbal lock danger).

                // Check if the end effector is close enough to the target
                if ((bones[bones.Length - 1].position - _plantLegPos).sqrMagnitude < tolerance * tolerance)
                {
                    breakSolving = true;
                    break;
                }
            }
            if (breakSolving) break;
        }

        // Rotate the lowest bone one last time

        // Calculate the vector from the bone to the end effector
        Vector3 toendEffector = footBone.position - bones[bones.Length - 1].position;
        // Calculate the vector from the bone to the target
        Vector3 totarget = raycastHitInfo_IK.point - bones[bones.Length - 1].position;

        // Calculate the rotation to get from the end effector to the target
        Quaternion targetrotation = Quaternion.FromToRotation(toendEffector, totarget);
        bones[bones.Length - 1].rotation = targetrotation * bones[bones.Length - 1].rotation; // Multiplying 2 Quaternions results in a composition (Apply Quaternion A, and then B, without Gimbal lock danger).

    }
    #endregion


    #region Gizmos
    private void OnDrawGizmos()
    {
        if (!showGizmo)
            return;

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(transform.position, 0.2f);


        Gizmos.color = Color.red;
        Gizmos.DrawSphere(raycastHitInfo_Target.point, 0.2f);

        //Gizmos.color = Color.green;
        //Gizmos.DrawSphere(raycastHitInfo.point, 0.15f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(raycastHitInfo_IK.point, raycastHitInfo_IK.point + raycastHitInfo_IK.normal);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(_plantLegPos, 0.2f);
    }
    #endregion
}