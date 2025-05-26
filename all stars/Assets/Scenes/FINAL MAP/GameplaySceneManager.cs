using Photon.Pun;
using UnityEngine;

public class GameplaySceneManager : MonoBehaviourPunCallbacks
{
    [Header("Player Prefab")]
    public string playerPrefabPath = "Prefabs/FirstPersonController";

    [Header("Spawn Points")]
    public Vector3[] spawnPoints = {
        new Vector3(0, 2, 0),   // Spawn point 1
        new Vector3(2, 2, 2),   // Spawn point 2
        new Vector3(-2, 2, -2), // Spawn point 3
        new Vector3(2, 2, -2)   // Spawn point 4
    };

    void Start()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("Not connected to Photon, cannot instantiate player");
            return;
        }
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("Not in a room, cannot instantiate player");
            return;
        }

        // Choose spawn point based on player count or actor number
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        int spawnIndex = playerIndex % spawnPoints.Length;
        Vector3 selectedSpawnPoint = spawnPoints[spawnIndex];

        GameObject player = PhotonNetwork.Instantiate(playerPrefabPath, selectedSpawnPoint, Quaternion.identity);
        if (player == null)
        {
            Debug.LogError($"Failed to instantiate player prefab at {selectedSpawnPoint}");
        }
        else
        {
            PhotonView view = player.GetComponent<PhotonView>();
            Debug.Log($"Player instantiated at {selectedSpawnPoint}, ViewID: {view?.ViewID}, ActorNumber: {PhotonNetwork.LocalPlayer.ActorNumber}");
        }
    }
}