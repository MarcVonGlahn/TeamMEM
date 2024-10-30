using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneryProperties_", menuName = "Custom/Custom Scenery Properties")]
public class SO_SceneryProperties : ScriptableObject
{
    [Header("Debug Stuff")]
    public Color TriggerColor = Color.white;

    [Header("Properties")]
    public string SceneryName = "ExampleName";
    [Space]
    public Color DirectionalLightColor = Color.white;
    public float DirectionalLightIntensity = 3.0f;
    [Space]
    public Material SkyboxMaterial;
}
