using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickUp : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform pickUpTransform;
    [SerializeField] float pickUpDuration = 0.5f;
    [Space]
    [SerializeField] LayerMask pickUpLayerMask;
    [Space]
    [SerializeField] float pickUpDistance = 4f;
    [Space]
    [SerializeField] Color pickUpColor = Color.white;

    InputAction _pickUpAction;

    bool _isHoldingItem = false;
    bool _canPickUp = false;

    PickUpItem _currentPickUpItem = null;

    PlayerScanner _scanner;

    Color _initScannerColor;

    #region Setup
    private void Awake()
    {
        _pickUpAction = inputActions.FindActionMap("Player").FindAction("PickUp");

        _scanner = GetComponent<PlayerScanner>();
        _initScannerColor = _scanner.GetScannerColor;
    }


    private void Start()
    {
        _pickUpAction.performed += PickUpAction_Performed;
    }

    private void OnDestroy()
    {
        _pickUpAction.performed -= PickUpAction_Performed;
    }

    private void OnEnable()
    {
        _pickUpAction.Enable();
    }

    private void OnDisable()
    {
        _pickUpAction.Disable();
    }
#endregion

    private void Update()
    {
        RaycastForPickup();

        if(_isHoldingItem && _currentPickUpItem != null)
        {
            _currentPickUpItem.transform.position = pickUpTransform.position;
            _currentPickUpItem.transform.rotation = pickUpTransform.rotation;
        }
    }



    private void RaycastForPickup()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);


        if (Physics.Raycast(ray, out RaycastHit hitInfo, pickUpDistance, pickUpLayerMask))
        {
            _canPickUp = true;

            _currentPickUpItem = hitInfo.collider.gameObject.GetComponent<PickUpItem>();

            if (_scanner.GetScannerColor != pickUpColor)
            {
                Debug.Log("SHIIITIT");
                _scanner.ChangeScannerDotColor(pickUpColor);
            }
        }
        else
        {
            if(_canPickUp && _scanner.GetScannerColor != _initScannerColor)
            {
                _scanner.ChangeScannerDotColor(_initScannerColor);

                _canPickUp = false;
            }

            if (!_isHoldingItem)
                _currentPickUpItem = null;
        }
    }


    private void PickUpAction_Performed(InputAction.CallbackContext obj)
    {
        if (_isHoldingItem)
        {
            _isHoldingItem = false;

            _currentPickUpItem.DoPutDown(pickUpDuration);
            _currentPickUpItem = null;
        }
        else
        {
            if (!_canPickUp)
                return;

            _isHoldingItem = true;

            if (_currentPickUpItem != null)
            {
                _currentPickUpItem.DoPickUp(pickUpTransform.position, pickUpTransform.rotation, pickUpDuration);
            }
        }
    }
}
