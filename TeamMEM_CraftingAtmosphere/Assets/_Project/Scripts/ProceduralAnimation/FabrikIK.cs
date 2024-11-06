using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.XR;

public class FabrikIK : MonoBehaviour
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
    [SerializeField] private bool smoothRotation = true;
    [Space]
    [SerializeField] private float pullWeight = 0.5f;
    [Space]
    [Header("Raycasting")]
    [SerializeField] private LayerMask floorLayerMask;
    [SerializeField] private float targetLegHeight = 0.4f;
    [SerializeField] private float footFloatingThreshold = 0.1f;
    [Header("Step Orientation")]
    [SerializeField] private Transform footTarget;
    

    private AnimationCurve _stepMakeCurve;
    private AnimationCurve _stepHeightAnimCurve;

    private bool _isDoingWalkingAnim;
    private bool _isInCooldown;

    private float _maxLegExtension;
    private float _stepDuration = 1.0f;
    private float _stepHeight = 0.5f;
    private float _stepLengthThreshold = 1f;
    private float _onMoveLegFinishedHeight = 0f;
    private float _cooldownTime = 0.2f;

    private RaycastHit raycastHitInfo_IK;
    private RaycastHit raycastHitInfo_Target;

    Vector3 _plantLegPos;
    Vector3 _plantLegTargetPos;
    Vector3 _targetLegPos;

    private float _distanceFromPlantedLeg = 0;

    private FabrikIK _opposingBone;
    private FabrikIK _sameSideBone;

    private List<Transform> _pullTransforms;
    private List<float> _initHeightDifferencePullTransform;


    public Vector3 GetPlantLegTargetPosition()
    {
        return _plantLegTargetPos;
    }


    public void SetOpposingBone(FabrikIK opposingBone)
    {
        _opposingBone = opposingBone;
    }

    public void SetSameSideBone(FabrikIK sameSideBone)
    {
        _sameSideBone = sameSideBone;
    }


    private void Start()
    {
        RaycastPlantLegTarget(true);

        _plantLegPos = _plantLegTargetPos;

        // Init the max leg Extension for this chain of bones
        for (int i = 0; i < ikBones.Count - 1; i++)
        {
            _maxLegExtension += Vector3.Distance(ikBones[i].boneTransform.position, ikBones[i + 1].boneTransform.position);
        }


        // Init the pull Transform list with all valid pull transforms and the corresponding height differences measured from the floor
        _pullTransforms = new List<Transform>();
        _initHeightDifferencePullTransform = new List<float>();
        foreach(IKBone iKBone in ikBones)
        {
            if (!iKBone.affectedByPull || iKBone.pullTransform == null)
                continue;

            _pullTransforms.Add(iKBone.pullTransform);

            _initHeightDifferencePullTransform.Add(iKBone.pullTransform.localPosition.y);
        }
    }


    private void Update()
    {
        // Lock Target Position to Height above ground.
        // target.position = new Vector3(target.position.x, targetHeight, target.position.z);

        // LogMessage($"Distance from target leg position (magenta) to PlantLegTarget (green): {Vector3.Distance(_plantLegTargetPos, _targetLegPos)}");
        // LogMessage($"PlantLegTarget (green): {_plantLegTargetPos}");
    }


    void LateUpdate()
    {
        RaycastStepCheckTarget();

        if (ShouldDoStep())
        {
            // LogMessage($"Should Do Step\n");
            RaycastPlantLegTarget();
            _isDoingWalkingAnim = true;

            StartCoroutine(MoveLeg_Routine());
        }

        if (IsFootFloating())
        {
            RaycastPlantLegTarget();
            _isDoingWalkingAnim = true;

            StartCoroutine(MoveLeg_Routine());

            //LogMessage($"{transform.name}: Step triggered from \"IsFootFloating\"-Method\n" +
            //        $"Foot Height:\t\t{footBone.position.y}\n" +
            //        $"Leg Height:\t\t{ikBones[ikBones.Count - 1].boneTransform.position.y}\n" +
            //        $"Plant Target Height:\t{_plantLegTargetPos.y}\n" +
            //        $"Floating Threshold:\t{footFloatingThreshold}");
        }

        SolveIK();
    }



    #region public Getters and Setters
    public void SetupIKController(float stepDuration, float stepHeight, float stepLengthThreshold, AnimationCurve stepMakeAnimationCurve,AnimationCurve stepHeightAnimationCurve)
    {
        _stepDuration = stepDuration;
        _stepHeight = stepHeight;
        _stepLengthThreshold = stepLengthThreshold;
        _stepMakeCurve = stepMakeAnimationCurve;
        _stepHeightAnimCurve = stepHeightAnimationCurve;
    }
    #endregion


    private void RaycastPlantLegTarget(bool isOnStart = false)
    {
        target.position = new Vector3(_targetLegPos.x, targetHeight + _targetLegPos.y, _targetLegPos.z);

        Ray ray = new Ray(target.position, Vector3.down);
        if(Physics.Raycast(ray, out raycastHitInfo_IK, 30f, floorLayerMask))
        {
            _plantLegTargetPos = raycastHitInfo_IK.point;

            _plantLegTargetPos = _plantLegTargetPos + raycastHitInfo_IK.normal.normalized * targetLegHeight;
        }
    }



    private void RaycastStepCheckTarget()
    {
        // Raycast for the step target
        Ray ray = new Ray(footTarget.position, Vector3.down);

        // Raycast distance 10 to ensure it's always hitting the floor
        if (Physics.Raycast(ray, out raycastHitInfo_Target, 10f, floorLayerMask))
        {
            _targetLegPos = raycastHitInfo_Target.point;
            _distanceFromPlantedLeg = (_plantLegTargetPos - _targetLegPos).sqrMagnitude;
        }
    }


    bool ShouldDoStep()
    {
        bool isLegFullyExtended = Vector3.Distance(ikBones[0].boneTransform.position, ikBones[ikBones.Count - 1].boneTransform.position) >= _maxLegExtension - 0.1f;

        bool isStepThresholdMet = _distanceFromPlantedLeg >= _stepLengthThreshold * _stepLengthThreshold;

        bool isOpposingLegDoingAnim = _opposingBone._isDoingWalkingAnim;

        bool isSameSideLegDoingAnim = true;

        if (_sameSideBone != null)
        {
            isSameSideLegDoingAnim = _sameSideBone._isDoingWalkingAnim;
        }

        //LogMessage($"Should Do Step based on Conditions: {(isLegFullyExtended || isStepThresholdMet) && !_isDoingWalkingAnim && !isOpposingLegDoingAnim && !isSameSideLegDoingAnim && !_isInCooldown}\n" +
        //    $"Is Leg fully extended: {isLegFullyExtended} OR is Step Threshold Met: {isStepThresholdMet}\n" +
        //    $"AND Is Opposing Leg NOT Doing Anim: {!isOpposingLegDoingAnim}\n" +
        //    $"AND Is Same Side Leg NOT Doing Anim: {!isSameSideLegDoingAnim}\n" +
        //    $"AND is not in Cooldown: {!_isInCooldown}");


        return (isLegFullyExtended || isStepThresholdMet) && !_isDoingWalkingAnim && !isOpposingLegDoingAnim && !isSameSideLegDoingAnim && !_isInCooldown;
    }



    private bool IsFootFloating()
    {
        if (_isDoingWalkingAnim)
            return false;

        if (_opposingBone._isDoingWalkingAnim)
            return false;

        if (_sameSideBone != null &&_sameSideBone._isDoingWalkingAnim)
            return false;


        if (Mathf.Abs(ikBones[ikBones.Count - 1].boneTransform.position.y - _onMoveLegFinishedHeight) > footFloatingThreshold)
        {
            //LogMessage($"Is Foot Floating\n" +
            //    $"Foot Height difference to plant leg height is bigger than threshold: {Mathf.Abs(ikBones[ikBones.Count - 1].boneTransform.position.y - _onMoveLegFinishedHeight) > footFloatingThreshold}\n" +
            //    $"Bone Height: {ikBones[ikBones.Count - 1].boneTransform.position.y}\n" +
            //    $"On Move Leg Start Height: {_onMoveLegFinishedHeight}");
            return true;
        }
        return false;
    }


    private IEnumerator MoveLeg_Routine()
    {
        // Add a small random waiting period, to avoid perfectly synchronous steps, which looks very unnatural
        float randWait = Random.Range(0, 13);
        randWait = randWait * 0.01f;
        yield return new WaitForSecondsRealtime(randWait);

        // We ground the current and the target leg position, to get an accurate representation of "Step Progress" in essentially 2D, as in how much longer until we have reached the desired leg position, not taking height into account.
        Vector3 groundedCurrentLegPos = _plantLegPos;


        // Actual step loop
        float timer = 0f;
        while (timer < _stepDuration)
        {
            Vector3 newPlantPos = Vector3.Lerp(groundedCurrentLegPos, _plantLegTargetPos, _stepMakeCurve.Evaluate(timer / _stepDuration));

            float evaluatedStepHeight = _stepHeightAnimCurve.Evaluate(timer / _stepDuration) * _stepHeight;

            newPlantPos.y = _plantLegTargetPos.y + evaluatedStepHeight;

            _plantLegPos = newPlantPos;

            // Take care of correctly positioning the pull transforms
            for (int i = 0; i < _pullTransforms.Count; i++)
            {
                // Add the initial height from the floor of the pull transform multiplied by the respective height affect to the current plantLegPos

                Vector3 temp = _pullTransforms[i].position;

                temp.y = _plantLegPos.y * ikBones[i].pullTransformHeightAffect + _initHeightDifferencePullTransform[i];

                _pullTransforms[i].position = temp;
            }

            timer += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        //Finally set _plantLegPos to target position once the threshold is met. Also set DoingWalkingAnim to false.
        _plantLegPos = _plantLegTargetPos;
        _isDoingWalkingAnim = false;

        // This is for "IsFootFloating" Check
        _onMoveLegFinishedHeight = ikBones[ikBones.Count - 1].boneTransform.position.y;

        // Have a cooldown to avoid another instant call of move leg method after finish
        _isInCooldown = true;
        yield return new WaitForSeconds(_cooldownTime);
        _isInCooldown = false;

        yield return null;
    }


    #region IK Solver
    void SolveIK()
    {
        if (ikBones.Count == 0 || target == null)
            return;

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

                if (ikBone.affectedByPull)
                    ApplyPullForce(ikBone);

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


    private void ApplyPullForce(IKBone bone)
    {
        if (bone.pullTransform == null || pullWeight == 0)
            return;

        // Get the target rotation from the pullTransform
        Vector3 toPullTarget = bone.pullTransform.position - bone.boneTransform.position;
        Quaternion pullRotation = Quaternion.FromToRotation(bone.boneTransform.up, toPullTarget);

        // Blend the current rotation with the pull rotation based on the weight
        bone.boneTransform.rotation = Quaternion.Slerp(bone.boneTransform.rotation, pullRotation * bone.boneTransform.rotation, pullWeight);
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

        // Lerp the Euler Angles to avoid Rotation Bullshit
        if (smoothRotation)
        {
            Vector3 newRotation = ikBone.boneTransform.localRotation.eulerAngles;
            newRotation = Vector3.MoveTowards(newRotation, currentEulerAngles, smoothFactor * Time.deltaTime);

            Quaternion constrainedRotation = Quaternion.Euler(newRotation);

            // Convert back to quaternion and apply to the bone
            ikBone.boneTransform.localRotation = constrainedRotation;// Quaternion.Lerp(ikBone.boneTransform.localRotation, constrainedRotation, smoothFactor * Time.deltaTime);
        }
        else
        {
            ikBone.boneTransform.localRotation = Quaternion.Euler(currentEulerAngles);
        }
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


        //if (_isDoingWalkingAnim)
        //    Gizmos.color = Color.green;
        //else
        //    Gizmos.color = Color.grey;

        //Gizmos.DrawSphere(raycastHitInfo_Target.point, 0.3f);

        // return;

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(transform.position, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(footTarget.position, 0.2f);


        Gizmos.color = Color.red;
        Gizmos.DrawSphere(raycastHitInfo_Target.point, 0.2f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_plantLegTargetPos, 0.15f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(raycastHitInfo_IK.point, raycastHitInfo_IK.point + raycastHitInfo_IK.normal);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(_plantLegPos, 0.2f);

        if (_pullTransforms == null)
            return;

        Gizmos.color = Color.magenta;

        foreach (var t in _pullTransforms)
        {
            Gizmos.DrawSphere(t.position, 0.2f);
        }
    }
    #endregion



    private void LogMessage(string message)
    {
        if (!showLog)
            return;

        Debug.Log($"{transform.name}\n" + message);
    }
}