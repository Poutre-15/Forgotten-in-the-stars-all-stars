using UnityEngine;
using Photon.Pun; // Add Photon.Pun for networking

public class ButtonManager : MonoBehaviourPun // Inherit from MonoBehaviourPun
{
    public static ButtonManager Instance;

    [SerializeField] private int totalButtons = 4;
    private int buttonsPressed = 0;

    [SerializeField] private GameObject door; // The door to destroy (must have Photon View)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        // Ensure this script has a Photon View if it needs to send RPCs
        if (GetComponent<PhotonView>() == null)
        {
            gameObject.AddComponent<PhotonView>();
        }
    }

    public void NotifyButtonPressed()
    {
        // Call the RPC to increment the counter on all clients
        photonView.RPC("IncrementButtonCount", RpcTarget.All);
    }

    [PunRPC]
    private void IncrementButtonCount()
    {
        buttonsPressed++;
        Debug.Log($"Button pressed! Total: {buttonsPressed}/{totalButtons}");

        if (buttonsPressed >= totalButtons)
        {
            DestroyDoor();
        }
    }

    private void DestroyDoor()
    {
        Debug.Log("All buttons pressed! Destroying door for all players.");
        if (door != null)
        {
            // Use PhotonNetwork.Destroy to destroy the door across the network
            if (door.GetComponent<PhotonView>() != null)
            {
                PhotonNetwork.Destroy(door);
            }
            else
            {
                Debug.LogError("Door does not have a Photon View! Can't destroy over network.");
                Destroy(door); // Fallback to local destroy (won't sync)
            }
        }
    }
}