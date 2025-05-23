using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class LobbyUIManager : MonoBehaviour
{
    [Header("UI References - Main Menu")]
    public Button btnCreate;
    public Button btnJoin;
    public InputField inputJoinCode;
    public Text txtLobbyCode;
    
    [Header("UI References - Lobby State")]
    public GameObject panelMainMenu;
    public GameObject panelLobby;
    public Text txtLobbyStatus;
    public Text txtPlayerList;
    public Button btnLeaveLobby;
    public Button btnStartGame;
    
    [Header("UI References - Loading")]
    public GameObject loadingPanel;
    public Text txtLoadingMessage;
    
    [Header("UI References - Error")]
    public GameObject errorPanel;
    public Text txtErrorMessage;
    public Button btnCloseError;
    
    private LobbyManager lobbyManager;
    private NetworkManager networkManager;
    
    private void Awake()
    {
        lobbyManager = FindObjectOfType<LobbyManager>();
        networkManager = FindObjectOfType<NetworkManager>();
        
        if (lobbyManager == null)
        {
            Debug.LogError("LobbyManager not found in scene!");
            return;
        }
        
        SetupButtonListeners();
       
        SubscribeToLobbyEvents();
        
        SubscribeToNetworkEvents();
        
        InitializeUI();
    }
    
    private void SetupButtonListeners()
    {
        if (btnCreate != null)
            btnCreate.onClick.AddListener(OnCreateClicked);
            
        if (btnJoin != null)
            btnJoin.onClick.AddListener(OnJoinClicked);
            
        if (btnLeaveLobby != null)
            btnLeaveLobby.onClick.AddListener(OnLeaveClicked);
            
        if (btnStartGame != null)
            btnStartGame.onClick.AddListener(OnStartGameClicked);
            
        if (btnCloseError != null)
            btnCloseError.onClick.AddListener(OnCloseErrorClicked);
    }
    
    private void SubscribeToLobbyEvents()
    {
        if (lobbyManager != null)
        {
            lobbyManager.OnLobbyCreated += OnLobbyCreated;
            lobbyManager.OnLobbyJoined += OnLobbyJoined;
            lobbyManager.OnLobbyError += OnLobbyError;
        }
    }
    
    private void SubscribeToNetworkEvents()
    {
        if (networkManager != null)
        {
            networkManager.OnClientConnectedCallback += OnClientConnected;
            networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }
    
    private void InitializeUI()
    {
        ShowMainMenu();
        
        SetLoadingState(false, "");
        SetErrorState(false, "");
        
        if (inputJoinCode != null)
        {
            inputJoinCode.text = "";
            inputJoinCode.characterLimit = 6;
            inputJoinCode.onValueChanged.AddListener(OnJoinCodeChanged);
        }
        
        if (txtLobbyCode != null)
            txtLobbyCode.text = "";
    }
    
    private void OnJoinCodeChanged(string value)
    {
        if (inputJoinCode != null && inputJoinCode.text != value.ToUpper())
        {
            inputJoinCode.text = value.ToUpper();
        }
    }
    
    #region Button Event Handlers
    
    private async void OnCreateClicked()
    {
        Debug.Log("Create Party button clicked");
        
        SetLoadingState(true, "Creating lobby...");
        DisableMainMenuButtons(true);
        
        string code = await lobbyManager.CreateLobbyWithCodeAsync();
        
        SetLoadingState(false, "");
        
        if (string.IsNullOrEmpty(code))
        {
            DisableMainMenuButtons(false);
        }
    }
    
    private async void OnJoinClicked()
    {
        string code = inputJoinCode?.text?.Trim().ToUpper();
        
        if (string.IsNullOrEmpty(code))
        {
            ShowError("Please enter a lobby code");
            return;
        }
        
        if (code.Length != 6)
        {
            ShowError("Lobby code must be 6 characters");
            return;
        }
        
        Debug.Log($"Join Party button clicked with code: {code}");
        
        SetLoadingState(true, "Joining lobby...");
        DisableMainMenuButtons(true);
        
        await lobbyManager.JoinLobbyAsync(code);
        
        SetLoadingState(false, "");
    }
    
    private async void OnLeaveClicked()
    {
        Debug.Log("Leave Lobby button clicked");
        
        SetLoadingState(true, "Leaving lobby...");
        
        await lobbyManager.LeaveLobbyAsync();
        
        SetLoadingState(false, "");
        ShowMainMenu();
        DisableMainMenuButtons(false);
        
        if (txtLobbyCode != null)
            txtLobbyCode.text = "";
    }
    
    private void OnStartGameClicked()
    {
        Debug.Log("Start Game button clicked");
        
        if (networkManager.IsHost)
        {
            LoadGameScene();
        }
        else
        {
            ShowError("Only the host can start the game");
        }
    }
    
    private void OnCloseErrorClicked()
    {
        SetErrorState(false, "");
    }
    
    #endregion
    
    #region Lobby Event Handlers
    
    private void OnLobbyCreated(string code)
    {
        Debug.Log($"Lobby created with code: {code}");
        
        if (txtLobbyCode != null)
            txtLobbyCode.text = $"Code: {code}";
            
        ShowLobbyPanel();
        UpdateLobbyStatus("Waiting for players...");
        
        // Host can start the game
        if (btnStartGame != null)
            btnStartGame.gameObject.SetActive(true);
    }
    
    private void OnLobbyJoined()
    {
        Debug.Log("Successfully joined lobby");
        
        ShowLobbyPanel();
        UpdateLobbyStatus("Connected to lobby");
        
        // Clients cannot start the game
        if (btnStartGame != null)
            btnStartGame.gameObject.SetActive(false);
            
        DisableMainMenuButtons(false);
    }
    
    private void OnLobbyError(string errorMessage)
    {
        Debug.LogError($"Lobby error: {errorMessage}");
        ShowError(errorMessage);
        DisableMainMenuButtons(false);
    }

    #endregion

    #region Network Event Handlers

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
        UpdatePlayerList();

        if (networkManager.IsHost)
        {
            UpdateLobbyStatus($"Players connected: {networkManager.ConnectedClientsList.Count}/{(lobbyManager != null ? 4 : 4)}");
        }
    }


    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}");
        UpdatePlayerList();

        if (networkManager.IsHost)
        {
            UpdateLobbyStatus($"Players connected: {networkManager.ConnectedClientsList.Count}/{(lobbyManager != null ? 4 : 4)}");
        }
    }


    #endregion

    #region UI State Management

    private void ShowMainMenu()
    {
        if (panelMainMenu != null)
            panelMainMenu.SetActive(true);
            
        if (panelLobby != null)
            panelLobby.SetActive(false);
    }
    
    private void ShowLobbyPanel()
    {
        if (panelMainMenu != null)
            panelMainMenu.SetActive(false);
            
        if (panelLobby != null)
            panelLobby.SetActive(true);
    }
    
    private void DisableMainMenuButtons(bool disabled)
    {
        if (btnCreate != null)
            btnCreate.interactable = !disabled;
            
        if (btnJoin != null)
            btnJoin.interactable = !disabled;
    }
    
    private void SetLoadingState(bool isLoading, string message)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(isLoading);
            
        if (txtLoadingMessage != null)
            txtLoadingMessage.text = message;
    }
    
    private void SetErrorState(bool hasError, string message)
    {
        if (errorPanel != null)
            errorPanel.SetActive(hasError);
            
        if (txtErrorMessage != null)
            txtErrorMessage.text = message;
    }
    
    private void ShowError(string message)
    {
        SetErrorState(true, message);
    }
    
    private void UpdateLobbyStatus(string status)
    {
        if (txtLobbyStatus != null)
            txtLobbyStatus.text = status;
    }
    
    private void UpdatePlayerList()
    {
        if (txtPlayerList != null && networkManager != null)
        {
            int connectedPlayers = networkManager.ConnectedClientsList.Count;
            txtPlayerList.text = $"Players: {connectedPlayers}/4";
        }
    }
    
    #endregion
    
    private void LoadGameScene()
    {
        
        Debug.Log("Loading game scene...");
        
        if (networkManager.IsHost)
        {
        }
    }
    
    private void OnDestroy()
    {
        if (lobbyManager != null)
        {
            lobbyManager.OnLobbyCreated -= OnLobbyCreated;
            lobbyManager.OnLobbyJoined -= OnLobbyJoined;
            lobbyManager.OnLobbyError -= OnLobbyError;
        }
        
        if (networkManager != null)
        {
            networkManager.OnClientConnectedCallback -= OnClientConnected;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}