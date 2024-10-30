using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSceneryChange : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] bool isDebug = false;

    private Material _cubeMaterial;
    private MeshRenderer _meshRenderer;



    [Header("Properties")]
    [SerializeField] SO_SceneryProperties sceneryProperties;


    private Light _directionalLight;




    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _cubeMaterial = new Material(_meshRenderer.material);

        _meshRenderer.material = _cubeMaterial;
        _cubeMaterial.color = sceneryProperties.TriggerColor;

        _directionalLight = FindObjectOfType<Light>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;

        Debug.Log("Player triggered Scenery Change to this: " + sceneryProperties.SceneryName);
    }
}
