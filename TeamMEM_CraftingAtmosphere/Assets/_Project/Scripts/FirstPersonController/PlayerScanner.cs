using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerScanner : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] InputActionAsset inputActions;

    [Header("Scanner Properties")]
    [SerializeField] float scanDurationUnknown = 0.9f;
    [SerializeField] float scanDurationKnown = 0.5f;
    [SerializeField] float scanMaxDistance = 20f;
    [Space]
    [SerializeField] Transform cameraTransform;
    [SerializeField] LayerMask creatureLayerMask;

    [Header("Canvas Properties")]
    [SerializeField] TextMeshProUGUI creatureNameDisplay;
    [SerializeField] TextMeshProUGUI creaturePropertiesDisplay;
    [SerializeField] Image scannerDot;
    [SerializeField] Slider scannerProgressSlider;
    [SerializeField] Color scannerDotPositiveColor;
    [SerializeField] Color scannerDotNegativeColor;
    [SerializeField] Image creaturePropertiesPanel;
    [SerializeField] TextMeshProUGUI creaturePropertiesNameText;
    [SerializeField] TextMeshProUGUI heightValText;
    [SerializeField] TextMeshProUGUI corruptedValText;
    [SerializeField] TextMeshProUGUI walkingStyleValText;
    [Space]
    [SerializeField] Transform outsideTransform;
    [SerializeField] Transform insideTransform;
    [SerializeField] float inDuration = 0.3f;
    [SerializeField] float outDuration = 0.7f;
    [Space]
    [SerializeField] AnimationCurve inCurve;

    private InputAction _scanAction;
    private InputAction _scanQuitAction;

    private bool _isScanning = false;
    private bool _isPointingAtCreature = false;

    private bool _doDisplayCreatureInfo = false;

    private CreaturePropertiesHelper _currentCreature;

    private void Awake()
    {
        _scanAction = inputActions.FindActionMap("Player").FindAction("Scan");
        _scanQuitAction = inputActions.FindActionMap("Player").FindAction("ScanQuitting");

        _scanAction.started += ScanAction_Started;
        _scanAction.canceled += ScanAction_Canceled;

        _scanQuitAction.performed += ScanQuitAction_Performed;

        creatureNameDisplay.gameObject.SetActive(false);
        scannerProgressSlider.gameObject.SetActive(false);

        creaturePropertiesPanel.transform.position = outsideTransform.position;

    }

    private void OnDestroy()
    {
        _scanAction.started -= ScanAction_Started;
        _scanAction.canceled -= ScanAction_Canceled;

        _scanQuitAction.performed -= ScanQuitAction_Performed;
    }

    #region Event Subscriptions
    private void ScanAction_Started(InputAction.CallbackContext obj)
    {
        if (!_isPointingAtCreature)
            return;

        _isScanning = true;

        StartCoroutine(ScanRoutine());
    }


    private void ScanAction_Canceled(InputAction.CallbackContext obj)
    {
        _isScanning = false;
    }


    private void ScanQuitAction_Performed(InputAction.CallbackContext obj)
    {
        EnableDisableCreaturePropertiesPanel(false);
    }
    #endregion


    private void OnEnable()
    {
        _scanAction.Enable();
        _scanQuitAction.Enable();
    }


    private void OnDisable()
    {
        _scanAction.Disable();
        _scanQuitAction.Disable();
    }


    private IEnumerator ScanRoutine()
    {
        float timer = 0;

        scannerProgressSlider.gameObject.SetActive(true);

        while (timer < scanDurationUnknown)
        {
            if (!_isScanning)
            {
                scannerProgressSlider.gameObject.SetActive(false);
                scannerProgressSlider.value = 0;
                yield break;
            }
            scannerProgressSlider.value = Mathf.Floor((timer / scanDurationUnknown) * 5);

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        scannerProgressSlider.gameObject.SetActive(false);

        _currentCreature.WasScanned = true;

        EnableDisableCreaturePropertiesPanel(true);
        

        yield return null;
    }


    private void Update()
    {
        HandleScan();
        HandleCreatureProperties();
    }

    private void HandleScan()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, scanMaxDistance, creatureLayerMask))
        {
            if(!_isPointingAtCreature)
                _isPointingAtCreature = true;

            if(!creatureNameDisplay.gameObject.activeSelf)
                creatureNameDisplay.gameObject.SetActive(true);



            if (hitInfo.collider.GetComponent<CreaturePropertiesHelper>() != null)
            {
                if (scannerDot.color != scannerDotPositiveColor)
                    scannerDot.color = scannerDotPositiveColor;

                if (_currentCreature == null)
                    _currentCreature = hitInfo.collider.GetComponent<CreaturePropertiesHelper>();
            }

            creatureNameDisplay.text = _currentCreature.WasScanned ? _currentCreature.creatureProperties.CreatureName : "?";
        }
        else
        {
            if (_isPointingAtCreature)
                _isPointingAtCreature = false;

            if (creatureNameDisplay.gameObject.activeSelf)
                creatureNameDisplay.gameObject.SetActive(false);

            if (scannerDot.color != scannerDotNegativeColor)
                scannerDot.color = scannerDotNegativeColor;

            
        }
    }


    private void EnableDisableCreaturePropertiesPanel(bool enable)
    {
        _doDisplayCreatureInfo = enable;

        if(enable)
        {
            creaturePropertiesPanel.gameObject.SetActive(true);

            creaturePropertiesPanel.transform.DOMove(insideTransform.position, inDuration).SetEase(inCurve);
            creaturePropertiesNameText.text = _currentCreature.creatureProperties.CreatureName;
            heightValText.text = _currentCreature.creatureProperties.Height.ToString();
            corruptedValText.text = _currentCreature.creatureProperties.Corruption.ToString();
            walkingStyleValText.text = _currentCreature.creatureProperties.WalkingStyle.ToString();
        }
        else
        {
            if (_currentCreature != null)
                _currentCreature = null;

            creaturePropertiesPanel.transform.DOMove(outsideTransform.position, outDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    creaturePropertiesPanel.gameObject.SetActive(false);
                });
        }
    }



    private void HandleCreatureProperties()
    {
        if (!_doDisplayCreatureInfo)
            return;
    }
}
