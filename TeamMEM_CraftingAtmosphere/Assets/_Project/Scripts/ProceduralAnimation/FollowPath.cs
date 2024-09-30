using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : MonoBehaviour
{
    [SerializeField] List<WaypathPoint> waypoints;

    private bool _shouldMove = false;

    private void Update()
    {
        if (!_shouldMove) return;

        print(DistanceXZ(transform.position, waypoints[0].transform.position));
    }


    public void SetShouldMove(bool shouldMove)
    {
        _shouldMove = shouldMove;
    }


    private float DistanceXZ(Vector3 pointA, Vector3 pointB)
    {
        Vector2 pointA_XZ = new Vector2(pointA.x, pointA.z);
        Vector2 pointB_XZ = new Vector2(pointB.x, pointB.z);
        return Vector2.Distance(pointA_XZ, pointB_XZ);
    }
}
