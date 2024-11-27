using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleportation_Trigger : MonoBehaviour
{
    [SerializeField] Transform teleportLocation;

    private Collider _collider;


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;

        Debug.Log("Teleport the thing");

        other.GetComponent<CharacterController>().enabled = false;
        other.transform.position = teleportLocation.position;
        other.GetComponent<CharacterController>().enabled = true;
    }
}
