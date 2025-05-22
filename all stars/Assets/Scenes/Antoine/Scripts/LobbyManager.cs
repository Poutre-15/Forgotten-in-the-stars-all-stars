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
    
    // Singleton pattern
    public static LobbyManager Instance { get; private set; }
    
    // Current lobby data
    private Lobby currentLobby;
    private string currentLobbyCode;
    
    // Events for UI updates
    public event Action<string> OnLobbyCreated;
    public event Action OnLobbyJoined;
    public event Action<string> OnLobbyError;
    
    private void Awake()
    {
        // Singleton setup
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
        
        // Get NetworkManager if not assigned
        if (networkManager == null)
            networkManager = FindObjectOfType<NetworkManager>();
            
        // Get UnityTransport if not assigned
        if (unityTransport == null)
            unityTransport = FindObjectOfType<UnityTransport>();
    }
    
    private async void Start()
    {
        await InitializeAsync();
    }
    
    /// <summary>
    /// Initialize Unity Services and authenticate anonymously
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            Debug.Log("Initializing Unity Services...");
            
            // Initialize Unity Services
            await UnityServices.InitializeAsync();
            
            // Sign in anonymously
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
    
    /// <summary>
    /// Create a new lobby with a generated code and start as host
    /// </summary>
    public async Task<string> CreateLobbyWithCodeAsync()
    {
        try
        {
            Debug.Log("Creating lobby...");
            
            // Generate unique lobby code
            string lobbyCode = GenerateCode(CodeLength);
            
            // Create Relay allocation for host
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            Debug.Log($"Relay allocation created. Join code: {relayJoinCode}");
            
            // Create lobby with the generated code
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
            
            // Configure Unity Transport with relay data
            ConfigureTransportAsHost(allocation);
            
            // Start as host
            bool hostStarted = networkManager.StartHost();
            if (!hostStarted)
            {
                throw new Exception("Failed to start as host");
            }
            
            Debug.Log("Started as host successfully!");
            
            // Start heartbeat to keep lobby alive
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
    
    /// <summary>
    /// Join an existing lobby using the provided code
    /// </summary>
    public async Task JoinLobbyAsync(string code)
    {
        try
        {
            Debug.Log($"Attempting to join lobby with code: {code}");
            
            // Join lobby by code
            JoinLobbyByCodeOptions joinOptions = new JoinLobbyByCodeOptions();
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, joinOptions);
            currentLobbyCode = code;
            
            Debug.Log($"Successfully joined lobby: {currentLobby.Name}");
            
            // Get relay join code from lobby data
            if (!currentLobby.Data.ContainsKey("RelayJoinCode"))
            {
                throw new Exception("Lobby doesn't contain relay join code");
            }
            
            string relayJoinCode = currentLobby.Data["RelayJoinCode"].Value;
            Debug.Log($"Retrieved relay join code: {relayJoinCode}");
            
            // Join relay allocation
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            
            // Configure Unity Transport with relay data
            ConfigureTransportAsClient(joinAllocation);
            
            // Start as client
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
    
    /// <summary>
    /// Leave the current lobby and disconnect
    /// </summary>
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
    
    /// <summary>
    /// Generate a random alphanumeric code
    /// </summary>
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
    
    /// <summary>
    /// Configure Unity Transport for host with relay data
    /// </summary>
    private void ConfigureTransportAsHost(Allocation allocation)
    {
        var relayServerData = new RelayServerData(allocation, "dtls");
        unityTransport.SetRelayServerData(relayServerData);
        Debug.Log("Transport configured as host with relay data");
    }
    
    /// <summary>
    /// Configure Unity Transport for client with relay data
    /// </summary>
    private void ConfigureTransportAsClient(JoinAllocation joinAllocation)
    {
        var relayServerData = new RelayServerData(joinAllocation, "dtls");
        unityTransport.SetRelayServerData(relayServerData);
        Debug.Log("Transport configured as client with relay data");
    }
    
    /// <summary>
    /// Send periodic heartbeat to keep lobby alive
    /// </summary>
    private async void StartLobbyHeartbeat()
    {
        while (currentLobby != null && networkManager.IsHost)
        {
            await Task.Delay(15000); // Send heartbeat every 15 seconds
            
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
    
    /// <summary>
    /// Get current lobby information
    /// </summary>
    public Lobby GetCurrentLobby()
    {
        return currentLobby;
    }
    
    /// <summary>
    /// Get current lobby code
    /// </summary>
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