using UnityEngine;
using Unity.Netcode;

public class DealDamage : NetworkBehaviour
{
    [SerializeField] private int damageAmount = 10;

    private ulong ownerClientId;

    // ✅ CORREGIDO: Asignar correctamente el parámetro
    public void SetOwner(ulong clientId)
    {
        this.ownerClientId = clientId;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Solo el servidor procesa colisiones
        if (!IsServer) return;

        // Ignorar colisión con el dueño de la bala
        if (other.TryGetComponent<NetworkObject>(out NetworkObject netObj))
        {
            if (ownerClientId == netObj.OwnerClientId)
            {
                return;
            }
        }

        // Aplicar daño si tiene componente Health
        if (other.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(damageAmount);

            // Destruir la bala después de hacer daño
            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn();
                Destroy(gameObject);
            }
        }
    }
}