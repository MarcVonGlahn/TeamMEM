using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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

    [Header("Fog")]
    public bool IsFogEnabled = false;
    public Color FogColor = Color.white;
    public FogMode fogMode = FogMode.ExponentialSquared;
    public float fogDensity = 1.0f;

    [Header("Environment")]
    public Color RealtimeShadowColor = Color.white;
    [Space]
    [Tooltip("DONT CHANGE PLS")]
    public AmbientMode EnvironmentLightingSource = AmbientMode.Trilight;
    [ColorUsage(true, true)]
    public Color SkyColorHDR = Color.white;
    [ColorUsage(true, true)]
    public Color EquatorColorHDR = Color.white;
    [ColorUsage(true, true)]
    public Color GroundColorHDR = Color.white;
    [Space]
    [Range(0.0f, 1.0f)]
    public float ReflectionIntensity = 1.0f;

}
