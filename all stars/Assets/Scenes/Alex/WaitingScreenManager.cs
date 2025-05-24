using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;

public class WaitingScreenManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TMP_Text waitingText;
    public GameObject startButton; // Reference for enabling/disabling
    public GameObject canva;
    
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

        UpdateUI();
        startButton.SetActive(PhotonNetwork.IsMasterClient);
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
        startButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    void UpdateUI()
    {
        waitingText.text = $"Waiting for Players: {PhotonNetwork.PlayerList.Length}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
    }

    public void OnStartButtonClicked()
    {
        Debug.Log("Start button clicked");
        if (PhotonNetwork.IsMasterClient)
        {
            canva.SetActive(true);
        }
    }
}