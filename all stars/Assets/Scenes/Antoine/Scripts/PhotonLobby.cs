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
    public int intermediateSceneIndex = 2; // Set to your intermediate scene index in Build Settings

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