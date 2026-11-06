
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Multiplayer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;

public class LobbyBrowser : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private LobbyEntryUI lobbyEntryPrefab;
    [SerializeField] private TMP_Text statusText;

    private readonly List<GameObject> lobbyEntries = new();

    private bool isRefreshing = false;
    private bool isJoining = false;

    public async void RefreshSessions()
    {
        if (isRefreshing || isJoining)
            return;

        isRefreshing = true;
        statusText.text = "Searching for games...";

        ClearLobbyList();

        try
        {
            QuerySessionsOptions options = new QuerySessionsOptions
            {
                Count = 50
            };

            QuerySessionsResults results =
                await MultiplayerService.Instance.QuerySessionsAsync(options);

            int foundSessions = 0;

            foreach (var session in results.Sessions)
            {
                // Don't display our own current session.
                if (SessionManager.CurrentSession != null &&
                    session.Id == SessionManager.CurrentSession.Id)
                {
                    continue;
                }

                LobbyEntryUI entry = Instantiate(
                    lobbyEntryPrefab,
                    contentParent
                );

                entry.Setup(
                    session.Id,
                    session.Name,
                    session.MaxPlayers - session.AvailableSlots,
                    session.MaxPlayers,
                    this
                );

                lobbyEntries.Add(entry.gameObject);
                foundSessions++;
            }

            if (foundSessions == 0)
            {
                statusText.text = "No available games found.";
            }
            else
            {
                statusText.text = "Found " + foundSessions + " games.";
            }

            Debug.Log("Found " + foundSessions + " available sessions.");
        }
        catch (Exception e)
        {
            statusText.text = "Failed to load games.";
            Debug.LogError("Session search failed: " + e);
        }
        finally
        {
            isRefreshing = false;
        }
    }

    public async void JoinSession(string sessionId)
    {
        if (isJoining || isRefreshing)
            return;

        isJoining = true;
        statusText.text = "Joining game...";

        try
        {
            ISession joinedSession =
                await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);

            SessionManager.SetCurrentSession(joinedSession);

            await SessionManager.SetLocalPlayerDataAsync(false);

            Debug.Log("Successfully joined session: " + joinedSession.Name);

            SceneManager.LoadScene("LobbyTesting");
        }
        catch (Exception e)
        {
            statusText.text = "Failed to join game.";
            Debug.LogError("Failed to join session: " + e);
        }
        finally
        {
            isJoining = false;
        }
    }

    private void ClearLobbyList()
    {
        foreach (GameObject entry in lobbyEntries)
        {
            Destroy(entry);
        }

        lobbyEntries.Clear();
    }
}
