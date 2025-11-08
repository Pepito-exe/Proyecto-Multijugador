using UnityEngine;
using Unity.Netcode;
using System;

public class Health : NetworkBehaviour
{
    [field: SerializeField] public int maxHealth { get; private set; } = 100;

    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    private bool isDead;

    public Action<Health> OnDie;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }
       
        currentHealth.Value = maxHealth;
       
    }

    public void TakeDamage(int damageValue)
    {
        ModifyHealth(-damageValue);
    }

    public void RestoreHealth(int healValue)
    {
        ModifyHealth(healValue);
    }

    private void ModifyHealth(int value)
    {
        if (!IsServer) { return; }

        int newHealth = currentHealth.Value + value;
        currentHealth.Value = Mathf.Clamp(newHealth, 0, maxHealth);

        if (currentHealth.Value == 0)
        {
            isDead = true;
            OnDie?.Invoke(this);
        }
    }
}
