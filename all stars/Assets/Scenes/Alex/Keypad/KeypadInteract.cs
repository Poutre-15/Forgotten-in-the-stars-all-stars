using UnityEngine;
using Photon.Pun;
using TMPro;

public class KeypadInteract : MonoBehaviourPun
{
    [SerializeField] private GameObject keypadUI;
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private string correctCode = "5933";
    private string enteredCode = "";
    private bool isKeypadActive = false;
    public GameObject objectToRotate;
    [SerializeField] private float interactDistance = 3f;
    private Transform player;
    [SerializeField] private string gunObjectName = "Gun"; // Matches the hierarchy
    private FirstPersonController playerController;

    [Header("Button à afficher si code correct")]
    [SerializeField] private GameObject successButton;

    void Start()
    {
        keypadUI.SetActive(false);
        if (successButton != null)
            successButton.SetActive(false); // Hide the success button at start
        StartCoroutine(FindPlayerCoroutine()); // Start coroutine to find player
    }

    System.Collections.IEnumerator FindPlayerCoroutine()
    {
        while (player == null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in players)
            {
                PhotonView pv = p.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    player = p.transform;
                    Debug.Log($"Local player found: {player.name}");
                    playerController = player.GetComponent<FirstPersonController>();
                    if (playerController == null)
                    {
                        Debug.LogError("FirstPersonController component not found on player!");
                    }
                    yield break;
                }
            }
            yield return null;
        }
    }

    void Update()
    {
        if (!photonView.IsMine || player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        if (distance <= interactDistance && Input.GetKeyDown(KeyCode.E))
        {
            ToggleKeypad();
        }

        if (isKeypadActive && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleKeypad();
        }
    }

    private void ToggleKeypad()
    {
        if (!photonView.IsMine || player == null) return;

        isKeypadActive = !isKeypadActive;
        keypadUI.SetActive(isKeypadActive);
        Cursor.lockState = isKeypadActive ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isKeypadActive;

        if (playerController != null)
        {
            playerController.ToggleCameraForKeypad(isKeypadActive);
        }

        if (!isKeypadActive)
        {
            enteredCode = "";
            UpdateDisplay();
        }
    }

    public void OnNumberButtonPressed(string number)
    {
        if (photonView.IsMine && enteredCode.Length < correctCode.Length)
        {
            enteredCode += number;
            UpdateDisplay();

            photonView.RPC("RPC_NumberPressed", RpcTarget.AllBuffered, number);
        }
    }

    [PunRPC]
    private void RPC_NumberPressed(string number)
    {
        if (!photonView.IsMine)
        {
            enteredCode += number;
            UpdateDisplay();
        }

        if (enteredCode.Length == correctCode.Length)
        {
            CheckCode();
        }
    }

    public void OnClearButtonPressed()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_ClearCode", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void RPC_ClearCode()
    {
        enteredCode = "";
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        displayText.text = enteredCode;
    }

    private void CheckCode()
    {
        if (enteredCode == correctCode)
        {
            Debug.Log("Code Correct!");
            photonView.RPC("RPC_CodeSuccess", RpcTarget.All);
            ToggleKeypad();

            if (successButton != null)
                successButton.SetActive(true);
        }
        else
        {
            Debug.Log("Code Incorrect!");
            photonView.RPC("RPC_ClearCode", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void RPC_CodeSuccess()
    {
        Debug.Log("Code accepted on all clients!");
        RotateObject();
        ActivateGun();
    }

    void RotateObject()
    {
        photonView.RPC("RotateObjectRPC", RpcTarget.All);
    }

    [PunRPC]
    void RotateObjectRPC()
    {
        objectToRotate.transform.rotation = Quaternion.Euler(0, -40, 0);
    }

    void ActivateGun()
    {
        // Recherche du joueur local (tag "Player" ou nom "FirstPersonController")
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject localPlayer = null;
        foreach (GameObject p in players)
        {
            PhotonView pv = p.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                localPlayer = p;
                break;
            }
        }

        if (localPlayer == null)
        {
            Debug.LogError("Local player not found in ActivateGun!");
            return;
        }

        // Recherche de l'objet Gun dans la hiérarchie du joueur
        Transform gunTransform = localPlayer.transform.Find(gunObjectName);
        if (gunTransform != null)
        {
            gunTransform.gameObject.SetActive(true);
            Debug.Log("Gun activated successfully!");

            photonView.RPC("RPC_ActivateGun", RpcTarget.Others, localPlayer.GetComponent<PhotonView>().ViewID);
        }
        else
        {
            Debug.LogError("Gun object not found in player hierarchy!");
        }
    }

    [PunRPC]
    void RPC_ActivateGun(int playerViewID)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            PhotonView pv = p.GetComponent<PhotonView>();
            if (pv != null && pv.ViewID == playerViewID)
            {
                Transform gunTransform = p.transform.Find(gunObjectName);
                if (gunTransform != null)
                {
                    gunTransform.gameObject.SetActive(true);
                    Debug.Log("Gun activated on remote client!");
                }
                else
                {
                    Debug.LogError("Gun object not found on remote client!");
                }
                break;
            }
        }
    }
}
