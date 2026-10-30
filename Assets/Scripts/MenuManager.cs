
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField addressInput;

    [Header("Network Settings")]
    [SerializeField] private ushort port = 7777;

    private NetworkManager networkManager;

    private void Start()
    {
        networkManager = NetworkManager.Singleton;

    if (networkManager == null)
        {
            Debug.LogError("NetworkManager Singleton not found!");
        }
        else
        {
            Debug.Log("NetworkManager successfully found!");
        }
    }

    public void HostGame()
    {
        bool success = networkManager.StartHost();

        if (success)
        {
            Debug.Log("Host started successfully!");

            networkManager.SceneManager.LoadScene("GameTesting", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("Failed to start Host.");
        }
    }

    public void JoinGame()
    {
        string address = addressInput.text.Trim();

        if (string.IsNullOrEmpty(address))
        {
            Debug.LogWarning("Please enter an IP address.");
            return;
        }

        UnityTransport transport =
            networkManager.GetComponent<UnityTransport>();

        transport.SetConnectionData(address, port);

        bool success = networkManager.StartClient();

        if (success)
        {
            Debug.Log("Connecting to Host...");
        }
        else
        {
            Debug.LogError("Failed to start Client.");
        }
    }

    public void Disconnect()
    {
        if (networkManager != null && networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
