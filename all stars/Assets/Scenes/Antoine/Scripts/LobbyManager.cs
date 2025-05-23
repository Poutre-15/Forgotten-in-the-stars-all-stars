using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Random = UnityEngine.Random;


public class LobbyManager : MonoBehaviour
{
    [Header("Configuration")]
    private const int MaxPlayers = 4;
    private const int CodeLength = 6;
    
    [Header("Dependencies")]
    public NetworkManager networkManager;
    public UnityTransport unityTransport;
    
    public static LobbyManager Instance { get; private set; }
    
    private Lobby currentLobby;
    private string currentLobbyCode;
    
    public event Action<string> OnLobbyCreated;
    public event Action OnLobbyJoined;
    public event Action<string> OnLobbyError;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (networkManager == null)
            networkManager = FindObjectOfType<NetworkManager>();
            
        if (unityTransport == null)
            unityTransport = FindObjectOfType<UnityTransport>();
    }
    
    private async void Start()
    {
        await InitializeAsync();
    }
    
    
    public async Task InitializeAsync()
    {
        try
        {
            Debug.Log("Initializing Unity Services...");
            
            await UnityServices.InitializeAsync();
            
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Signed in as: {AuthenticationService.Instance.PlayerId}");
            }
            
            Debug.Log("Unity Services initialized successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
            OnLobbyError?.Invoke($"Initialization failed: {e.Message}");
        }
    }
   
    public async Task<string> CreateLobbyWithCodeAsync()
    {
        try
        {
            Debug.Log("Creating lobby...");
            
            string lobbyCode = GenerateCode(CodeLength);
            
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            Debug.Log($"Relay allocation created. Join code: {relayJoinCode}");
            
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new System.Collections.Generic.Dictionary<string, DataObject>
                {
                    {
                        "RelayJoinCode", new DataObject(
                            visibility: DataObject.VisibilityOptions.Member,
                            value: relayJoinCode
                        )
                    }
                }
            };
            
            currentLobby = await LobbyService.Instance.CreateLobbyAsync(
                lobbyName: $"Lobby_{lobbyCode}",
                maxPlayers: MaxPlayers,
                options: createLobbyOptions
            );
            
            currentLobbyCode = lobbyCode;
            
            Debug.Log($"Lobby created successfully! Code: {lobbyCode}");
            
            ConfigureTransportAsHost(allocation);
            
            bool hostStarted = networkManager.StartHost();
            if (!hostStarted)
            {
                throw new Exception("Failed to start as host");
            }
            
            Debug.Log("Started as host successfully!");
            
            StartLobbyHeartbeat();
            
            OnLobbyCreated?.Invoke(lobbyCode);
            return lobbyCode;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
            OnLobbyError?.Invoke($"Failed to create lobby: {e.Message}");
            return null;
        }
    }
    
    public async Task JoinLobbyAsync(string code)
    {
        try
        {
            Debug.Log($"Attempting to join lobby with code: {code}");
            
            JoinLobbyByCodeOptions joinOptions = new JoinLobbyByCodeOptions();
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, joinOptions);
            currentLobbyCode = code;
            
            Debug.Log($"Successfully joined lobby: {currentLobby.Name}");
            
            if (!currentLobby.Data.ContainsKey("RelayJoinCode"))
            {
                throw new Exception("Lobby doesn't contain relay join code");
            }
            
            string relayJoinCode = currentLobby.Data["RelayJoinCode"].Value;
            Debug.Log($"Retrieved relay join code: {relayJoinCode}");
            
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            
            ConfigureTransportAsClient(joinAllocation);
            
            bool clientStarted = networkManager.StartClient();
            if (!clientStarted)
            {
                throw new Exception("Failed to start as client");
            }
            
            Debug.Log("Started as client successfully!");
            OnLobbyJoined?.Invoke();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Lobby service error: {e.Message}");
            string errorMessage = e.Reason switch
            {
                LobbyExceptionReason.LobbyNotFound => "Lobby not found. Check your code.",
                LobbyExceptionReason.LobbyFull => "Lobby is full.",
                _ => $"Failed to join lobby: {e.Message}"
            };
            OnLobbyError?.Invoke(errorMessage);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
            OnLobbyError?.Invoke($"Failed to join lobby: {e.Message}");
        }
    }
    
   
    public async Task LeaveLobbyAsync()
    {
        try
        {
            if (currentLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);
                currentLobby = null;
                currentLobbyCode = null;
                Debug.Log("Left lobby successfully");
            }
            
            if (networkManager.IsHost || networkManager.IsClient)
            {
                networkManager.Shutdown();
                Debug.Log("Network manager shutdown");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error leaving lobby: {e.Message}");
        }
    }
    
  
    private string GenerateCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] result = new char[length];
        
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[Random.Range(0, chars.Length)];
        }
        
        return new string(result);
    }
    
   
    private void ConfigureTransportAsHost(Allocation allocation)
    {
        var relayServerData = new RelayServerData(allocation, "dtls");
        unityTransport.SetRelayServerData(relayServerData);
        Debug.Log("Transport configured as host with relay data");
    }
    
  
    private void ConfigureTransportAsClient(JoinAllocation joinAllocation)
    {
        var relayServerData = new RelayServerData(joinAllocation, "dtls");
        unityTransport.SetRelayServerData(relayServerData);
        Debug.Log("Transport configured as client with relay data");
    }
    
    
    private async void StartLobbyHeartbeat()
    {
        while (currentLobby != null && networkManager.IsHost)
        {
            await Task.Delay(15000); 
            
            if (currentLobby != null)
            {
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
                    Debug.Log("Lobby heartbeat sent");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to send lobby heartbeat: {e.Message}");
                    break;
                }
            }
        }
    }
    
    
    public Lobby GetCurrentLobby()
    {
        return currentLobby;
    }
    
   
    public string GetCurrentLobbyCode()
    {
        return currentLobbyCode;
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            _ = LeaveLobbyAsync();
        }
    }
}