using DG.Tweening;
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
    [SerializeField] float transitionDuration = 3.0f;


    private Light _directionalLight;




    private void Awake()
    {
        if (isDebug)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _cubeMaterial = new Material(_meshRenderer.material);

            _meshRenderer.material = _cubeMaterial;
            _cubeMaterial.color = sceneryProperties.TriggerColor;
        }
        else
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshRenderer.enabled = false;
        }

        _directionalLight = GameObject.FindGameObjectWithTag("DirectionalLight").GetComponent<Light>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;

        Debug.Log("Player triggered Scenery Change to this: " + sceneryProperties.SceneryName);


        _directionalLight.DOColor(sceneryProperties.DirectionalLightColor, transitionDuration);
        _directionalLight.DOIntensity(sceneryProperties.DirectionalLightIntensity, transitionDuration);


        // Smoothly fade skybox
        StartCoroutine(SkyboxChange_Routine(RenderSettings.skybox.shader.name, sceneryProperties.SkyboxMaterial.shader.name));
    }



    private IEnumerator SkyboxChange_Routine(string currentShaderName, string futureShaderName)
    {
        // If the skybox is procedural, we have to lerp the Exposure
        if (currentShaderName.Contains("Procedural"))
        {
            Material currentSkyboxMaterial = new Material(RenderSettings.skybox);
            RenderSettings.skybox = currentSkyboxMaterial;

            float currentExposure = currentSkyboxMaterial.GetFloat("_Exposure");

            yield return DOTween
                .To(x => currentExposure = x, currentExposure, 0, transitionDuration / 2)
                .OnUpdate(() => currentSkyboxMaterial.SetFloat("_Exposure", currentExposure))
                .WaitForCompletion();
        }
        else if (currentShaderName.Contains("6 Sided")) //if it's six sided, we have to lerp _Tint-Color
        {
            Material currentSkyboxMaterial = new Material(RenderSettings.skybox);
            RenderSettings.skybox = currentSkyboxMaterial;

            Color currentTint = currentSkyboxMaterial.GetColor("_Tint");
            yield return DOVirtual
                .Color(currentTint, Color.black, transitionDuration / 2, (value) =>
                {
                    currentSkyboxMaterial.SetColor("_Tint", value);
                })
                .WaitForCompletion();
        }


        if (futureShaderName.Contains("Procedural"))
        {
            Material futureSkyboxMaterial = new Material(sceneryProperties.SkyboxMaterial);
            float futureExposure = futureSkyboxMaterial.GetFloat("_Exposure");

            // Reset future Skybox Material to correct starting value
            futureSkyboxMaterial.SetFloat("_Exposure", 0);

            RenderSettings.skybox = futureSkyboxMaterial;

            yield return DOTween
                .To(x => futureExposure = x, 0, futureExposure, transitionDuration / 2)
                .OnUpdate(() => RenderSettings.skybox.SetFloat("_Exposure", futureExposure))
                .WaitForCompletion();
        }
        else if (futureShaderName.Contains("6 Sided")) //if it's six sided, we have to lerp _Tint-Color
        {
            Material futureSkyboxMaterial = new Material(sceneryProperties.SkyboxMaterial);
            Color futureTint = futureSkyboxMaterial.GetColor("_Tint");

            // Reset future Skybox Material to correct starting value
            futureSkyboxMaterial.SetColor("_Tint", Color.black);

            RenderSettings.skybox = futureSkyboxMaterial;

            yield return DOVirtual
                .Color(Color.black, futureTint, transitionDuration/2, (value) =>
                {
                    RenderSettings.skybox.SetColor("_Tint", value);
                })
                .WaitForCompletion();
        }

        yield return null;
    }
}
