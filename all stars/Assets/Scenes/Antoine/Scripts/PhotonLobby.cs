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
        Debug.Log("Connecté à Photon Master");
        btnCreate.interactable = true;
        btnJoin.interactable = true;

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void OnCreateClicked()
    {
        string code = Random.Range(1000, 9999).ToString();
        PhotonNetwork.CreateRoom(code, new RoomOptions { MaxPlayers = 4 });
        Debug.Log($"Room créée : {code}");
    }

    void OnJoinClicked()
    {
        string code = inputCode.text.Trim();
        if (string.IsNullOrEmpty(code)) return;
        PhotonNetwork.JoinRoom(code);
        Debug.Log($"Tentative de jointure de la room : {code}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Échec de la jointure : {message}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Room rejointe, chargement de la scène de jeu…");
        SceneManager.LoadSceneAsync(3);
    }
}
