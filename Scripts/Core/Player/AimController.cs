using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class AimController : NetworkBehaviour
{
    public static AimController instance;

    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CinemachineCamera thirdPersonCamera;
    [SerializeField] private CinemachineCamera aimCamera;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] private Transform fireTransform;

    [Header("Settings")]
    public bool isAimingStatus;
    [SerializeField] private float rotationSpeed = 20f;

    [Header("Camera Priorities")]
    [SerializeField] private int normalPriority = 10;
    [SerializeField] private int lowPriority = 0;

    [Header("Third Person Camera Settings")]
    [SerializeField] private float mouseSensitivityX = 2f;
    [SerializeField] private float mouseSensitivityY = 2f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 70f;

    private float cameraRotationX = 0f;
    private float cameraRotationY = 0f;

    public float CameraRotationY => cameraRotationY;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        inputReader.OnAimEvent += HandleAim;

        thirdPersonCamera.Priority.Value = normalPriority;
        aimCamera.Priority.Value = lowPriority;

        // Inicializar rotación
        cameraRotationY = transform.eulerAngles.y;
        cameraRotationX = 0f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        instance = this;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

        inputReader.OnAimEvent -= HandleAim;
    }

    private void HandleAim(bool isAiming)
    {
        isAimingStatus = isAiming;

        if (isAiming)
        {
            thirdPersonCamera.Priority.Value = lowPriority;
            aimCamera.Priority.Value = normalPriority;
        }
        else
        {
            thirdPersonCamera.Priority.Value = normalPriority;
            aimCamera.Priority.Value = lowPriority;
        }
    }

    private void LateUpdate()
    {
        if (!IsOwner) { return; }

        if (!isAimingStatus)
        {
            RotateThirdPersonCamera();
        }
        else
        {
            // ✅ En modo apuntar: rotar con mouse libremente
            RotateAimMode();
        }
    }

    // ✅ NUEVO: Rotación libre con mouse en modo apuntar
    private void RotateAimMode()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;

        cameraRotationY += mouseX;
        cameraRotationX -= mouseY;
        cameraRotationX = Mathf.Clamp(cameraRotationX, minVerticalAngle, maxVerticalAngle);

        // Rotar el personaje horizontalmente con el mouse
        transform.rotation = Quaternion.Euler(0f, cameraRotationY, 0f);

        // Debug
        Debug.DrawRay(transform.position, transform.forward * 5f, Color.red);
    }

    // ✅ MODIFICADO: Método compartido para actualizar rotación con mouse
    private void UpdateCameraRotationWithMouse()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;

        cameraRotationY += mouseX;
        cameraRotationX -= mouseY;
        cameraRotationX = Mathf.Clamp(cameraRotationX, minVerticalAngle, maxVerticalAngle);
    }

    private void RotateThirdPersonCamera()
    {
        // Actualizar rotación con mouse
        UpdateCameraRotationWithMouse();

        // Rotar el personaje solo en el eje Y
        transform.rotation = Quaternion.Euler(0f, cameraRotationY, 0f);

        // Debug para verificar
        Debug.DrawRay(transform.position, transform.forward * 5f, Color.blue);
    }

    public Vector3 AimToRayPoint()
    {
        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);

        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            if (fireTransform != null)
            {
                fireTransform.position = raycastHit.point;
            }
            mouseWorldPosition = raycastHit.point;
        }
        else
        {
            // Si no hay colisión, apuntar hacia adelante
            mouseWorldPosition = ray.GetPoint(100f);
        }

        return mouseWorldPosition;
    }
}
