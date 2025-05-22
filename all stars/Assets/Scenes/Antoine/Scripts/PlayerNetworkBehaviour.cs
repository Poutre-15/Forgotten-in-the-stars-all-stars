using Unity.Netcode;
using UnityEngine;
using Unity.Collections;    // <-- nécessaire pour FixedString32Bytes


/// <summary>
/// Network behaviour for player objects in the lobby system
/// This script should be attached to player prefabs that need to exist across the network
/// </summary>
public class PlayerNetworkBehaviour : NetworkBehaviour
{
    [Header("Player Info")]
    [SerializeField] private string playerName = "Player";
    
    // Network variables that sync across all clients
    private NetworkVariable<FixedString32Bytes> networkPlayerName = 
        new NetworkVariable<FixedString32Bytes>(
            "Player", 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Owner
        );
    
    private NetworkVariable<bool> isReady = 
        new NetworkVariable<bool>(
            false, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Owner
        );
    
    // Events for UI updates
    public static event System.Action<ulong, string> OnPlayerNameChanged;
    public static event System.Action<ulong, bool> OnPlayerReadyChanged;
    public static event System.Action<ulong> OnPlayerJoined;
    public static event System.Action<ulong> OnPlayerLeft;
    
    public override void OnNetworkSpawn()
    {
        // Subscribe to network variable changes
        networkPlayerName.OnValueChanged += OnPlayerNameValueChanged;
        isReady.OnValueChanged += OnPlayerReadyValueChanged;
        
        // Set initial player name if this is our player
        if (IsOwner)
        {
            SetPlayerNameServerRpc($"Player_{OwnerClientId}");
        }
        
        // Notify that a player joined
        OnPlayerJoined?.Invoke(OwnerClientId);
        
        Debug.Log($"Player {OwnerClientId} spawned with name: {networkPlayerName.Value}");
    }
    
    public override void OnNetworkDespawn()
    {
        // Unsubscribe from network variable changes
        networkPlayerName.OnValueChanged -= OnPlayerNameValueChanged;
        isReady.OnValueChanged -= OnPlayerReadyValueChanged;
        
        // Notify that a player left
        OnPlayerLeft?.Invoke(OwnerClientId);
        
        Debug.Log($"Player {OwnerClientId} despawned");
    }
    
    #region Network Variable Callbacks
    
    private void OnPlayerNameValueChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        Debug.Log($"Player {OwnerClientId} name changed from {previousValue} to {newValue}");
        OnPlayerNameChanged?.Invoke(OwnerClientId, newValue.ToString());
    }
    
    private void OnPlayerReadyValueChanged(bool previousValue, bool newValue)
    {
        Debug.Log($"Player {OwnerClientId} ready state changed from {previousValue} to {newValue}");
        OnPlayerReadyChanged?.Invoke(OwnerClientId, newValue);
    }
    
    #endregion
    
    #region Server RPCs (Client to Server)
    
    [ServerRpc]
    public void SetPlayerNameServerRpc(string newName, ServerRpcParams rpcParams = default)
    {
        // Validate that the RPC comes from the owner
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
        {
            return;
        }
        
        // Update the network variable
        networkPlayerName.Value = newName;
    }
    
    [ServerRpc]
    public void SetPlayerReadyServerRpc(bool ready, ServerRpcParams rpcParams = default)
    {
        // Validate that the RPC comes from the owner
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
        {
            return;
        }
        
        // Update the network variable
        isReady.Value = ready;
    }
    
    #endregion
    
    #region Client RPCs (Server to Client)
    
    [ClientRpc]
    public void NotifyGameStartingClientRpc()
    {
        Debug.Log("Game is starting!");
        // Handle game start logic here
    }
    
    [ClientRpc]
    public void KickPlayerClientRpc(string reason)
    {
        Debug.Log($"Kicked from lobby: {reason}");
        
        // Return to main menu
        if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Set player name (only owner can call this)
    /// </summary>
    public void SetPlayerName(string newName)
    {
        if (!IsOwner)
        {
            Debug.LogWarning("Only the owner can set player name");
            return;
        }
        
        SetPlayerNameServerRpc(newName);
    }
    
    /// <summary>
    /// Set player ready state (only owner can call this)
    /// </summary>
    public void SetPlayerReady(bool ready)
    {
        if (!IsOwner)
        {
            Debug.LogWarning("Only the owner can set ready state");
            return;
        }
        
        SetPlayerReadyServerRpc(ready);
    }
    
    /// <summary>
    /// Get player name
    /// </summary>
    public string GetPlayerName()
    {
        return networkPlayerName.Value.ToString();
    }
    
    /// <summary>
    /// Get player ready state
    /// </summary>
    public bool GetPlayerReady()
    {
        return isReady.Value;
    }
    
    /// <summary>
    /// Check if this player is the host
    /// </summary>
    public bool IsHost()
    {
        return OwnerClientId == 0; // Host is always client ID 0
    }
    
    #endregion
    
    #region Host-Only Methods
    
    /// <summary>
    /// Kick a player from the lobby (host only)
    /// </summary>
    public void KickPlayer(ulong clientId, string reason = "Kicked by host")
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Only host can kick players");
            return;
        }
        
        // Find the player to kick
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            // Notify the player they're being kicked
            var playerBehaviour = client.PlayerObject?.GetComponent<PlayerNetworkBehaviour>();
            playerBehaviour?.KickPlayerClientRpc(reason);
            
            // Disconnect the client
            NetworkManager.Singleton.DisconnectClient(clientId);
        }
    }
    
    /// <summary>
    /// Check if all players are ready (host only)
    /// </summary>
    public bool AreAllPlayersReady()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            return false;
        }
        
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            var playerBehaviour = client.PlayerObject?.GetComponent<PlayerNetworkBehaviour>();
            if (playerBehaviour != null && !playerBehaviour.GetPlayerReady())
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Start the game for all players (host only)
    /// </summary>
    public void StartGameForAllPlayers()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Only host can start the game");
            return;
        }
        
        // Notify all players that the game is starting
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            var playerBehaviour = client.PlayerObject?.GetComponent<PlayerNetworkBehaviour>();
            playerBehaviour?.NotifyGameStartingClientRpc();
        }
        
        // Load the game scene (example)
        // NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
    
    #endregion
}