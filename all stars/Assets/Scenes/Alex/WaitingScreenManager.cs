using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;

public class WaitingScreenManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TMP_Text waitingText;
    public GameObject startButton; // Reference for enabling/disabling

    [Header("Scene Settings")]
    public int dialogueSceneIndex = 4; // Dialogue scene index

    void Start()
    {
        if (waitingText == null)
        {
            Debug.LogError("WaitingText is not assigned in WaitingScreenManager", this);
            return;
        }
        if (startButton == null)
        {
            Debug.LogError("StartButton is not assigned in WaitingScreenManager", this);
            return;
        }

        Debug.Log($"IsMasterClient: {PhotonNetwork.IsMasterClient}, NetworkClientState: {PhotonNetwork.NetworkClientState}");
        UpdateUI();
        startButton.SetActive(PhotonNetwork.IsMasterClient);
        if (PhotonNetwork.IsMasterClient && startButton.activeSelf)
        {
            Debug.Log("Start button enabled for Master Client");
        }
        else
        {
            Debug.LogWarning("Start button not enabled: Not Master Client or button inactive");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateUI();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"Master Client switched to {newMasterClient.NickName}");
        startButton.SetActive(PhotonNetwork.IsMasterClient);
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Start button enabled for new Master Client");
        }
    }

    void UpdateUI()
    {
        if (waitingText != null)
        {
            waitingText.text = $"Waiting for Players: {PhotonNetwork.PlayerList.Length}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        }
    }

    public void OnStartButtonClicked()
    {
        Debug.Log("OnStartButtonClicked called");
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("Cannot load scene: Not connected to Photon Master Server");
            return;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"Master Client loading dialogue scene index {dialogueSceneIndex}");
            if (UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings > dialogueSceneIndex)
            {
                PhotonNetwork.LoadLevel(dialogueSceneIndex);
            }
            else
            {
                Debug.LogError($"Dialogue scene index {dialogueSceneIndex} not found in Build Settings", this);
            }
        }
        else
        {
            Debug.LogWarning("OnStartButtonClicked: Not Master Client, ignoring");
        }
    }
}