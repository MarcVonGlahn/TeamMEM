using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiBoneIK : MonoBehaviour
{
    public Transform target; // The target the IK should try to reach
    public Transform[] bones; // Array of bones in the kinematic chain
    public int iterations = 10; // Number of iterations for refining the solution
    public float tolerance = 0.01f; // How close to the target is considered "good enough"

    void LateUpdate()
    {
        SolveIK();
    }

    void SolveIK()
    {
        if (bones.Length == 0 || target == null)
            return;

        for (int i = 0; i < iterations; i++)
        {
            for (int j = bones.Length - 1; j >= 0; j--)
            {
                Transform bone = bones[j];

                // Calculate the vector from the bone to the end effector
                Vector3 toEndEffector = bones[bones.Length - 1].position - bone.position;
                // Calculate the vector from the bone to the target
                Vector3 toTarget = target.position - bone.position;

                // Calculate the rotation to get from the end effector to the target
                Quaternion targetRotation = Quaternion.FromToRotation(toEndEffector, toTarget);
                bone.rotation = targetRotation * bone.rotation;

                // Check if the end effector is close enough to the target
                if ((bones[bones.Length - 1].position - target.position).sqrMagnitude < tolerance * tolerance)
                {
                    return;
                }
            }
        }
    }
}