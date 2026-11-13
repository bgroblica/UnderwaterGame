
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    private bool playersPositioned = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        NetworkManager.SceneManager.OnLoadEventCompleted +=
            OnLoadEventCompleted;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
            return;

        NetworkManager.SceneManager.OnLoadEventCompleted -=
            OnLoadEventCompleted;
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
        if (!IsServer || playersPositioned)
            return;

        List<NetworkClient> clients =
            new List<NetworkClient>(
                NetworkManager.Singleton.ConnectedClientsList
            );

        // Consistent player ordering.
        clients.Sort((a, b) =>
            a.ClientId.CompareTo(b.ClientId));

        if (clients.Count > spawnPoints.Length)
        {
            Debug.LogError("Not enough spawn points for all players!");
            return;
        }

        for (int i = 0; i < clients.Count; i++)
        {
            NetworkObject playerObject =
                clients[i].PlayerObject;

            if (playerObject == null)
            {
                Debug.LogError(
                    "PlayerObject missing for client: " +
                    clients[i].ClientId
                );

                return;
            }

            PlayerMovement movement =
                playerObject.GetComponent<PlayerMovement>();

            if (movement == null)
            {
                Debug.LogError("PlayerMovement missing!");
                return;
            }

            movement.AssignSpawnPosition(
                spawnPoints[i].position
            );

            Debug.Log(
                "Assigned spawn point " + i +
                " to player " + clients[i].ClientId
            );
        }

        playersPositioned = true;
    }

    private void Update()
    {
        if (!IsServer || !playersPositioned)
            return;

        foreach (NetworkClient client in
                 NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                return;

            PlayerMovement movement =
                client.PlayerObject.GetComponent<PlayerMovement>();

            if (movement == null || !movement.HasAppliedSpawn())
                return;
        }

        Debug.Log("All players have applied their spawn positions!");
    }
}
