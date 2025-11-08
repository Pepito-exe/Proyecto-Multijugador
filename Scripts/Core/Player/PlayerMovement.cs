using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CharacterController characterController;

    [Header("Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    private Vector3 previousMovementInput;
    private Vector3 velocity;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        inputReader.OnMoveEvent += HandleMovement;

        // Verificar referencias
        if (characterController == null)
        {
            Debug.LogError("CharacterController no asignado!");
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

        inputReader.OnMoveEvent -= HandleMovement;
    }

    private void Update()
    {
        if (!IsOwner) { return; }

        Movement();
        ApplyGravity();
    }

    private void Movement()
    {
        if (characterController == null) return;

        float x = previousMovementInput.x;
        float z = previousMovementInput.z;

        // Debug para verificar input
        if (x != 0 || z != 0)
        {
            Debug.Log($"Moviendo - X: {x}, Z: {z}");
        }

        Vector3 moveDirection;

        // ✅ Verificar si está apuntando
        if (AimController.instance != null && AimController.instance.isAimingStatus)
        {
            // En modo apuntar: movimiento relativo a la cámara (strafe)
            Transform cameraTransform = Camera.main.transform;
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            // Mantener movimiento en el plano horizontal
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            moveDirection = right * x + forward * z;
        }
        else
        {
            // En modo normal: movimiento relativo al personaje
            moveDirection = transform.right * x + transform.forward * z;
        }

        moveDirection.Normalize();

        // Aplicar movimiento
        characterController.Move(moveDirection * movementSpeed * Time.deltaTime);

        // Debug visual
        if (moveDirection.magnitude > 0.1f)
        {
            Debug.DrawRay(transform.position, moveDirection * 2f, Color.green);
        }
    }

    private void ApplyGravity()
    {
        if (characterController == null) return;

        // Aplicar gravedad
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Pequeño valor para mantener grounded
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleMovement(Vector3 movementInput)
    {
        previousMovementInput = movementInput;

        // Debug para verificar que llega el input
        if (movementInput.magnitude > 0)
        {
            Debug.Log($"Input recibido: {movementInput}");
        }
    }
}
