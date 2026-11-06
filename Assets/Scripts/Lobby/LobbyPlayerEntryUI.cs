
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyPlayerEntryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text hostIndicatorText;
    [SerializeField] private TMP_Text readyStatusText;

    [Header("Ready Panel")]
    [SerializeField] private Image readyStatusPanel;

    [Header("Colors")]
    [SerializeField] private Color readyColor = new Color(0.2f, 0.65f, 0.3f);
    [SerializeField] private Color notReadyColor = new Color(0.75f, 0.25f, 0.25f);

    public void Setup(string playerName, bool isHost, bool isReady)
    {
        playerNameText.text = playerName;

        hostIndicatorText.gameObject.SetActive(isHost);

        readyStatusText.text = isReady ? "Ready" : "Not Ready";

        readyStatusPanel.color = isReady
            ? readyColor
            : notReadyColor;
    }
}
