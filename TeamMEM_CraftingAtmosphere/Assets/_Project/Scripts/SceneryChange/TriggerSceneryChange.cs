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

        // Smoothly fade fog
        StartCoroutine(FogChange_Routine());

        // Smoothly fade shadowColor
        Color initShadowColor = RenderSettings.subtractiveShadowColor;
        DOVirtual
               .Color(initShadowColor, sceneryProperties.RealtimeShadowColor, transitionDuration, (value) =>
               {
                   RenderSettings.subtractiveShadowColor = value;
               });


        // Smoothly fade environment lighting
        Color initSkyColor = RenderSettings.ambientSkyColor;
        Color initEquatorColor = RenderSettings.ambientEquatorColor;
        Color initGroundColor = RenderSettings.ambientGroundColor;

        DOVirtual
               .Color(initSkyColor, sceneryProperties.SkyColorHDR, transitionDuration, (value) =>
               {
                   RenderSettings.ambientSkyColor = value;
               });
        DOVirtual
               .Color(initEquatorColor, sceneryProperties.EquatorColorHDR, transitionDuration, (value) =>
               {
                   RenderSettings.ambientEquatorColor = value;
               });
        DOVirtual
               .Color(initGroundColor, sceneryProperties.GroundColorHDR, transitionDuration, (value) =>
               {
                   RenderSettings.ambientGroundColor = value;
               });

        // Smoothly Fade Environment Reflections
        float newReflectionIntensity = RenderSettings.reflectionIntensity;
        DOTween
            .To(x => newReflectionIntensity = x, newReflectionIntensity, sceneryProperties.ReflectionIntensity, transitionDuration)
            .OnUpdate(() => RenderSettings.reflectionIntensity = newReflectionIntensity);
    }



    private IEnumerator SkyboxChange_Routine(string currentShaderName, string futureShaderName)
    {
        // Current Shader stuff

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
        else if (futureShaderName.Contains("Cubemap Extended"))
        {
            Material currentSkyboxMaterial = new Material(RenderSettings.skybox);
            RenderSettings.skybox = currentSkyboxMaterial;

            float currentExposure = currentSkyboxMaterial.GetFloat("_Exposure");

            yield return DOTween
                .To(x => currentExposure = x, currentExposure, 0, transitionDuration / 2)
                .OnUpdate(() => currentSkyboxMaterial.SetFloat("_Exposure", currentExposure))
                .WaitForCompletion();
        }


        // Future Shader stuff

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
        else if (futureShaderName.Contains("Cubemap Extended"))
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

        yield return null;
    }


    private IEnumerator FogChange_Routine()
    {
        float newFogDensity = 0;
        if (sceneryProperties.IsFogEnabled) // turn fog on
        {
            RenderSettings.fogDensity = 0.0f;
            RenderSettings.fog = true;

            

            yield return DOTween
                .To(x => newFogDensity = x, RenderSettings.fogDensity, sceneryProperties.fogDensity, transitionDuration)
                .OnUpdate(() => RenderSettings.fogDensity = newFogDensity)
                .WaitForCompletion();
        }
        else
        {
            yield return DOTween
                .To(x => newFogDensity = x, RenderSettings.fogDensity, 0, transitionDuration)
                .OnUpdate(() => RenderSettings.fogDensity = newFogDensity)
                .WaitForCompletion();

            RenderSettings.fogDensity = 0.0f;
            RenderSettings.fog = false;
        }


        yield return null;
    }
}
