using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class MultiBoneIK : MonoBehaviour
{
    public bool showLog = false;
    [SerializeField] private Transform target; // The target the IK should try to reach
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
    [SerializeField] private float stepLength;


    private RaycastHit raycastHitInfo_IK;
    private RaycastHit raycastHitInfo_Target;

    Vector3 _plantLegPos;
    Vector3 _targetLegPos;

    private float _distanceFromPlantedLeg = 0;

    void LateUpdate()
    {
        RaycastTarget();

        if (ShouldDoStep())
            Debug.Log(gameObject.name + " is signalling that step should be taken");

        SolveIK();
    }


    private void RaycastTarget()
    {
        // Raycast for IK and planting the foot
        Ray ray = new Ray(target.position, Vector3.down);
        if(Physics.Raycast(ray, out raycastHitInfo_IK, 5f, floorLayerMask))
        {
            // if (showLog) Debug.Log($"Hitting floor");
        }

        // Raycast for the step target
        ray = new Ray(footTarget.position, Vector3.down);
        if(Physics.Raycast(ray,out raycastHitInfo_Target, 5f, floorLayerMask))
        {
            _targetLegPos = raycastHitInfo_Target.point;
            _distanceFromPlantedLeg = (_plantLegPos - _targetLegPos).sqrMagnitude;
        }
    }


    bool ShouldDoStep()
    {
        return (_plantLegPos - _targetLegPos).magnitude >= stepLength;
    }


    void SolveIK()
    {
        if (bones.Length == 0 || target == null)
            return;

        _plantLegPos = raycastHitInfo_IK.point;

        _plantLegPos = _plantLegPos + raycastHitInfo_IK.normal.normalized * targetLegHeight;

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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(raycastHitInfo_Target.point, 0.2f);

        //Gizmos.color = Color.green;
        //Gizmos.DrawSphere(raycastHitInfo.point, 0.15f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(raycastHitInfo_IK.point, raycastHitInfo_IK.point + raycastHitInfo_IK.normal);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(_plantLegPos, 0.2f);
    }
}