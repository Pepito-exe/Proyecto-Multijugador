using UnityEngine;
using Unity.Netcode;

public class ProjectileController : NetworkBehaviour
{
    private Vector3 moveDirection;
    private float speed;
    private bool isInitialized = false;

    [Header("Settings")]
    [SerializeField] private float lifeTime = 3f;     // tiempo de vida
    [SerializeField] private LayerMask hitLayers;     // capas que la bala puede matar o destruir

    private float lifeTimer;

    // Llamado por el ServerRpc
    public void Initialize(Vector3 direction, float projectileSpeed)
    {
        moveDirection = direction;
        speed = projectileSpeed;
        isInitialized = true;
        lifeTimer = lifeTime;
    }

    void Update()
    {
        // Solo el servidor/host mueve y destruye la bala
        if (!IsServer || !isInitialized)
            return;

        // Mover
        transform.position += moveDirection * speed * Time.deltaTime;

        // Tiempo de vida
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            DespawnProjectile();
        }
    }

    // ✅ Cuando la bala toca algo
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        // Si el collider está dentro de las capas válidas para colisión
        if (((1 << other.gameObject.layer) & hitLayers) != 0)
        {
            // Aquí puedes agregar daño al objetivo
            // other.GetComponent<Health>()?.TakeDamage();

            DespawnProjectile();
        }
    }

    // ✅ Método seguro para destruir la bala en red
    private void DespawnProjectile()
    {
        if (NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }

        Destroy(gameObject);
    }
}
