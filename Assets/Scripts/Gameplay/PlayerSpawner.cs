
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    private bool matchStarted = false;

    private HashSet<ulong> assignedPlayers = new HashSet<ulong>();
    private int nextSpawnIndex = 0;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
            return;

        NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
    }

    private void OnLoadEventCompleted(
        string sceneName,
        LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        if (sceneName != gameObject.scene.name)
            return;

        if (clientsTimedOut.Count > 0)
        {
            Debug.LogError("Some clients timed out while loading!");
            return;
        }

        AssignSpawnPositions();
    }

    private void AssignSpawnPositions()
    {
        if (!IsServer)
            return;

        List<NetworkClient> clients = new List<NetworkClient>(NetworkManager.Singleton.ConnectedClientsList);

        clients.Sort((a, b) => a.ClientId.CompareTo(b.ClientId));

        foreach (NetworkClient client in clients)
        {
            if (assignedPlayers.Contains(client.ClientId))
                continue;

            if (client.PlayerObject == null)
            {
                Debug.LogWarning(
                    $"PlayerObject not ready for client {client.ClientId}"
                );

                continue;
            }

            if (nextSpawnIndex >= spawnPoints.Length)
            {
                Debug.LogError("Not enough spawn points!");
                return;
            }

            PlayerMovement movement =
                client.PlayerObject.GetComponent<PlayerMovement>();

            if (movement == null)
            {
                Debug.LogError("PlayerMovement missing!");
                continue;
            }

            Vector3 spawnPosition =
                spawnPoints[nextSpawnIndex].position;

            movement.AssignSpawnPosition(spawnPosition);

            assignedPlayers.Add(client.ClientId);

            Debug.Log(
                $"Assigned spawn {nextSpawnIndex} to client {client.ClientId}"
            );

            nextSpawnIndex++;
        }
    }

    private void Update()
    {
        if (!IsServer || matchStarted)
            return;

        AssignSpawnPositions();

        var clients = NetworkManager.ConnectedClientsList;

        if (SessionManager.CurrentSession == null)
            return;

        int expectedPlayers = SessionManager.CurrentSession.Players.Count;

        if (expectedPlayers == 0 || clients.Count != expectedPlayers)
            return;

        if (assignedPlayers.Count != clients.Count)
            return;

        foreach (var client in clients)
        {
            if (client.PlayerObject == null)
                return;

            PlayerMovement movement =
                client.PlayerObject.GetComponent<PlayerMovement>();

            if (movement == null)
                return;

            if (!movement.HasAppliedSpawn())
                return;
        }

        matchStarted = true;

        Debug.Log("All players spawned! Enabling movement.");

        foreach (var client in clients)
        {
            PlayerMovement movement =
                client.PlayerObject.GetComponent<PlayerMovement>();

            movement.EnableMovement();
        }
    }
}
