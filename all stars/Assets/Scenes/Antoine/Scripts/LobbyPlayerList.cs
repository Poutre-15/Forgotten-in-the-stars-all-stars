using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the display of players in the lobby UI
/// </summary>
public class LobbyPlayerList : MonoBehaviour
{
    [Header("Player List UI")]
    public Transform playerListParent;
    public GameObject playerEntryPrefab;
    public int maxPlayers = 4;
    
    [Header("Ready Button")]
    public Button readyButton;
    public Text readyButtonText;
    
    [Header("Host Controls")]
    public Button startGameButton;
    public GameObject hostControlsPanel;
    
    // Dictionary to track player UI entries
    private Dictionary<ulong, GameObject> playerEntries = new Dictionary<ulong, GameObject>();
    private PlayerNetworkBehaviour localPlayer;
    private bool isLocalPlayerReady = false;
    
    private void Awake()
    {
        // Setup ready button
        if (readyButton != null)
        {
            readyButton.onClick.AddListener(OnReadyButtonClicked);
        }
        
        // Setup start game button
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        }
        
        // Subscribe to player events
        PlayerNetworkBehaviour.OnPlayerJoined += OnPlayerJoined;
        PlayerNetworkBehaviour.OnPlayerLeft += OnPlayerLeft;
        PlayerNetworkBehaviour.OnPlayerNameChanged += OnPlayerNameChanged;
        PlayerNetworkBehaviour.OnPlayerReadyChanged += OnPlayerReadyChanged;
    }
    
    private void Start()
    {
        RefreshPlayerList();
        UpdateHostControls();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        PlayerNetworkBehaviour.OnPlayerJoined -= OnPlayerJoined;
        PlayerNetworkBehaviour.OnPlayerLeft -= OnPlayerLeft;
        PlayerNetworkBehaviour.OnPlayerNameChanged -= OnPlayerNameChanged;
        PlayerNetworkBehaviour.OnPlayerReadyChanged -= OnPlayerReadyChanged;
    }
    
    #region Player Event Handlers
    
    private void OnPlayerJoined(ulong clientId)
    {
        Debug.Log($"Player joined: {clientId}");
        RefreshPlayerList();
        UpdateHostControls();
    }
    
    private void OnPlayerLeft(ulong clientId)
    {
        Debug.Log($"Player left: {clientId}");
        RemovePlayerEntry(clientId);
        UpdateHostControls();
    }
    
    private void OnPlayerNameChanged(ulong clientId, string newName)
    {
        Debug.Log($"Player {clientId} name changed to: {newName}");
        UpdatePlayerEntry(clientId);
    }
    
    private void OnPlayerReadyChanged(ulong clientId, bool isReady)
    {
        Debug.Log($"Player {clientId} ready state: {isReady}");
        UpdatePlayerEntry(clientId);
        
        // Update local ready button state if this is our player
        if (NetworkManager.Singleton != null && 
            NetworkManager.Singleton.LocalClientId == clientId)
        {
            isLocalPlayerReady = isReady;
            UpdateReadyButton();
        }
        
        UpdateHostControls();
    }
    
    #endregion
    
    #region UI Management
    
    private void RefreshPlayerList()
    {
        if (NetworkManager.Singleton == null || playerListParent == null)
            return;
        
        // Clear existing entries
        ClearPlayerList();
        
        // Add entries for all connected players
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            if (client.PlayerObject != null)
            {
                CreatePlayerEntry(client.ClientId, client.PlayerObject.GetComponent<PlayerNetworkBehaviour>());
            }
        }
    }
    
    private void ClearPlayerList()
    {
        foreach (var entry in playerEntries.Values)
        {
            if (entry != null)
                Destroy(entry);
        }
        playerEntries.Clear();
    }
    
    private void CreatePlayerEntry(ulong clientId, PlayerNetworkBehaviour playerBehaviour)
    {
        if (playerEntryPrefab == null || playerListParent == null || playerBehaviour == null)
            return;
        
        // Don't create duplicate entries
        if (playerEntries.ContainsKey(clientId))
            return;
        
        // Instantiate player entry UI
        GameObject entry = Instantiate(playerEntryPrefab, playerListParent);
        playerEntries[clientId] = entry;
        
        // Get UI components
        var entryComponent = entry.GetComponent<PlayerListEntry>();
        if (entryComponent != null)
        {
            entryComponent.Initialize(clientId, playerBehaviour);
        }
        else
        {
            // Fallback if PlayerListEntry component doesn't exist
            SetupBasicPlayerEntry(entry, clientId, playerBehaviour);
        }
        
        // Store reference to local player
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            localPlayer = playerBehaviour;
        }
    }
    
    private void SetupBasicPlayerEntry(GameObject entry, ulong clientId, PlayerNetworkBehaviour playerBehaviour)
    {
        // Find UI components in the prefab (basic setup)
        var nameText = entry.GetComponentInChildren<Text>();
        if (nameText != null)
        {
            nameText.text = $"{playerBehaviour.GetPlayerName()} {(playerBehaviour.GetPlayerReady() ? "(Ready)" : "(Not Ready)")}";
        }
        
        // Add host indicator
        if (clientId == 0) // Host is client ID 0
        {
            if (nameText != null)
                nameText.text += " [HOST]";
        }
    }
    
    private void UpdatePlayerEntry(ulong clientId)
    {
        if (!playerEntries.ContainsKey(clientId))
            return;
        
        var entry = playerEntries[clientId];
        if (entry == null)
            return;
        
        // Get player behaviour
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var playerBehaviour = client.PlayerObject?.GetComponent<PlayerNetworkBehaviour>();
            if (playerBehaviour != null)
            {
                var entryComponent = entry.GetComponent<PlayerListEntry>();
                if (entryComponent != null)
                {
                    entryComponent.UpdateDisplay(playerBehaviour);
                }
                else
                {
                    SetupBasicPlayerEntry(entry, clientId, playerBehaviour);
                }
            }
        }
    }
    
    private void RemovePlayerEntry(ulong clientId)
    {
        if (playerEntries.TryGetValue(clientId, out var entry))
        {
            if (entry != null)
                Destroy(entry);
            playerEntries.Remove(clientId);
        }
    }
    
    #endregion
    
    #region Button Handlers
    
    private void OnReadyButtonClicked()
    {
        if (localPlayer == null)
        {
            Debug.LogWarning("Local player not found");
            return;
        }
        
        bool newReadyState = !isLocalPlayerReady;
        localPlayer.SetPlayerReady(newReadyState);
        
        Debug.Log($"Setting ready state to: {newReadyState}");
    }
    
    private void OnStartGameButtonClicked()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Only host can start the game");
            return;
        }
        
        // Check if all players are ready
        if (localPlayer != null && localPlayer.AreAllPlayersReady())
        {
            Debug.Log("Starting game for all players...");
            localPlayer.StartGameForAllPlayers();
        }
        else
        {
            Debug.LogWarning("Not all players are ready!");
            // You could show a UI message here
        }
    }
    
    #endregion
    
    #region UI Updates
    
    private void UpdateReadyButton()
    {
        if (readyButton == null || readyButtonText == null)
            return;
        
        if (isLocalPlayerReady)
        {
            readyButtonText.text = "Not Ready";
            readyButton.GetComponent<Image>().color = Color.red;
        }
        else
        {
            readyButtonText.text = "Ready";
            readyButton.GetComponent<Image>().color = Color.green;
        }
    }
    
    private void UpdateHostControls()
    {
        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        
        // Show/hide host controls
        if (hostControlsPanel != null)
        {
            hostControlsPanel.SetActive(isHost);
        }
        
        // Update start game button
        if (startGameButton != null && isHost)
        {
            bool allReady = localPlayer != null && localPlayer.AreAllPlayersReady();
            int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
            
            startGameButton.interactable = allReady && playerCount >= 2; // Minimum 2 players
            
            // Update button text
            var buttonText = startGameButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                if (playerCount < 2)
                {
                    buttonText.text = "Need More Players";
                }
                else if (!allReady)
                {
                    buttonText.text = "Waiting for Players";
                }
                else
                {
                    buttonText.text = "Start Game";
                }
            }
        }
    }
    
    #endregion
}