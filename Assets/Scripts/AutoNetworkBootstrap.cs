using Mirror;
using UnityEngine;

public class AutoNetworkBootstrap : MonoBehaviour
{

    void Start()
    {
        var manager = NetworkManager.singleton;

        Debug.Log("[Mirror] Пробую подключиться как клиент...");
        manager.StartClient();

        Invoke(nameof(CheckConnection), 1f);
    }

    void CheckConnection()
    {
        if (NetworkClient.isConnected)
        {
            Debug.Log("[Mirror] Клиент успешно подключился");
        }
        else
        {
            Debug.Log("[Mirror] Сервер не найден — создаю хост");
            NetworkManager.singleton.StopClient();
            NetworkManager.singleton.StartHost();
        }
    }
}