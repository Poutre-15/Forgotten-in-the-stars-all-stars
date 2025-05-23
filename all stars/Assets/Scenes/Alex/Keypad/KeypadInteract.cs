using UnityEngine;
using Photon.Pun;
using TMPro;

public class KeypadInteract : MonoBehaviourPun // Changed to MonoBehaviourPun
{
    [SerializeField] private GameObject keypadUI; // Reference to local player's keypad UI
    [SerializeField] private TextMeshProUGUI displayText; // Code display
    [SerializeField] private string correctCode = "1234"; // Set your correct code
    private string enteredCode = "";
    private bool isKeypadActive = false;

    void Start()
    {
        // Ensure keypad UI is hidden at start
        keypadUI.SetActive(false);
    }

    void Update()
    {
        // Only local player can interact
        if (photonView.IsMine && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.transform == transform)
            {
                ToggleKeypad();
            }
        }

        // Close keypad with Escape key
        if (isKeypadActive && photonView.IsMine && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleKeypad();
        }
    }

    private void ToggleKeypad()
    {
        if (photonView.IsMine) // Only local player toggles UI
        {
            isKeypadActive = !isKeypadActive;
            keypadUI.SetActive(isKeypadActive);
            Cursor.lockState = isKeypadActive ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isKeypadActive;

            if (!isKeypadActive)
            {
                enteredCode = "";
                UpdateDisplay();
            }
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
        // Add your success logic here (e.g., open door, trigger event)
    }
}