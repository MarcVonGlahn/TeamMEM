using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootPlacement : MonoBehaviour
{
    private Vector3 m_initFootPlacement;

    private void Start()
    {
        m_initFootPlacement = transform.position;
    }

    private void Update()
    {
        transform.SetPositionAndRotation(m_initFootPlacement, Quaternion.identity);
    }
}
