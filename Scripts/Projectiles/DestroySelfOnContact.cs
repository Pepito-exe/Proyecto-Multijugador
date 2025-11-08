using UnityEngine;

public class DestroyOnSelfContact : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
