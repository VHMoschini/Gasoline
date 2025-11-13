using UnityEngine;

/// <summary>
/// Script de setup automático para o sistema multiplayer
/// Adicione este script a um GameObject na cena para configuração rápida
/// </summary>
public class MultiplayerSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [Tooltip("Cria automaticamente os componentes necessários")]
    public bool autoSetup = true;
    
    [Header("Configurações")]
    [Tooltip("URL do servidor WebSocket")]
    public string serverUrl = "ws://genericserverwebsocket.onrender.com/:8080";
    
    [Tooltip("Nome do jogo no servidor")]
    public string gameName = "xadrez";
    
    void Start()
    {
        if (autoSetup)
        {
            SetupMultiplayer();
        }
    }
    
    [ContextMenu("Setup Multiplayer")]
    public void SetupMultiplayer()
    {
        Debug.Log("[MultiplayerSetup] Configurando sistema multiplayer...");
        
        // 1. Verifica se já existe NetworkManager
        NetworkManager networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager == null)
        {
            GameObject nmObj = new GameObject("NetworkManager");
            networkManager = nmObj.AddComponent<NetworkManager>();
            Debug.Log("[MultiplayerSetup] NetworkManager criado");
        }
        
        // 2. Verifica se já existe WSSampleConn
        WSSampleConn wsConn = FindFirstObjectByType<WSSampleConn>();
        if (wsConn == null)
        {
            GameObject wsObj = new GameObject("WebSocketConnection");
            wsConn = wsObj.AddComponent<WSSampleConn>();
            Debug.Log("[MultiplayerSetup] WSSampleConn criado");
        }
        
        // 3. Configura WSSampleConn
        wsConn.serverUrl = serverUrl;
        wsConn.game = gameName;
        wsConn.autoConnectOnStart = false; // Não conecta automaticamente
        wsConn.autoReconnect = true;
        
        // 4. Conecta NetworkManager com WSSampleConn
        networkManager.webSocketConnection = wsConn;
        
        Debug.Log("[MultiplayerSetup] Setup completo!");
        Debug.Log("[MultiplayerSetup] Use NetworkManager.Instance.StartLocalGame() para jogo local");
        Debug.Log("[MultiplayerSetup] Use NetworkManager.Instance.StartOnlineGame() para jogo online");
    }
    
    [ContextMenu("Test Local Game")]
    public void TestLocalGame()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.StartLocalGame();
            Debug.Log("[MultiplayerSetup] Jogo local iniciado!");
        }
        else
        {
            Debug.LogError("[MultiplayerSetup] NetworkManager não encontrado! Execute Setup primeiro.");
        }
    }
    
    [ContextMenu("Test Online Game")]
    public void TestOnlineGame()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.StartOnlineGame();
            Debug.Log("[MultiplayerSetup] Tentando conectar online...");
        }
        else
        {
            Debug.LogError("[MultiplayerSetup] NetworkManager não encontrado! Execute Setup primeiro.");
        }
    }
}