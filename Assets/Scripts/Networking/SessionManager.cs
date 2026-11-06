
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Multiplayer;
using Unity.Services.Authentication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    public static ISession CurrentSession { get; private set; }

    private bool isCreatingSession = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ==========================================
    // CREATE SESSION
    // ==========================================

    public async void CreateSession()
    {
        if (isCreatingSession)
            return;

        if (UnityServicesInitializer.Instance == null ||
            !UnityServicesInitializer.Instance.IsInitialized)
        {
            Debug.LogError("Unity Services are not initialized!");
            return;
        }

        isCreatingSession = true;

        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;

            string shortId = playerId.Substring(
                0,
                Math.Min(6, playerId.Length)
            );

            string hostName = "Player_" + shortId;

            SessionOptions options = new SessionOptions
            {
                Name = hostName,
                MaxPlayers = 4,
                IsPrivate = false
            };

            CurrentSession =
                await MultiplayerService.Instance.CreateSessionAsync(options);

            Debug.Log("Session created successfully!");
            Debug.Log("Session ID: " + CurrentSession.Id);
            Debug.Log("Session name: " + hostName);

            await SetLocalPlayerDataAsync(true);

            SceneManager.LoadScene("LobbyTesting");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to create session: " + e);
        }
        finally
        {
            isCreatingSession = false;
        }
    }

    // ==========================================
    // SET LOCAL PLAYER DATA
    // ==========================================

    public static async Task SetLocalPlayerDataAsync(bool isHost)
    {
        if (CurrentSession == null)
        {
            Debug.LogError("Cannot set player data: no active session!");
            return;
        }

        string playerId = AuthenticationService.Instance.PlayerId;

        string shortId = playerId.Substring(
            0,
            Math.Min(6, playerId.Length)
        );

        string playerName = "Player_" + shortId;

        var properties = new Dictionary<string, PlayerProperty>
        {
            {
                "playerName",
                new PlayerProperty(
                    visibility: VisibilityPropertyOptions.Member,
                    value: playerName
                )
            },
            {
                "isReady",
                new PlayerProperty(
                    visibility: VisibilityPropertyOptions.Member,
                    value: "false"
                )
            },
            {
                "isHost",
                new PlayerProperty(
                    visibility: VisibilityPropertyOptions.Member,
                    value: isHost.ToString()
                )
            }
        };

        CurrentSession.CurrentPlayer.SetProperties(properties);

        await CurrentSession.SaveCurrentPlayerDataAsync();

        Debug.Log("Player data saved!");
        Debug.Log("Player name: " + playerName);
        Debug.Log("Host: " + isHost);
        Debug.Log("Ready: false");
    }

    // ==========================================
    // SET CURRENT SESSION
    // ==========================================

    public static void SetCurrentSession(ISession session)
    {
        CurrentSession = session;
    }

    // ==========================================
    // LEAVE SESSION
    // ==========================================

    public static async Task LeaveCurrentSessionAsync()
    {
        if (CurrentSession == null)
        {
            Debug.LogWarning("No active session to leave.");
            return;
        }

        ISession session = CurrentSession;

        try
        {
            if (session.IsHost)
            {
                Debug.Log("Host is deleting the session...");

                await session.AsHost().DeleteAsync();

                Debug.Log("Session successfully deleted!");
            }
            else
            {
                Debug.Log("Player is leaving the session...");

                await session.LeaveAsync();

                Debug.Log("Successfully left the session!");
            }

            CurrentSession = null;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to leave session: " + e);
            throw;
        }
    }

    // ==========================================
    // CLEAR SESSION
    // ==========================================

    public static void ClearCurrentSession()
    {
        CurrentSession = null;
    }
}
