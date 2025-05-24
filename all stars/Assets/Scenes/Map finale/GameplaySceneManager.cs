using Photon.Pun;
using UnityEngine;

public class GameplaySceneManager : MonoBehaviourPunCallbacks
{
    [Header("Player Prefab")]
    public string playerPrefabPath = "Prefabs/FirstPersonController"; // Path relative to Resources

    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            if (!string.IsNullOrEmpty(playerPrefabPath))
            {
                GameObject prefab = Resources.Load<GameObject>(playerPrefabPath);
                if (prefab != null)
                {
                    Vector3 spawnPosition = new Vector3(0, 1, 0); // Adjust to avoid ground
                    PhotonNetwork.Instantiate(playerPrefabPath, spawnPosition, Quaternion.identity);
                    Debug.Log($"Instantiated player prefab: {playerPrefabPath}, ViewID: {PhotonNetwork.LocalPlayer.ActorNumber * 1000 + 1}");
                }
                else
                {
                    Debug.LogError($"Failed to load prefab at Resources/{playerPrefabPath}.prefab. Ensure it exists in a Resources folder.");
                }
            }
            else
            {
                Debug.LogError("Player prefab path is not set in GameplaySceneManager");
            }
        }
        else
        {
            Debug.LogWarning("Not in a Photon room. Player instantiation skipped.");
        }
    }
}