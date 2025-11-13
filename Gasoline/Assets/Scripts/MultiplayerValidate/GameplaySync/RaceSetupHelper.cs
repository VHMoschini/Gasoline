using UnityEngine;

/// <summary>
/// Script de exemplo para integrar o sistema multiplayer em uma corrida
/// INSTRUÇÕES DE USO:
/// 
/// 1. SETUP INICIAL NA CENA:
///    - Adicione NetworkManager (GameObject vazio)
///    - Adicione WSSampleConn (GameObject vazio)
///    - Adicione RaceNetworkManager (GameObject vazio)
///    - Configure o serverUrl no WSSampleConn
/// 
/// 2. CONFIGURAR CADA CARRO:
///    - Adicione NetworkCarSync no GameObject do carro
///    - Atribua o CarPhysics e o carBody no Inspector
///    - Para o carro do JOGADOR LOCAL: marque isLocalCar = true
///    - Para carros de OPONENTES: serão spawnados automaticamente
/// 
/// 3. CONFIGURAR RaceNetworkManager:
///    - Atribua o prefab do carro (com NetworkCarSync) em carPrefab
///    - Crie Transform[] com posições de spawn e atribua em spawnPoints
/// 
/// 4. INICIAR O JOGO:
///    - Jogo Local: NetworkManager.Instance.StartLocalGame()
///    - Jogo Online: NetworkManager.Instance.StartOnlineGame()
/// 
/// 5. INICIAR A CORRIDA:
///    - Quando quiser liberar os carros: RaceNetworkManager.Instance.StartRace()
///    - Isso pode ser feito após uma contagem regressiva, por exemplo
/// </summary>
public class RaceSetupHelper : MonoBehaviour
{
    [Header("Referências")]
    public NetworkCarSync localPlayerCar;
    
    [Header("Modo de Teste")]
    [Tooltip("Inicia automaticamente em modo online ao iniciar")]
    public bool autoStartOnline = false;
    
    [Tooltip("Inicia automaticamente em modo local ao iniciar")]
    public bool autoStartLocal = false;
    
    [Tooltip("Delay em segundos antes de iniciar a corrida")]
    public float raceStartDelay = 3f;
    
    void Start()
    {
        // Verifica se o setup está correto
        ValidateSetup();
        
        // Auto-start se configurado
        if (autoStartOnline)
        {
            StartOnlineRace();
        }
        else if (autoStartLocal)
        {
            StartLocalRace();
        }
    }
    
    void ValidateSetup()
    {
        bool hasErrors = false;
        
        if (NetworkManager.Instance == null)
        {
            Debug.LogError("[RaceSetupHelper] NetworkManager não encontrado na cena!");
            hasErrors = true;
        }
        
        if (RaceNetworkManager.Instance == null)
        {
            Debug.LogError("[RaceSetupHelper] RaceNetworkManager não encontrado na cena!");
            hasErrors = true;
        }
        
        if (localPlayerCar == null)
        {
            Debug.LogWarning("[RaceSetupHelper] localPlayerCar não atribuído!");
        }
        else if (!localPlayerCar.isLocalCar)
        {
            Debug.LogWarning("[RaceSetupHelper] localPlayerCar.isLocalCar deveria ser true!");
        }
        
        if (!hasErrors)
        {
            Debug.Log("[RaceSetupHelper] Setup validado com sucesso!");
        }
    }
    
    [ContextMenu("Start Local Race")]
    public void StartLocalRace()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.StartLocalGame();
            Invoke(nameof(StartRaceAfterDelay), raceStartDelay);
            Debug.Log($"[RaceSetupHelper] Corrida local iniciará em {raceStartDelay}s");
        }
    }
    
    [ContextMenu("Start Online Race")]
    public void StartOnlineRace()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.StartOnlineGame();
            // A corrida iniciará automaticamente quando todos conectarem
            Debug.Log("[RaceSetupHelper] Conectando ao servidor...");
            
            // Escuta quando a sessão estiver pronta
            if (NetworkManager.Instance.webSocketConnection != null)
            {
                NetworkManager.Instance.webSocketConnection.OnSessionReady += () =>
                {
                    Debug.Log("[RaceSetupHelper] Todos jogadores conectados!");
                    Invoke(nameof(StartRaceAfterDelay), raceStartDelay);
                };
            }
        }
    }
    
    void StartRaceAfterDelay()
    {
        if (RaceNetworkManager.Instance != null)
        {
            RaceNetworkManager.Instance.StartRace();
            Debug.Log("[RaceSetupHelper] CORRIDA INICIADA!");
        }
    }
    
    // Métodos públicos para UI
    
    public void OnLocalButtonClicked()
    {
        StartLocalRace();
    }
    
    public void OnOnlineButtonClicked()
    {
        StartOnlineRace();
    }
}
