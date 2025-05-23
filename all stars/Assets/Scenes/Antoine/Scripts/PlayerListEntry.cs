using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


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
    
    
    public void Initialize(ulong clientId, PlayerNetworkBehaviour playerBehaviour)
    {
        this.clientId = clientId;
        this.playerBehaviour = playerBehaviour;
        this.isHost = clientId == 0; 
        
        SetupKickButton();
        
        UpdateDisplay(playerBehaviour);
    }
    
    
    public void UpdateDisplay(PlayerNetworkBehaviour playerBehaviour)
    {
        if (playerBehaviour == null)
            return;
        
        this.playerBehaviour = playerBehaviour;
        
        UpdatePlayerName();
        
        UpdateReadyStatus();
        
        UpdateHostIndicator();
    }
    
    private void SetupKickButton()
    {
        if (kickButton != null)
        {
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
        
        if (readyStatusText != null)
        {
            readyStatusText.text = isReady ? "Ready" : "Not Ready";
            readyStatusText.color = isReady ? readyColor : notReadyColor;
        }
        
        if (readyStatusIcon != null)
        {
            readyStatusIcon.color = isReady ? readyColor : notReadyColor;
            
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
        
        bool shouldKick = true; 
        
        if (shouldKick && playerBehaviour != null)
        {
            string playerName = playerBehaviour.GetPlayerName();
            Debug.Log($"Kicking player: {playerName} (ID: {clientId})");
            
            var localPlayerBehaviour = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetworkBehaviour>();
            if (localPlayerBehaviour != null)
            {
                localPlayerBehaviour.KickPlayer(clientId, "Kicked by host");
            }
        }
    }
    
    
    public ulong GetClientId()
    {
        return clientId;
    }
    
    
    public PlayerNetworkBehaviour GetPlayerBehaviour()
    {
        return playerBehaviour;
    }
    
    
    public bool IsHost()
    {
        return isHost;
    }
}