using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : MonoBehaviour
{
    [SerializeField] List<WaypathPoint> waypoints;
    [SerializeField] float waypointThreshold = 0.2f;

    private bool _shouldMove = false;

    private int _curWPIndex = 0;

    private float _walkingSpeed = 0;

    private Vector3 _toNextWaypoint;

    public Vector3 ToNextWaypoint { get => _toNextWaypoint; }

    private void Start()
    {
        _toNextWaypoint = waypoints[0].transform.position - transform.position;
    }

    private void Update()
    {
        if (!_shouldMove) return;

        // Debug.Log($"{DistanceXZ(transform.position, waypoints[_curWPIndex].transform.position)}, {IsInFront(transform.position, waypoints[_curWPIndex].transform.position, transform.forward)}");

        if (DistanceXZ(transform.position, waypoints[_curWPIndex].transform.position) < waypointThreshold)
        {
            _curWPIndex++;

            if (_curWPIndex >= waypoints.Count)
                _curWPIndex = 0;
        }

        transform.position = Vector3.MoveTowards(transform.position, waypoints[_curWPIndex].transform.position, _walkingSpeed * Time.deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation((waypoints[_curWPIndex].transform.position - transform.position).normalized);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 30f * Time.deltaTime);
    }


    public void SetShouldMove(bool shouldMove)
    {
        _shouldMove = shouldMove;
    }

    public void SetWalkingSpeed(float walkingSpeed)
    {
        _walkingSpeed = walkingSpeed;
    }


    private float DistanceXZ(Vector3 pointA, Vector3 pointB)
    {
        Vector2 pointA_XZ = new Vector2(pointA.x, pointA.z);
        Vector2 pointB_XZ = new Vector2(pointB.x, pointB.z);
        return Vector2.Distance(pointA_XZ, pointB_XZ);
    }


    private bool IsInFront(Vector3 pointA, Vector3 pointB, Vector3 forwardDirection)
    {
        Vector3 directionToPointB = pointB - pointA; // Vector from pointA to pointB
        float dotProduct = Vector3.Dot(forwardDirection.normalized, directionToPointB.normalized);

        return dotProduct > 0; // Returns true if pointB is in front of pointA
    }
}