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
    [SerializeField] private string playerObjectName = "ActivationObject";
    private FirstPersonController playerController; // Reference to FirstPersonController

    void Start()
    {
        keypadUI.SetActive(false);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<FirstPersonController>(); // Get reference
    }

    void Update()
    {
        if (!photonView.IsMine) return;

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
        if (!photonView.IsMine) return;

        isKeypadActive = !isKeypadActive;
        keypadUI.SetActive(isKeypadActive);
        Cursor.lockState = isKeypadActive ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isKeypadActive;

        // Toggle camera movement and cursor in FirstPersonController
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
            photonView.RPC("RPC_NumberPressed", RpcTarget.AllBuffered, number);
        }
    }

    [PunRPC]
    private void RPC_NumberPressed(string number)
    {
        enteredCode += number;
        UpdateDisplay();

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
        ActivateGun(); // Activate object in player prefab
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

    [PunRPC]
    void ActivateGun()
    {
        GameObject playerObj = PhotonNetwork.LocalPlayer.TagObject as GameObject;
        if (playerObj != null)
        {
            GameObject targetObject = playerObj.transform.Find(playerObjectName)?.gameObject;
            if (targetObject != null)
            {
                targetObject.SetActive(true);
            }
            else
            {
                Debug.LogError("ActivationObject not found in player prefab!");
            }
        }
    }
   
}