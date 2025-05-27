using UnityEngine;
using Photon.Pun;
using System.Collections;

public class BookInteraction : MonoBehaviour
{
    public Canvas instructionsCanvas; // Canvas to display instructions (assigned in Inspector)
    private Camera playerCamera;
    private bool isVisible = false;

    void Start()
    {
        // Find the local player's camera
        StartCoroutine(FindLocalPlayerCamera());
    }

    private IEnumerator FindLocalPlayerCamera()
    {
        while (PhotonNetwork.IsConnectedAndReady == false || PhotonNetwork.LocalPlayer == null)
        {
            Debug.Log("Waiting for Photon to initialize...");
            yield return null;
        }

        Debug.Log("Searching for local player...");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0)
        {
            Debug.LogWarning("No players found with tag 'Player'!");
        }

        foreach (GameObject player in players)
        {
            PhotonView photonView = player.GetComponent<PhotonView>();
            if (photonView != null && photonView.IsMine)
            {
                playerCamera = player.GetComponentInChildren<Camera>();
                if (playerCamera != null)
                {
                    Debug.Log("Found local player's camera: " + playerCamera.name + " for player " + PhotonNetwork.LocalPlayer.NickName);
                }
                else
                {
                    Debug.LogWarning("Local player found but no camera detected in its children!");
                }
                break;
            }
        }

        if (playerCamera == null)
        {
            Debug.LogError("Failed to find local player's camera!");
        }
    }

    void Update()
    {
        if (playerCamera == null || instructionsCanvas == null)
        {
            if (playerCamera == null) Debug.LogWarning("Player camera is null!");
            if (instructionsCanvas == null) Debug.LogWarning("Instructions Canvas is not assigned!");
            return;
        }

        // Check for left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse clicked by " + PhotonNetwork.LocalPlayer.NickName);
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 10f)) // 10f is the max distance
            {
                Debug.Log("Raycast hit: " + hit.transform.name);
                if (hit.transform == transform) // If the book was clicked
                {
                    Debug.Log("Book clicked by " + PhotonNetwork.LocalPlayer.NickName + ". Toggling Canvas...");
                    instructionsCanvas.enabled = !isVisible; // Toggle Canvas visibility
                    isVisible = !isVisible;
                }
            }
            else
            {
                Debug.Log("Raycast did not hit any object within 10 units.");
            }
        }
    }
}