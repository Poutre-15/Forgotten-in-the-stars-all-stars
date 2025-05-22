using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component for individual player entries in the lobby player list
/// Attach this to your player entry prefab
/// </summary>
public class PlayerListEntry : MonoBehaviour
{
    [Header("UI References")]
    public Text playerNameText;
    public Text readyStatusText;
    public Image readyStatusIcon;
    public Image hostIcon;
    public Button kickButton;
    
    [Header("Visual Settings")]
    public Color readyColor = Color.green;
    public Color notReadyColor = Color.red;
    public Sprite readySprite;
    public Sprite notReadySprite;
    
    private ulong clientId;
    private PlayerNetworkBehaviour playerBehaviour;
    private bool isHost;
    
    /// <summary>
    /// Initialize the player entry with client data
    /// </summary>
    public void Initialize(ulong clientId, PlayerNetworkBehaviour playerBehaviour)
    {
        this.clientId = clientId;
        this.playerBehaviour = playerBehaviour;
        this.isHost = clientId == 0; // Host is always client ID 0
        
        // Setup kick button (only show for host and if this isn't the host's own entry)
        SetupKickButton();
        
        // Initial display update
        UpdateDisplay(playerBehaviour);
    }
    
    /// <summary>
    /// Update the visual display of this player entry
    /// </summary>
    public void UpdateDisplay(PlayerNetworkBehaviour playerBehaviour)
    {
        if (playerBehaviour == null)
            return;
        
        this.playerBehaviour = playerBehaviour;
        
        // Update player name
        UpdatePlayerName();
        
        // Update ready status
        UpdateReadyStatus();
        
        // Update host indicator
        UpdateHostIndicator();
    }
    
    private void SetupKickButton()
    {
        if (kickButton != null)
        {
            // Only show kick button if we're the host and this isn't our own entry
            bool showKickButton = NetworkManager.Singleton.IsHost && clientId != NetworkManager.Singleton.LocalClientId;
            
            kickButton.gameObject.SetActive(showKickButton);
            
            if (showKickButton)
            {
                kickButton.onClick.RemoveAllListeners();
                kickButton.onClick.AddListener(OnKickButtonClicked);
            }
        }
    }
    
    private void UpdatePlayerName()
    {
        if (playerNameText != null && playerBehaviour != null)
        {
            string playerName = playerBehaviour.GetPlayerName();
            
            // Add host indicator to name if this is the host
            if (isHost)
            {
                playerName += " [HOST]";
            }
            
            playerNameText.text = playerName;
        }
    }
    
    private void UpdateReadyStatus()
    {
        if (playerBehaviour == null)
            return;
        
        bool isReady = playerBehaviour.GetPlayerReady();
        
        // Update ready status text
        if (readyStatusText != null)
        {
            readyStatusText.text = isReady ? "Ready" : "Not Ready";
            readyStatusText.color = isReady ? readyColor : notReadyColor;
        }
        
        // Update ready status icon
        if (readyStatusIcon != null)
        {
            readyStatusIcon.color = isReady ? readyColor : notReadyColor;
            
            // Change sprite if available
            if (isReady && readySprite != null)
            {
                readyStatusIcon.sprite = readySprite;
            }
            else if (!isReady && notReadySprite != null)
            {
                readyStatusIcon.sprite = notReadySprite;
            }
        }
    }
    
    private void UpdateHostIndicator()
    {
        if (hostIcon != null)
        {
            hostIcon.gameObject.SetActive(isHost);
        }
    }
    
    private void OnKickButtonClicked()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Only host can kick players");
            return;
        }
        
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.LogWarning("Host cannot kick themselves");
            return;
        }
        
        // Show confirmation dialog (optional)
        bool shouldKick = true; // You could implement a confirmation dialog here
        
        if (shouldKick && playerBehaviour != null)
        {
            string playerName = playerBehaviour.GetPlayerName();
            Debug.Log($"Kicking player: {playerName} (ID: {clientId})");
            
            // Use the local player's reference to kick the target player
            var localPlayerBehaviour = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetworkBehaviour>();
            if (localPlayerBehaviour != null)
            {
                localPlayerBehaviour.KickPlayer(clientId, "Kicked by host");
            }
        }
    }
    
    /// <summary>
    /// Get the client ID associated with this entry
    /// </summary>
    public ulong GetClientId()
    {
        return clientId;
    }
    
    /// <summary>
    /// Get the player behaviour associated with this entry
    /// </summary>
    public PlayerNetworkBehaviour GetPlayerBehaviour()
    {
        return playerBehaviour;
    }
    
    /// <summary>
    /// Check if this entry represents the host
    /// </summary>
    public bool IsHost()
    {
        return isHost;
    }
}