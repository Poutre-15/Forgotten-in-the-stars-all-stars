using Unity.Netcode;
using UnityEngine;
using Unity.Collections;    



public class PlayerNetworkBehaviour : NetworkBehaviour
{
    [Header("Player Info")]
    [SerializeField] private string playerName = "Player";
    
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
    
    public static event System.Action<ulong, string> OnPlayerNameChanged;
    public static event System.Action<ulong, bool> OnPlayerReadyChanged;
    public static event System.Action<ulong> OnPlayerJoined;
    public static event System.Action<ulong> OnPlayerLeft;
    
    public override void OnNetworkSpawn()
    {
        networkPlayerName.OnValueChanged += OnPlayerNameValueChanged;
        isReady.OnValueChanged += OnPlayerReadyValueChanged;
        
        if (IsOwner)
        {
            SetPlayerNameServerRpc($"Player_{OwnerClientId}");
        }
        
        OnPlayerJoined?.Invoke(OwnerClientId);
        
        Debug.Log($"Player {OwnerClientId} spawned with name: {networkPlayerName.Value}");
    }
    
    public override void OnNetworkDespawn()
    {
        networkPlayerName.OnValueChanged -= OnPlayerNameValueChanged;
        isReady.OnValueChanged -= OnPlayerReadyValueChanged;
        
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
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
        {
            return;
        }
        
        networkPlayerName.Value = newName;
    }
    
    [ServerRpc]
    public void SetPlayerReadyServerRpc(bool ready, ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
        {
            return;
        }
        
        isReady.Value = ready;
    }
    
    #endregion
    
    #region Client RPCs (Server to Client)
    
    [ClientRpc]
    public void NotifyGameStartingClientRpc()
    {
        Debug.Log("Game is starting!");
    }
    
    [ClientRpc]
    public void KickPlayerClientRpc(string reason)
    {
        Debug.Log($"Kicked from lobby: {reason}");
        
        if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
    
    #endregion
    
    #region Public Methods
    
    
    public void SetPlayerName(string newName)
    {
        if (!IsOwner)
        {
            Debug.LogWarning("Only the owner can set player name");
            return;
        }
        
        SetPlayerNameServerRpc(newName);
    }
    
    
    public void SetPlayerReady(bool ready)
    {
        if (!IsOwner)
        {
            Debug.LogWarning("Only the owner can set ready state");
            return;
        }
        
        SetPlayerReadyServerRpc(ready);
    }
    
    
    public string GetPlayerName()
    {
        return networkPlayerName.Value.ToString();
    }
    
    
    public bool GetPlayerReady()
    {
        return isReady.Value;
    }
    
    
    public bool IsHost()
    {
        return OwnerClientId == 0;
    }
    
    #endregion
    
    #region Host-Only Methods
    
    
    public void KickPlayer(ulong clientId, string reason = "Kicked by host")
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Only host can kick players");
            return;
        }
        
        
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            
            var playerBehaviour = client.PlayerObject?.GetComponent<PlayerNetworkBehaviour>();
            playerBehaviour?.KickPlayerClientRpc(reason);
            
            
            NetworkManager.Singleton.DisconnectClient(clientId);
        }
    }
    
    
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
    
    
    public void StartGameForAllPlayers()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Only host can start the game");
            return;
        }
        
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            var playerBehaviour = client.PlayerObject?.GetComponent<PlayerNetworkBehaviour>();
            playerBehaviour?.NotifyGameStartingClientRpc();
        }
        
        
    }
    
    #endregion
}