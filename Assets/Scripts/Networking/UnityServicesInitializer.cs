
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System;
using System.Threading.Tasks;

public class UnityServicesInitializer : MonoBehaviour
{
    public static UnityServicesInitializer Instance { get; private set; }

    public bool IsInitialized { get; private set; }

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InitializeServices();
    }

    private async Task InitializeServices()
    {
        try
        {
            // Initialize Unity Gaming Services
            await UnityServices.InitializeAsync();

            // Authenticate anonymously
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            IsInitialized = true;

            Debug.Log("Unity Services initialized successfully!");
            Debug.Log("Player ID: " +
                AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e)
        {
            Debug.LogError("Unity Services initialization failed: " + e);
        }
    }
}
