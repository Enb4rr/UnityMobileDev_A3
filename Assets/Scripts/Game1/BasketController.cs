using UnityEngine;
using UnityEngine.InputSystem;

public class BasketController : MonoBehaviour
{
    #region Variables
    [Header("Input")]
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private string _positionActionName = "PointerPosition";
    [SerializeField] private string _pressActionName = "Press";

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 12f;
    [SerializeField] private float _minX = -2.5f;
    [SerializeField] private float _maxX = 2.5f;

    private Camera _mainCamera;

    private InputAction _positionAction;
    private InputAction _pressAction;

    private bool _isPressing;
    #endregion

    private void Awake()
    {
        _mainCamera = Camera.main;

        _positionAction = _inputActions.FindAction(_positionActionName);
        _pressAction = _inputActions.FindAction(_pressActionName);
    }

    private void OnEnable()
    {
        _positionAction.Enable();
        _pressAction.Enable();

        _pressAction.performed += OnPressStarted;
        _pressAction.canceled += OnPressCanceled;
    }

    private void OnDisable()
    {
        _pressAction.performed -= OnPressStarted;
        _pressAction.canceled -= OnPressCanceled;

        _positionAction.Disable();
        _pressAction.Disable();
    }

    private void Update()
    {
        if (!_isPressing) return;

        Vector2 screenPosition = _positionAction.ReadValue<Vector2>();

        Vector3 worldPosition = ScreenToWorldPosition(screenPosition);

        MoveToX(worldPosition.x);
    }

    private void OnPressStarted(InputAction.CallbackContext context)
    {
        _isPressing = true;
    }

    private void OnPressCanceled(InputAction.CallbackContext context)
    {
        _isPressing = false;
    }

    private Vector3 ScreenToWorldPosition(Vector2 screenPosition)
    {
        // Calculate the world position
        Vector3 position = new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(_mainCamera.transform.position.z - transform.position.z));

        return _mainCamera.ScreenToWorldPoint(position); // Convert screen position to world position
    }

    private void MoveToX(float targetX)
    {
        float clampedX = Mathf.Clamp(targetX, _minX, _maxX); // Clamp position to stay within bounds

        Vector3 targetPosition = new Vector3(clampedX, transform.position.y, transform.position.z); // Asign new position in the X axis

        transform.position = Vector3.Lerp(transform.position, targetPosition, _moveSpeed * Time.deltaTime); // Move basket
    }
}