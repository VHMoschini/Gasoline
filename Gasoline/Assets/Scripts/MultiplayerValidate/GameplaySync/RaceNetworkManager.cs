using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Gerencia a corrida multiplayer
/// - Registra todos os carros na corrida
/// - Distribui mensagens de sincronização para os carros corretos
/// - Controla estado da corrida (início, fim, etc)
/// </summary>
public class RaceNetworkManager : MonoBehaviour
{
    public static RaceNetworkManager Instance;
    
    [Header("Configuração")]
    [Tooltip("Prefab do carro para spawnar oponentes remotos")]
    public GameObject carPrefab;
    
    [Tooltip("Pontos de spawn dos carros (1 por jogador)")]
    public Transform[] spawnPoints;
    
    [Header("Estado da Corrida")]
    public bool raceStarted = false;
    public int maxPlayers = 8;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    // Dicionário de carros: playerId -> NetworkCarSync
    private Dictionary<string, NetworkCarSync> cars = new Dictionary<string, NetworkCarSync>();
    
    // Lista de IDs de jogadores conectados
    private List<string> connectedPlayers = new List<string>();
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    void Start()
    {
        // Escuta eventos do NetworkManager
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnMoveReceived += OnNetworkMessageReceived;
            NetworkManager.Instance.OnPlayerJoined += OnPlayerJoined;
            NetworkManager.Instance.OnPlayerLeft += OnPlayerLeft;
            
            if (showDebugLogs)
                Debug.Log("[RaceNetworkManager] Registrado nos eventos do NetworkManager");
        }
        else
        {
            Debug.LogWarning("[RaceNetworkManager] NetworkManager não encontrado!");
        }
    }
    
    void OnDestroy()
    {
        // Remove listeners
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnMoveReceived -= OnNetworkMessageReceived;
            NetworkManager.Instance.OnPlayerJoined -= OnPlayerJoined;
            NetworkManager.Instance.OnPlayerLeft -= OnPlayerLeft;
        }
    }
    
    /// <summary>
    /// Registra um carro no sistema de rede
    /// </summary>
    public void RegisterCar(NetworkCarSync carSync)
    {
        if (string.IsNullOrEmpty(carSync.carOwnerId))
        {
            Debug.LogWarning("[RaceNetworkManager] Tentando registrar carro sem ownerId!");
            return;
        }
        
        if (!cars.ContainsKey(carSync.carOwnerId))
        {
            cars.Add(carSync.carOwnerId, carSync);
            
            if (showDebugLogs)
                Debug.Log($"[RaceNetworkManager] Carro registrado: {carSync.carOwnerId} (Total: {cars.Count})");
        }
    }
    
    /// <summary>
    /// Remove um carro do sistema
    /// </summary>
    public void UnregisterCar(string ownerId)
    {
        if (cars.ContainsKey(ownerId))
        {
            cars.Remove(ownerId);
            
            if (showDebugLogs)
                Debug.Log($"[RaceNetworkManager] Carro removido: {ownerId}");
        }
    }
    
    /// <summary>
    /// Processa mensagens de rede recebidas
    /// Formato esperado: JSON com campo "carId" e dados de sincronização
    /// </summary>
    void OnNetworkMessageReceived(string from, string to)
    {
        // "from" e "to" são campos genéricos do NetworkManager
        // Vamos tentar parsear como CarSyncData
        try
        {
            // Reconstrói o payload completo
            string payloadJson = $"{{\"from\":\"{from}\",\"to\":\"{to}\"}}";
            
            // Tenta parsear como CarSyncData
            var syncData = JsonUtility.FromJson<CarSyncData>(payloadJson);
            
            if (syncData != null && !string.IsNullOrEmpty(syncData.carId))
            {
                ApplyCarSyncData(syncData);
            }
        }
        catch (Exception e)
        {
            if (showDebugLogs)
                Debug.LogWarning($"[RaceNetworkManager] Erro ao processar mensagem: {e.Message}");
        }
    }
    
    /// <summary>
    /// Aplica dados de sincronização a um carro específico
    /// </summary>
    void ApplyCarSyncData(CarSyncData data)
    {
        if (cars.ContainsKey(data.carId))
        {
            var carSync = cars[data.carId];
            
            // Só aplica se for carro remoto
            if (!carSync.isLocalCar)
            {
                carSync.ApplyRemoteUpdate(data);
                
                if (showDebugLogs && Time.frameCount % 120 == 0) // Log a cada 120 frames
                    Debug.Log($"[RaceNetworkManager] Aplicando sync do carro {data.carId}");
            }
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning($"[RaceNetworkManager] Carro não encontrado: {data.carId}");
        }
    }
    
    /// <summary>
    /// Quando um jogador entra na corrida
    /// </summary>
    void OnPlayerJoined(string playerId)
    {
        if (!connectedPlayers.Contains(playerId))
        {
            connectedPlayers.Add(playerId);
            
            if (showDebugLogs)
                Debug.Log($"[RaceNetworkManager] Jogador conectado: {playerId} (Total: {connectedPlayers.Count})");
            
            // Se não é o jogador local, spawna um carro remoto
            if (NetworkManager.Instance != null && playerId != NetworkManager.Instance.GetPlayerNetworkId())
            {
                SpawnRemoteCar(playerId);
            }
        }
    }
    
    /// <summary>
    /// Quando um jogador sai da corrida
    /// </summary>
    void OnPlayerLeft(string playerId)
    {
        if (connectedPlayers.Contains(playerId))
        {
            connectedPlayers.Remove(playerId);
            
            if (showDebugLogs)
                Debug.Log($"[RaceNetworkManager] Jogador desconectado: {playerId}");
            
            // Remove o carro do jogador
            if (cars.ContainsKey(playerId))
            {
                var carSync = cars[playerId];
                if (carSync != null && carSync.gameObject != null)
                {
                    Destroy(carSync.gameObject);
                }
                cars.Remove(playerId);
            }
        }
    }
    
    /// <summary>
    /// Spawna um carro para um jogador remoto
    /// </summary>
    void SpawnRemoteCar(string playerId)
    {
        if (carPrefab == null)
        {
            Debug.LogWarning("[RaceNetworkManager] carPrefab não definido! Não pode spawnar carro remoto.");
            return;
        }
        
        // Encontra um spawn point disponível
        Transform spawnPoint = GetAvailableSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogWarning("[RaceNetworkManager] Sem spawn points disponíveis!");
            return;
        }
        
        // Instancia o carro
        GameObject carObj = Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);
        carObj.name = $"RemoteCar_{playerId}";
        
        // Configura o NetworkCarSync
        NetworkCarSync carSync = carObj.GetComponent<NetworkCarSync>();
        if (carSync == null)
            carSync = carObj.AddComponent<NetworkCarSync>();
        
        carSync.SetIsLocal(false, playerId);
        
        if (showDebugLogs)
            Debug.Log($"[RaceNetworkManager] Carro remoto spawnado para {playerId}");
    }
    
    /// <summary>
    /// Encontra um spawn point disponível
    /// </summary>
    Transform GetAvailableSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return null;
        
        // Por enquanto, retorna baseado no número de carros
        int index = cars.Count % spawnPoints.Length;
        return spawnPoints[index];
    }
    
    /// <summary>
    /// Inicia a corrida
    /// </summary>
    public void StartRace()
    {
        raceStarted = true;
        
        // Ativa movimento em todos os carros locais
        foreach (var carPair in cars)
        {
            if (carPair.Value.isLocalCar && carPair.Value.carPhysics != null)
            {
                carPair.Value.carPhysics.canMove = true;
            }
        }
        
        if (showDebugLogs)
            Debug.Log("[RaceNetworkManager] Corrida iniciada!");
    }
    
    /// <summary>
    /// Para a corrida
    /// </summary>
    public void StopRace()
    {
        raceStarted = false;
        
        // Desativa movimento em todos os carros
        foreach (var carPair in cars)
        {
            if (carPair.Value.carPhysics != null)
            {
                carPair.Value.carPhysics.canMove = false;
            }
        }
        
        if (showDebugLogs)
            Debug.Log("[RaceNetworkManager] Corrida parada!");
    }
    
    /// <summary>
    /// Obtém o carro de um jogador específico
    /// </summary>
    public NetworkCarSync GetCar(string playerId)
    {
        return cars.ContainsKey(playerId) ? cars[playerId] : null;
    }
    
    /// <summary>
    /// Obtém todos os carros registrados
    /// </summary>
    public Dictionary<string, NetworkCarSync> GetAllCars()
    {
        return cars;
    }
}
