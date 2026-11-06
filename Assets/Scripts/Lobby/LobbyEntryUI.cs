
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyEntryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text sessionNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Button joinButton;

    private string sessionId;
    private LobbyBrowser lobbyBrowser;

    public void Setup(
        string id,
        string sessionName,
        int playerCount,
        int maxPlayers,
        LobbyBrowser browser)
    {
        sessionId = id;
        lobbyBrowser = browser;

        sessionNameText.text = sessionName;
        playerCountText.text = playerCount + "/" + maxPlayers;

        joinButton.interactable = playerCount < maxPlayers;

        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(JoinLobby);
    }

    private void JoinLobby()
    {
        lobbyBrowser.JoinSession(sessionId);
    }
}
