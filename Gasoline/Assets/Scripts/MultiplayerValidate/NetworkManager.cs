using System;
using UnityEngine;

/// <summary>
/// Gerencia a conexão e comunicação multiplayer
/// Controla se o jogo está em modo local ou online
/// </summary>
public class NetworkManager : MonoBehaviour, INetworkManager
{
    [Header("Network Settings")]
    // public bool isOnlineMode = false; // Removido
    public string playerNetworkId;
    public Team localPlayerTeam = Team.A;
    private bool isOnlineMode = false; // Controla se está em modo online
    
    [Header("Connection")]
    public WSSampleConn webSocketConnection;
    
    public static NetworkManager Instance;
    
    // Events
    public Action<string, string> OnMoveReceived; // from, to
    public Action<Team> OnGameStarted;
    public Action<string> OnPlayerJoined;
    public Action<string> OnPlayerLeft;
    
    private bool isHost = false;
    private string opponentNetworkId;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
        if (webSocketConnection != null)
            webSocketConnection.networkManager = this;

        // Gera um ID local se não vier da internet
        if (string.IsNullOrEmpty(playerNetworkId))
        {
            playerNetworkId = Guid.NewGuid().ToString();
            Debug.Log($"[NetworkManager] Gerando playerNetworkId local: {playerNetworkId}");
            OnPlayerJoined?.Invoke(playerNetworkId);
        }
        Debug.Log("[NetworkManager] Awake chamado. webSocketConnection=" + (webSocketConnection != null));
    }
    
    void Start()
    {
        if (webSocketConnection == null)
            webSocketConnection = FindFirstObjectByType<WSSampleConn>();
            
        if (webSocketConnection != null)
        {
            // Modifica o WSSampleConn para usar nossos callbacks
            var wsConn = webSocketConnection;
            // Vamos interceptar as mensagens
        }

        Debug.Log("[NetworkManager] Start chamado. webSocketConnection=" + (webSocketConnection != null));
        if (webSocketConnection != null)
        {
            Debug.Log("[NetworkManager] webSocketConnection encontrado e configurado.");
        }
    }
    
    void Update()
    {
        // Debug com tecla M removido - use NetworkDebug para testar
    }
    
    /// <summary>
    /// Inicia jogo em modo local
    /// </summary>
    public void StartLocalGame()
    {
        isOnlineMode = false;
        Debug.Log("[NetworkManager] Iniciando jogo local");
        localPlayerTeam = Team.A;
        OnGameStarted?.Invoke(Team.A);
    }
    
    /// <summary>
    /// Inicia jogo online
    /// </summary>
    public void StartOnlineGame()
    {
        isOnlineMode = true;
        Debug.Log("[NetworkManager] Iniciando jogo online");
        if (webSocketConnection != null)
        {
            webSocketConnection.Connect();
        }
        else
        {
            Debug.LogError("[NetworkManager] WebSocket connection não encontrada!");
        }
    }
    
    /// <summary>
    /// Verifica se está em modo online
    /// </summary>
    public bool IsOnlineMode()
    {
        return isOnlineMode;
    }
    
    /// <summary>
    /// Obtém o ID do jogador local
    /// </summary>
    public string GetPlayerNetworkId()
    {
        return playerNetworkId;
    }
    
    /// <summary>
    /// Obtém o time do jogador local
    /// </summary>
    public Team GetLocalPlayerTeam()
    {
        return localPlayerTeam;
    }
    
    /// <summary>
    /// Envia mensagem genérica via WebSocket
    /// </summary>
    public void SendNetworkMessage(string messageType, string payloadJson)
    {
        if (!isOnlineMode || webSocketConnection == null)
            return;
            
        string json = $"{{\"type\":\"event\",\"payload\":{payloadJson}}}";
        Debug.Log($"[NetworkManager] Enviando mensagem: {json}");
        SendMessage(json);
    }
    
    /// <summary>
    /// Processa mensagens recebidas do WebSocket
    /// </summary>
    public void ProcessNetworkMessage(string json)
    {
        Debug.Log($"[NetworkManager] Mensagem recebida do servidor: {json}");
        Debug.Log("[NetworkManager] ProcessNetworkMessage chamado.");

        // Desserializa o JSON manualmente para extrair o payload
        var wrapper = JsonUtility.FromJson<NetworkMessage>(json);
        string payloadJson = "";

        // Extração robusta do payload (suporta objetos aninhados)
        int payloadIndex = json.IndexOf("\"payload\":");
        if (payloadIndex != -1)
        {
            int start = json.IndexOf("{", payloadIndex);
            int braceCount = 1;
            int end = start;
            for (int i = start + 1; i < json.Length && braceCount > 0; i++)
            {
                if (json[i] == '{') braceCount++;
                else if (json[i] == '}') braceCount--;
                end = i;
            }
            if (start != -1 && end != -1 && end > start)
            {
                payloadJson = json.Substring(start, end - start + 1);
            }
        }

        Debug.Log($"[NetworkManager] Payload extraído: {payloadJson}");

        if (!string.IsNullOrEmpty(payloadJson))
        {
            var moveData = JsonUtility.FromJson<MovePayload>(payloadJson);
            if (moveData != null && moveData.type == "move")
            {
                Debug.Log($"[NetworkManager] Dados recebidos do payload");
                
                // Invoca evento genérico para que outros sistemas processem
                // (RaceNetworkManager vai escutar isso)
                OnMoveReceived?.Invoke(moveData.from, moveData.to);
            }
        }
    }
    
    private void OnWelcomeReceived(NetworkMessage message)
    {
        playerNetworkId = message.net_id;
        Debug.Log($"[NetworkManager] Conectado com ID: {playerNetworkId}");
        
        // Primeiro jogador é sempre Team A (host)
        if (message.sessionSize == 1)
        {
            isHost = true;
            localPlayerTeam = Team.A;
            Debug.Log("[NetworkManager] Você é o host (Team A)");
        }
        else
        {
            isHost = false;
            localPlayerTeam = Team.B;
            Debug.Log("[NetworkManager] Você é o cliente (Team B)");
            
            if (message.sessionSize == 2)
            {
                OnGameStarted?.Invoke(Team.A);
            }
        }
    }
    
    private void OnPeerJoined(NetworkMessage message)
    {
        opponentNetworkId = message.net_id;
        OnPlayerJoined?.Invoke(message.net_id);
        Debug.Log($"[NetworkManager] Jogador entrou: {message.net_id}");
        
        // Se somos o host e agora temos 2 jogadores, inicia o jogo
        if (isHost)
        {
            OnGameStarted?.Invoke(Team.A);
        }
    }
    
    private void OnPeerLeft(NetworkMessage message)
    {
        OnPlayerLeft?.Invoke(message.net_id);
        Debug.Log($"[NetworkManager] Jogador saiu: {message.net_id}");
    }
    
    private void ProcessGameEvent(NetworkMessage message)
    {
        Debug.Log("[NetworkManager] ProcessGameEvent chamado.");
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
                        Debug.Log($"[NetworkManager] Movimento recebido: {moveData.from} -> {moveData.to}");
                        Debug.Log($"[NetworkManager] MENSAGEM DO TIME {moveData.team} RECEBIDA: {message.payload}");
                        Debug.Log("[NetworkManager] OnMoveReceived chamado via ProcessGameEvent");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[NetworkManager] Erro ao processar movimento: {e.Message}");
                    Debug.LogError($"[NetworkManager] Payload: {message.payload}");
                }
            }
        }
    }
    // INetworkManager implementation
    public void OnConnected(string playerId, string sessionId)
    {
        playerNetworkId = playerId;
        Debug.Log($"[NetworkManager] Conectado com ID: {playerId}, Sessão: {sessionId}");
    }

    public void OnDisconnected(string reason)
    {
        Debug.LogWarning($"[NetworkManager] Desconectado: {reason}");
    }

    public void OnMessageReceived(string json)
    {
        ProcessNetworkMessage(json);
    }

    public new void SendMessage(string json)
    {
        if (webSocketConnection != null)
            webSocketConnection.SendMessage(json);
    }

    public void OnError(string errorMsg)
    {
        Debug.LogError($"[NetworkManager] Erro: {errorMsg}");
    }
}

[System.Serializable]
public class NetworkMessage
{
    public string type;
    public string net_id;
    public string game;
    public string sessionId;
    public int capacity;
    public int sessionSize;
    public string payload;
}

[System.Serializable]
public class MoveData
{
    public string type;
    public string from;
    public string to;
    public string player;
    public string team;
}

[System.Serializable]
public class MovePayload
{
    public string type;
    public string from;
    public string to;
    public string player;
    public string team;
}