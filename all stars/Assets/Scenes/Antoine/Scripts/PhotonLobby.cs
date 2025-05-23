using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TMP_PhotonLobby : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TMP_InputField inputCode;
    public UnityEngine.UI.Button btnCreate;
    public UnityEngine.UI.Button btnJoin;

    [Header("Player Prefab")]
    public string playerPrefabPath = "Prefabs/FirstPersonController"; // Ensure this matches

    void Start()
    {
        btnCreate.interactable = false;
        btnJoin.interactable = false;
        PhotonNetwork.ConnectUsingSettings();

        btnCreate.onClick.AddListener(OnCreateClicked);
        btnJoin.onClick.AddListener(OnJoinClicked);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master");
        btnCreate.interactable = true;
        btnJoin.interactable = true;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void OnCreateClicked()
    {
        string code = Random.Range(1000, 9999).ToString();
        PhotonNetwork.CreateRoom(code, new RoomOptions { MaxPlayers = 4 });
        Debug.Log($"Room created: {code}");
    }

    void OnJoinClicked()
    {
        string code = inputCode.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("Room code is empty");
            return;
        }
        PhotonNetwork.JoinRoom(code);
        Debug.Log($"Attempting to join room: {code}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Join room failed: {message}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name + ", Players: " + PhotonNetwork.PlayerList.Length);
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Master Client loading scene index 3");
            if (SceneManager.sceneCountInBuildSettings > 3)
            {
                PhotonNetwork.LoadLevel(3);
            }
            else
            {
                Debug.LogError("Scene index 3 not found in Build Settings");
            }
        }
        if (!string.IsNullOrEmpty(playerPrefabPath))
        {
            // Verify prefab exists before instantiating
            GameObject prefab = Resources.Load<GameObject>(playerPrefabPath);
            if (prefab != null)
            {
                PhotonNetwork.Instantiate(playerPrefabPath, Vector3.zero, Quaternion.identity);
                Debug.Log($"Instantiating player prefab: {playerPrefabPath}");
            }
            else
            {
                Debug.LogError($"Failed to load prefab at Resources/{playerPrefabPath}.prefab. Ensure it exists and is in a Resources folder.");
            }
        }
        else
        {
            Debug.LogError("Player prefab path is not set in PhotonLobby");
        }
    }
}






//old

//using Photon.Pun;
//using Photon.Realtime;
//using UnityEngine;
//using UnityEngine.UI;               
//using UnityEngine.SceneManagement;
//using TMPro;                       
//
//public class TMP_PhotonLobby : MonoBehaviourPunCallbacks
//{
//    [Header("UI References")]
//    public TMP_InputField inputCode;                  
//    public UnityEngine.UI.Button btnCreate;          
//    public UnityEngine.UI.Button btnJoin;
//
//    void Start()
//    {
//        
//        btnCreate.interactable = false;
//        btnJoin.interactable = false;
//        PhotonNetwork.ConnectUsingSettings();
//
//        btnCreate.onClick.AddListener(OnCreateClicked);
//        btnJoin.onClick.AddListener(OnJoinClicked);
//    }
//
//    public override void OnConnectedToMaster()
//    {
//        Debug.Log("Connecté à Photon Master");
//        btnCreate.interactable = true;
//        btnJoin.interactable = true;
//
//        PhotonNetwork.AutomaticallySyncScene = true;
//    }
//
//    void OnCreateClicked()
//    {
//        string code = Random.Range(1000, 9999).ToString();
//        PhotonNetwork.CreateRoom(code, new RoomOptions { MaxPlayers = 4 });
//        Debug.Log($"Room créée : {code}");
//    }
//
//    void OnJoinClicked()
//    {
//        string code = inputCode.text.Trim();
//        if (string.IsNullOrEmpty(code)) return;
//        PhotonNetwork.JoinRoom(code);
//        Debug.Log($"Tentative de jointure de la room : {code}");
//    }
//
//    public override void OnJoinRoomFailed(short returnCode, string message)
//    {
//        Debug.LogError($"Échec de la jointure : {message}");
//    }
//
//    public override void OnJoinedRoom()
//    {
//        Debug.Log("Room rejointe, chargement de la scène de jeu…");
//        SceneManager.LoadSceneAsync(3);
//    }
//}
