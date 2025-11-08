using UnityEngine;
using Unity.Netcode;

public class FireController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] InputReader inputReader;
    [SerializeField] AimController aimController; // Asegúrate de asignar esto en el Inspector
    [SerializeField] Transform projectileSpawnPoint;

    // El prefab con el componente NetworkObject. ¡ASIGNAR ESTE!
    [SerializeField] GameObject projectileServer;

    private bool isFiring = false;

    [Header("Settings")]
    [SerializeField] float projectileSpeed = 10f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        inputReader.OnFireEvent += HandleFirePrimary;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

        inputReader.OnFireEvent -= HandleFirePrimary;
    }

    private void HandleFirePrimary(bool isFire)
    {
        this.isFiring = isFire;

        if (isFire)
        {
            TryFire();
        }
    }

    private void TryFire()
    {
        // Verificar si estamos apuntando (AimController.isAimingStatus debe ser public)
        if (aimController != null && aimController.isAimingStatus)
        {
            // Obtener el punto de puntería y la posición de inicio
            Vector3 targetPoint = aimController.AimToRayPoint();
            Vector3 spawnPosition = projectileSpawnPoint.position;

            // Calcular la dirección del disparo
            Vector3 fireDirection = (targetPoint - spawnPosition).normalized;

            // Solo el dueño (Owner) debe solicitar el disparo al servidor
            if (IsOwner)
            {
                // Llamar al método ServerRpc para que el servidor spawnee la bala en la red
                FireProjectileServerRpc(spawnPosition, fireDirection);
            }
        }
    }

    // Server Rpc: Solo se ejecuta en el Host/Servidor
    [ServerRpc]
    private void FireProjectileServerRpc(Vector3 spawnPosition, Vector3 direction)
    {
        // 1. Instanciar el prefab de bala (que DEBE tener NetworkObject)
        GameObject projectileInstance = Instantiate(
            projectileServer,
            spawnPosition,
            Quaternion.LookRotation(direction)
        );

        if (projectileInstance.TryGetComponent<DealDamage>(out DealDamage damage))
        {
            damage.SetOwner(OwnerClientId);
        }

        // 2. Aplicar la velocidad de la bala
        ProjectileController projectile = projectileInstance.GetComponent<ProjectileController>();
        if (projectile != null)
        {
            projectile.Initialize(direction, projectileSpeed);
        }

        // 3. ¡Parte Crítica! Spawnea la bala en la red para que todos la vean.
        projectileInstance.GetComponent<NetworkObject>().Spawn();
    }
}