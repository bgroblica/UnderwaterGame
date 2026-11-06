
using UnityEngine;
using Unity.Services.Multiplayer;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Services.Relay;

public class LobbyUIManager : MonoBehaviour
{
    [Header("Player List")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private LobbyPlayerEntryUI playerEntryPrefab;
    [SerializeField] private TMP_Text playerCountText;

    [Header("Player Panels")]
    [SerializeField] private GameObject hostPanel;
    [SerializeField] private GameObject playerPanel;

    [Header("Lobby Buttons")]
    [SerializeField] private Button hostReadyButton;
    [SerializeField] private Button playerReadyButton;
    [SerializeField] private Button hostLeaveButton;

    [SerializeField] private Button playerLeaveButton;
    [SerializeField] private Button startGameButton;

    [Header("Ready Button Appearance")]
    [SerializeField] private Color readyButtonColor = new Color(0.75f, 0.25f, 0.25f);
    [SerializeField] private Color notReadyButtonColor = new Color(0.2f, 0.65f, 0.3f);
    
    [SerializeField] private float readyButtonFontSize = 40f;
    [SerializeField] private float readyNotButtonFontSize = 50f;

    private ISession currentSession;

    private bool isReady = false;
    private bool isUpdatingReady = false;
    private bool isStartingGame = false;

    private bool AreAllPlayersReady()
    {
        if (currentSession == null)
            return false;
    
        var players = currentSession.Players;
    
        if (players.Count == 0)
            return false;
    
        foreach (var player in players)
        {
            if (!player.Properties.TryGetValue("isReady", out var readyProperty))
                return false;
    
            if (!bool.TryParse(readyProperty.Value, out bool playerReady))
                return false;
    
            if (!playerReady)
                return false;
        }
    
        return true;
    }

    private void Start()
    {
        currentSession = SessionManager.CurrentSession;

        if (currentSession == null)
        {
            Debug.LogError("No active session found!");
            return;
        }

        // Show the correct panel for this player.
        bool isHost = currentSession.IsHost;

        hostPanel.SetActive(isHost);
        playerPanel.SetActive(!isHost);

        // Read the local player's current ready status.
        if (currentSession.CurrentPlayer.Properties.TryGetValue(
            "isReady", out var readyProperty))
        {
            bool.TryParse(readyProperty.Value, out isReady);
        }

        UpdateReadyButtonText();

        // Listen for session changes.
        currentSession.Changed += RefreshPlayerList;
        currentSession.Deleted += OnSessionDeleted;

        RefreshPlayerList();

        Debug.Log(isHost
            ? "Lobby initialized as HOST."
            : "Lobby initialized as PLAYER.");
    }

    private void OnDestroy()
    {
        if (currentSession != null)
        {
            currentSession.Changed -= RefreshPlayerList;
            currentSession.Deleted -= OnSessionDeleted;
        }
    }

    // ==========================================
    // READY BUTTON
    // ==========================================

    public async void OnReadyClicked()
    {
        Debug.Log("READY BUTTON CLICKED!");
    
        if (currentSession == null || isUpdatingReady)
        {
            Debug.LogWarning("Cannot toggle ready. Session missing or update in progress.");
            return;
        }
    
        isUpdatingReady = true;
    
        bool newReadyState = !isReady;
    
        UpdateReadyButtonText();
    
        Debug.Log("Saving new ready state: " + newReadyState);
    
        try
        {
            var properties = new Dictionary<string, PlayerProperty>
            {
                {
                    "isReady",
                    new PlayerProperty(
                        visibility: Unity.Services.Multiplayer.VisibilityPropertyOptions.Member,
                        value: newReadyState.ToString().ToLower()
                    )
                }
            };
    
            currentSession.CurrentPlayer.SetProperties(properties);
    
            await currentSession.SaveCurrentPlayerDataAsync();
    
            isReady = newReadyState;
    
            Debug.Log("Ready status successfully saved: " + isReady);
    
            RefreshPlayerList();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to update ready status: " + e);
        }
        finally
        {
            isUpdatingReady = false;
    
            UpdateReadyButtonText();
    
            Debug.Log("Ready button interactable: " +
                (currentSession.IsHost
                    ? hostReadyButton.interactable
                    : playerReadyButton.interactable));
        }
    }

    private void UpdateReadyButtonText()
    {
        string buttonText = isReady ? "Not Ready" : "Ready";
    
        Color buttonColor = isReady
            ? readyButtonColor
            : notReadyButtonColor;
    
        UpdateButtonAppearance(hostReadyButton, buttonText, buttonColor);
        UpdateButtonAppearance(playerReadyButton, buttonText, buttonColor);
    }
    
    private void UpdateButtonAppearance(Button button, string text, Color color)
    {
        if (button == null)
            return;
    
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
    
        if (buttonText != null)
        {
            buttonText.text = text;
    
            // Different font size depending on button text.
            buttonText.fontSize = text == "Ready"
                ? readyButtonFontSize
                : readyNotButtonFontSize;
        }
    
        Image buttonImage = button.GetComponent<Image>();
    
        if (buttonImage != null)
        {
            buttonImage.color = color;
        }
    
        button.interactable = !isUpdatingReady;
    }

    // ==========================================
    // REFRESH PLAYER LIST
    // ==========================================

    private void RefreshPlayerList()
    {
        if (currentSession == null)
            return;

        // Clear previous entries.
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        var players = currentSession.Players;

        // Update player count.
        playerCountText.text =
            players.Count + "/" + currentSession.MaxPlayers;

        // Create an entry for each player.
        foreach (var player in players)
        {
            string playerName = "Unknown Player";
            bool isHost = false;
            bool playerReady = false;

            if (player.Properties.TryGetValue("playerName", out var nameProperty))
            {
                playerName = nameProperty.Value ?? playerName;
            }

            if (player.Properties.TryGetValue("isHost", out var hostProperty))
            {
                bool.TryParse(hostProperty.Value, out isHost);
            }

            if (player.Properties.TryGetValue("isReady", out var readyProperty))
            {
                bool.TryParse(readyProperty.Value, out playerReady);
            }

            LobbyPlayerEntryUI entry = Instantiate(
                playerEntryPrefab,
                contentParent
            );

            entry.Setup(playerName, isHost, playerReady);
        }

        Debug.Log("Lobby updated. Players: " + players.Count);
        UpdateStartGameButton();
    }

    public async void OnLeaveClicked()
    {
        Debug.Log("Leaving lobby...");
    
        try
        {
            await SessionManager.LeaveCurrentSessionAsync();
    
            SceneManager.LoadScene("MenuTesting");
        }
        catch (Exception e)
        {
            Debug.LogError("Could not leave lobby: " + e.Message);
        }
    }

    private void OnSessionDeleted()
    {
        // The host handles its own scene transition.
        if (currentSession != null && currentSession.IsHost)
            return;
    
        Debug.LogWarning("Host closed the lobby!");
    
        SessionManager.ClearCurrentSession();
    
        SceneManager.LoadScene("MenuTesting");
    }


    public async void OnStartGameClicked()
    {
        if (currentSession == null || !currentSession.IsHost)
            return;
    
        if (isStartingGame)
            return;
    
        if (!AreAllPlayersReady())
        {
            Debug.LogWarning("Not everyone is ready!");
            return;
        }
    
        isStartingGame = true;
        UpdateStartGameButton();
    
        Debug.Log("Everyone is ready! Starting Relay network...");
    
        try
        {
            // Start Relay networking for this existing session.
            await currentSession.AsHost().Network.StartRelayNetworkAsync(new RelayNetworkOptions());
    
            Debug.Log("Relay network started!");
    
            // Check that NGO was started by the session integration.
            NetworkManager networkManager = NetworkManager.Singleton;
    
            if (networkManager == null ||
                !networkManager.IsListening ||
                !networkManager.IsHost)
            {
                throw new Exception(
                    "Relay started, but NGO host is not running."
                );
            }
    
            Debug.Log("NGO host is running. Loading GameTesting...");
    
            // Load the gameplay scene for everyone.
            var status = networkManager.SceneManager.LoadScene(
                "GameTesting",
                LoadSceneMode.Single
            );
    
            if (status != Unity.Netcode.SceneEventProgressStatus.Started)
            {
                throw new Exception(
                    "Failed to start network scene loading: " + status
                );
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to start game: " + e);
    
            isStartingGame = false;
            UpdateStartGameButton();
        }
    }


    private void UpdateStartGameButton()
    {
        if (startGameButton == null || currentSession == null)
            return;
    
        bool canStart =
            currentSession.IsHost &&
            AreAllPlayersReady() &&
            !isStartingGame;
    
        startGameButton.interactable = canStart;
    }
}
