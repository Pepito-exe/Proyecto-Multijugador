using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode; // <-- ¡Añade esta línea!

public class JoinServer : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
