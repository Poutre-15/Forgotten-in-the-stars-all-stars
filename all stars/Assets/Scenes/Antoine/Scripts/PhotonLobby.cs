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

    [Header("Scene Settings")]
    public int intermediateSceneIndex = 2; // Set to your intermediate scene index

    void Start()
    {
        // Disable buttons until connected
        if (btnCreate != null) btnCreate.interactable = false;
        if (btnJoin != null) btnJoin.interactable = false;

        // Check connection state
        if (PhotonNetwork.NetworkClientState == ClientState.Disconnected)
        {
            Debug.Log("Client is Disconnected. Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
        {
            Debug.Log("Client already connected to Master Server");
            OnConnectedToMaster(); // Enable buttons immediately
        }
        else
        {
            Debug.LogWarning($"Client in state {PhotonNetwork.NetworkClientState}. Disconnecting and reconnecting...");
            PhotonNetwork.Disconnect(); // Disconnect to reset state
        }

        // Assign button listeners
        if (btnCreate != null) btnCreate.onClick.AddListener(OnCreateClicked);
        if (btnJoin != null) btnJoin.onClick.AddListener(OnJoinClicked);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected from Photon: {cause}");
        if (cause != DisconnectCause.DisconnectByClientLogic)
        {
            Debug.LogWarning("Unexpected disconnection. Reconnecting...");
        }
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
        if (btnCreate != null) btnCreate.interactable = true;
        if (btnJoin != null) btnJoin.interactable = true;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void OnCreateClicked()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("Cannot create room: Not connected to Photon Master Server");
            return;
        }
        string code = Random.Range(1000, 9999).ToString();
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.CreateRoom(code, roomOptions);
        Debug.Log($"Creating room: {code}");
    }

    void OnJoinClicked()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("Cannot join room: Not connected to Photon Master Server");
            return;
        }
        string code = inputCode != null ? inputCode.text.Trim() : "";
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
        Debug.LogError($"Join room failed: {message} (Code: {returnCode})");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name}, Players: {PhotonNetwork.PlayerList.Length}");
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"Master Client loading intermediate scene index {intermediateSceneIndex}");
            if (SceneManager.sceneCountInBuildSettings > intermediateSceneIndex)
            {
                PhotonNetwork.LoadLevel(intermediateSceneIndex);
            }
            else
            {
                Debug.LogError($"Intermediate scene index {intermediateSceneIndex} not found in Build Settings");
            }
        }
    }
}