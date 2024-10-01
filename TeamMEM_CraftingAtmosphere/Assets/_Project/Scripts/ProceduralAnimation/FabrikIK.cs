using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

public class MultiBoneIK : MonoBehaviour
{
    public bool showLog = false;
    public bool showGizmo = true;
    [Header("Foot Control Properties")]
    [SerializeField] private Transform target; // The target the IK should try to reach
    [SerializeField] private float targetHeight = 2f;
    [Header("IK Properties")]
    [SerializeField] private List<IKBone> ikBones;
    [Space]
    [SerializeField] private Transform footBone;
    [Space]
    [SerializeField] private int iterations = 10; // Number of iterations for refining the solution
    [SerializeField] private float tolerance = 0.1f; // How close to the target is considered "good enough"
    [SerializeField] private float smoothFactor = 0.1f;
    [Space]
    [Header("Raycasting")]
    [SerializeField] private LayerMask floorLayerMask;
    [SerializeField] private float targetLegHeight = 0.4f;
    [Header("Step Orientation")]
    [SerializeField] private Transform footTarget;
    [SerializeField] private float stepLengthThreshold = 1f;
    [Header("Animation Properties")]
    [Tooltip("The lower the step speed, the faster the step")]
    [SerializeField] private CustomAnimationCurve_Collection curves;
    [SerializeField] private CustomAnimationCurveType stepAnimCurveType;

    private AnimationCurve _stepAnimCurve;

    private bool _isDoingWalkingAnim;

    private float _maxLegExtension;
    private float _stepSpeed = 0.05f;
    private float _maxStepDuration = 1.0f;
    private float _stepHeight = 0.5f;

    private RaycastHit raycastHitInfo_IK;
    private RaycastHit raycastHitInfo_Target;

    Vector3 _plantLegPos;
    Vector3 _plantLegTargetPos;
    Vector3 _targetLegPos;

    private float _distanceFromPlantedLeg = 0;


    public Vector3 GetPlantLegTargetPosition()
    {
        return _plantLegTargetPos;
    }




    private void Start()
    {
        RaycastPlantLegTarget(true);

        _plantLegPos = _plantLegTargetPos;

        _maxLegExtension = Vector3.Distance(ikBones[0].boneTransform.position, ikBones[1].boneTransform.position)
            + Vector3.Distance(ikBones[1].boneTransform.position, ikBones[2].boneTransform.position);
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



    #region public Getters and Setters
    public void SetupIKController(float stepSpeed, float maxStepDuration, float stepHeight, AnimationCurve stepAnimationCurve)
    {
        _stepSpeed = stepSpeed;
        _maxStepDuration = maxStepDuration;
        _stepHeight = stepHeight;
        _stepAnimCurve = stepAnimationCurve;
    }
    #endregion


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
        bool isLegFullyExtended = Vector3.Distance(ikBones[0].boneTransform.position, ikBones[ikBones.Count - 1].boneTransform.position) >= _maxLegExtension - 0.1f;

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

        float timer = 0f;

        while (currentDistanceToTarget >= 0.05f || timer <= _maxStepDuration)
        {
            // We use the MoveTowards method to dynamically move the leg towards the target positon ...
            Vector3 newPlantPos = Vector3.MoveTowards(_plantLegPos, _plantLegTargetPos, _stepSpeed);

            // ... while an Animation Curve controls how the step works height wise over distance.
            newPlantPos.y = _plantLegTargetPos.y + curves.GetAnimationCurve(stepAnimCurveType).Evaluate(1 - currentDistanceToTarget / distanceOnStepBegin) * _stepHeight;

            _plantLegPos = newPlantPos;

            // Then adjust current 2D Leg Position and distance to target
            groundedCurrentLegPos = new Vector2(_plantLegPos.x, _plantLegPos.z);
            currentDistanceToTarget = Vector3.Distance(groundedCurrentLegPos, groundedTargetPos);

            timer += Time.deltaTime;

            yield return new WaitForEndOfFrame();


            if (showLog)
            {
                Debug.Log($"Plant Leg Height{_plantLegPos.y}");
            }
        }
        //Finally set _plantLegPos to target position once the threshold is met. Also set DoingWalkingAnim to false.
        _plantLegPos = _plantLegTargetPos;
        
        yield return new WaitForSecondsRealtime(0.2f);
        _isDoingWalkingAnim = false;

        yield return null;
    }


    #region IK Solver
    void SolveIK()
    {
        if (ikBones.Count == 0 || target == null)
            return;

        Vector3 tposition = target.position;

        bool breakSolving = false;

        for (int i = 0; i < iterations; i++)
        {
            for (int j = ikBones.Count - 1; j >= 0; j--)
            {
                IKBone ikBone = ikBones[j];

                // Calculate the vector from the bone to the end effector
                Vector3 toEndEffector = ikBones[ikBones.Count - 1].boneTransform.position - ikBone.boneTransform.position;
                // Calculate the vector from the bone to the target
                Vector3 toTarget = _plantLegPos - ikBone.boneTransform.position;

                Quaternion targetRotation = Quaternion.FromToRotation(toEndEffector, toTarget);

                ikBone.boneTransform.rotation = targetRotation * ikBone.boneTransform.rotation; // Multiplying 2 Quaternions results in a composition (Apply Quaternion A, and then B, without Gimbal lock danger).

                ApplyJointConstraints(ikBone);

                // Check if the end effector is close enough to the target
                if ((ikBones[ikBones.Count - 1].boneTransform.position - _plantLegPos).sqrMagnitude < tolerance * tolerance)
                {
                    breakSolving = true;
                    break;
                }
            }
            if (breakSolving) break;
        }

        // Rotate the lowest bone one last time

        // Calculate the vector from the bone to the end effector
        Vector3 toendEffector = footBone.position - ikBones[ikBones.Count - 1].boneTransform.position;
        // Calculate the vector from the bone to the target
        Vector3 totarget = raycastHitInfo_IK.point - ikBones[ikBones.Count - 1].boneTransform.position;

        // Calculate the rotation to get from the end effector to the target
        Quaternion targetrotation = Quaternion.FromToRotation(toendEffector, totarget);
        ikBones[ikBones.Count - 1].boneTransform.rotation = targetrotation * ikBones[ikBones.Count - 1].boneTransform.rotation; // Multiplying 2 Quaternions results in a composition (Apply Quaternion A, and then B, without Gimbal lock danger).

        ApplyJointConstraints(ikBones[ikBones.Count - 1]);
    }


    // Method to clamp joint rotation within the constraints
    void ApplyJointConstraints(IKBone ikBone)
    {
        // Get the current bone rotation in local space
        Vector3 currentEulerAngles = ikBone.boneTransform.localRotation.eulerAngles;

        // Ensure the angles are in the range of -180 to 180
        currentEulerAngles = NormalizeEulerAngles(currentEulerAngles);

        // Clamp the rotation to the allowed range
        currentEulerAngles.x = Mathf.Clamp(currentEulerAngles.x, ikBone.minRotation.x, ikBone.maxRotation.x);
        currentEulerAngles.y = Mathf.Clamp(currentEulerAngles.y, ikBone.minRotation.y, ikBone.maxRotation.y);
        currentEulerAngles.z = Mathf.Clamp(currentEulerAngles.z, ikBone.minRotation.z, ikBone.maxRotation.z);

        Quaternion constrainedRotation = Quaternion.Euler(currentEulerAngles);

        // Convert back to quaternion and apply to the bone
        ikBone.boneTransform.localRotation = Quaternion.Slerp(ikBone.boneTransform.localRotation, constrainedRotation, smoothFactor);
    }

    // Method to normalize Euler angles to the range of -180 to 180 degrees
    Vector3 NormalizeEulerAngles(Vector3 angles)
    {
        angles.x = NormalizeAngle(angles.x);
        angles.y = NormalizeAngle(angles.y);
        angles.z = NormalizeAngle(angles.z);
        return angles;
    }

    // Normalize an individual angle to the range of -180 to 180 degrees
    float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
    #endregion


    #region Gizmos
    private void OnDrawGizmos()
    {
        if (!showGizmo)
            return;

        //Gizmos.color = Color.black;
        //Gizmos.DrawSphere(transform.position, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(footTarget.position, 0.2f);


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