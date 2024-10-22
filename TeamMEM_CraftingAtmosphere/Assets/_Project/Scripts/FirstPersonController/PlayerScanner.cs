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


    private InputAction _scanAction;

    private bool _isScanning = false;
    private bool _isPointingAtCreature = false;

    private CreaturePropertiesHelper _currentCreature;

    private void Awake()
    {
        _scanAction = inputActions.FindActionMap("Player").FindAction("Scan");

        _scanAction.started += ScanAction_Started;
        _scanAction.canceled += ScanAction_Canceled;

        creatureNameDisplay.gameObject.SetActive(false);
        scannerProgressSlider.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _scanAction.started -= ScanAction_Started;
        _scanAction.canceled -= ScanAction_Canceled;
    }


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


    private void OnEnable()
    {
        _scanAction.Enable();
    }


    private void OnDisable()
    {
        _scanAction.Disable();
    }


    private IEnumerator ScanRoutine()
    {
        float timer = 0;

        scannerProgressSlider.gameObject.SetActive(true);

        while (_isScanning && timer < scanDurationUnknown)
        {
            scannerProgressSlider.value = timer / scanDurationUnknown;

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        scannerProgressSlider.gameObject.SetActive(false);

        _currentCreature.WasScanned = true;

        yield return null;
    }


    private void Update()
    {
        HandleScan();
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

            if (_currentCreature != null)
                _currentCreature = null;
        }
    }
}
