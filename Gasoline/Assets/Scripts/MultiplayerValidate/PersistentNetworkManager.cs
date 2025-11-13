using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Versão do NetworkManager que persiste entre cenas
/// Use esta versão se quiser menu e jogo em cenas separadas
/// </summary>
public class PersistentNetworkManager : MonoBehaviour
{
    [Header("Scene Management")]
    public string menuSceneName = "MainMenu";
    public string gameSceneName = "GameScene";
    
    [Header("Network Settings")]
    public bool isOnlineMode = false;
    public string playerNetworkId;
    public Team localPlayerTeam = Team.A;
    
    [Header("Connection")]
    public WSSampleConn webSocketConnection;
    
    public static PersistentNetworkManager Instance;
    
    // Events
    public System.Action<string, string> OnMoveReceived;
    public System.Action<Team> OnGameStarted;
    public System.Action<string> OnPlayerJoined;
    public System.Action<string> OnPlayerLeft;
    public System.Action OnConnectionLost;
    
    private bool isHost = false;
    private string opponentNetworkId;
    private bool isConnecting = false;
    
    void Awake()
    {
        // Singleton persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Escuta mudanças de cena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[PersistentNetworkManager] Cena carregada: {scene.name}");
        
        // Reconecta componentes após carregar cena
        ReconnectComponents();
        
        // Se estava conectando e chegou na cena do jogo
        if (isConnecting && scene.name == gameSceneName)
        {
            isConnecting = false;
            
            // Notifica que chegou na cena do jogo
            if (isOnlineMode && !string.IsNullOrEmpty(playerNetworkId))
            {
                // Se já tem oponente, inicia o jogo
                if (!string.IsNullOrEmpty(opponentNetworkId))
                {
                    OnGameStarted?.Invoke(Team.A);
                }
            }
            else
            {
                // Jogo local
                OnGameStarted?.Invoke(Team.A);
            }
        }
    }
    
    private void ReconnectComponents()
    {
        // Reconecta WebSocket se não existe
        if (webSocketConnection == null)
        {
            webSocketConnection = FindFirstObjectByType<WSSampleConn>();
        }
    }
    
    /// <summary>
    /// Inicia jogo local e carrega cena do jogo
    /// </summary>
    public void StartLocalGame()
    {
        Debug.Log("[PersistentNetworkManager] Iniciando jogo local");
        isOnlineMode = false;
        localPlayerTeam = Team.A;
        
        LoadGameScene();
    }
    
    /// <summary>
    /// Inicia jogo online e carrega cena do jogo quando conectar
    /// </summary>
    public void StartOnlineGame()
    {
        Debug.Log("[PersistentNetworkManager] Iniciando jogo online");
        isOnlineMode = true;
        
        if (webSocketConnection != null)
        {
            webSocketConnection.Connect();
            isConnecting = true;
        }
        else
        {
            Debug.LogError("[PersistentNetworkManager] WebSocket connection não encontrada!");
        }
    }
    
    /// <summary>
    /// Carrega a cena do jogo
    /// </summary>
    public void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }
    
    /// <summary>
    /// Volta para o menu principal
    /// </summary>
    public void LoadMenuScene()
    {
        // Desconecta se estiver online
        if (isOnlineMode && webSocketConnection != null)
        {
            webSocketConnection.Leave();
        }
        
        // Reset estado
        isOnlineMode = false;
        playerNetworkId = "";
        opponentNetworkId = "";
        isConnecting = false;
        
        SceneManager.LoadScene(menuSceneName);
    }
    
    /// <summary>
    /// Envia movimento para o oponente
    /// </summary>
    public void SendMove(string from, string to)
    {
        if (!isOnlineMode)
        {
            Debug.Log($"[PersistentNetworkManager] Movimento local: {from} -> {to}");
            return;
        }
        
        if (webSocketConnection != null)
        {
            var moveData = new
            {
                type = "move",
                from = from,
                to = to,
                player = playerNetworkId,
                team = localPlayerTeam.ToString()
            };
            
            string json = JsonUtility.ToJson(moveData);
            webSocketConnection.SendRawEvent(json);
            Debug.Log($"[PersistentNetworkManager] Enviando movimento: {from} -> {to}");
        }
    }
    
    /// <summary>
    /// Verifica se é a vez do jogador local (para jogos baseados em turnos)
    /// </summary>
    public bool IsLocalPlayerTurn()
    {
        // Para corrida, todos jogam simultaneamente
        return true;
    }
    
    /// <summary>
    /// Verifica se o jogador pode controlar este time
    /// </summary>
    public bool CanControlTeam(Team team)
    {
        if (!isOnlineMode)
            return true;
            
        return team == localPlayerTeam;
    }
    
    /// <summary>
    /// Processa mensagens recebidas do WebSocket
    /// </summary>
    public void ProcessNetworkMessage(string json)
    {
        try
        {
            var message = JsonUtility.FromJson<NetworkMessage>(json);
            
            switch (message.type)
            {
                case "welcome":
                    OnWelcomeReceived(message);
                    break;
                    
                case "peer_joined":
                    OnPeerJoined(message);
                    break;
                    
                case "peer_left":
                    OnPeerLeft(message);
                    break;
                    
                case "event":
                    ProcessGameEvent(message);
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PersistentNetworkManager] Erro ao processar mensagem: {e.Message}");
        }
    }
    
    private void OnWelcomeReceived(NetworkMessage message)
    {
        playerNetworkId = message.net_id;
        Debug.Log($"[PersistentNetworkManager] Conectado com ID: {playerNetworkId}");
        
        // Primeiro jogador é sempre Team A (host)
        if (message.sessionSize == 1)
        {
            isHost = true;
            localPlayerTeam = Team.A;
            Debug.Log("[PersistentNetworkManager] Você é o host (Team A)");
            
            // Carrega cena do jogo e aguarda segundo jogador
            LoadGameScene();
        }
        else
        {
            isHost = false;
            localPlayerTeam = Team.B;
            Debug.Log("[PersistentNetworkManager] Você é o cliente (Team B)");
            
            // Se já tem 2 jogadores, carrega cena e inicia o jogo
            if (message.sessionSize == 2)
            {
                LoadGameScene();
            }
        }
    }
    
    private void OnPeerJoined(NetworkMessage message)
    {
        opponentNetworkId = message.net_id;
        OnPlayerJoined?.Invoke(message.net_id);
        Debug.Log($"[PersistentNetworkManager] Jogador entrou: {message.net_id}");
        
        // Se somos o host e agora temos 2 jogadores, inicia o jogo
        if (isHost)
        {
            OnGameStarted?.Invoke(Team.A);
        }
    }
    
    private void OnPeerLeft(NetworkMessage message)
    {
        OnPlayerLeft?.Invoke(message.net_id);
        OnConnectionLost?.Invoke();
        Debug.Log($"[PersistentNetworkManager] Jogador saiu: {message.net_id}");
    }
    
    private void ProcessGameEvent(NetworkMessage message)
    {
        if (message.payload != null)
        {
            if (message.payload.Contains("move") || message.payload.Contains("type"))
            {
                try
                {
                    var moveData = JsonUtility.FromJson<MoveData>(message.payload);
                    if (moveData != null && moveData.player != playerNetworkId)
                    {
                        OnMoveReceived?.Invoke(moveData.from, moveData.to);
                        Debug.Log($"[PersistentNetworkManager] Movimento recebido: {moveData.from} -> {moveData.to}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PersistentNetworkManager] Erro ao processar movimento: {e.Message}");
                }
            }
        }
    }
}