using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiBoneIK : MonoBehaviour
{
    public bool showLog = false;
    [SerializeField] private Transform target; // The target the IK should try to reach
    [SerializeField] private Transform[] bones; // Array of bones in the kinematic chain
    [SerializeField] private int iterations = 10; // Number of iterations for refining the solution
    [SerializeField] private float tolerance = 0.01f; // How close to the target is considered "good enough"
    [Space]
    [Header("Body")]
    [SerializeField] private Transform body_target;
    [Header("Raycasting")]
    [SerializeField] private LayerMask floorLayerMask;



    private RaycastHit raycastHitInfo;

    private Vector3 _lowerLegTargetPos;

    void LateUpdate()
    {
        RaycastTarget();

        SolveIK();
    }


    private void RaycastTarget()
    {
        if (body_target == null) return;

        Ray ray = new Ray(body_target.position, Vector3.down);
        if(Physics.Raycast(ray, out raycastHitInfo, 5f))
        {

        }
    }


    void SolveIK()
    {
        if (bones.Length == 0 || target == null)
            return;

        for (int i = 0; i < iterations; i++)
        {
            for (int j = bones.Length - 2; j >= 0; j--)
            {
                Transform bone = bones[j];

                // Calculate the vector from the bone to the end effector
                Vector3 toEndEffector = bones[bones.Length - 1].position - bone.position;
                // Calculate the vector from the bone to the target
                Vector3 toTarget = target.position - bone.position;

                // Calculate the rotation to get from the end effector to the target
                Quaternion targetRotation = Quaternion.FromToRotation(toEndEffector, toTarget);

                bone.rotation = targetRotation * bone.rotation; // Multiplying 2 Quaternions results in a composition (Apply Quaternion A, and then B, without Gimbal lock danger).

                // Check if the end effector is close enough to the target
                if ((bones[bones.Length - 1].position - target.position).sqrMagnitude < tolerance * tolerance)
                {
                    return;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(body_target == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(body_target.position, 0.2f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(raycastHitInfo.point, 0.15f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(raycastHitInfo.point, raycastHitInfo.point + raycastHitInfo.normal);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(_lowerLegTargetPos, 0.1f);
    }
}