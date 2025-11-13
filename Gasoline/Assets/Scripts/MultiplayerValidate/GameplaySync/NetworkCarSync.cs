using UnityEngine;
using System;

/// <summary>
/// Sincroniza posição e rotação de um carro via rede
/// - Se for carro LOCAL: envia posição/rotação periodicamente
/// - Se for carro REMOTO: recebe e aplica posição/rotação com interpolação
/// </summary>
public class NetworkCarSync : MonoBehaviour
{
    [Header("Identificação")]
    public string carOwnerId; // ID do jogador que controla este carro
    public bool isLocalCar = true;
    
    [Header("Sincronização")]
    [Tooltip("Intervalo em segundos para enviar posição (apenas carro local)")]
    public float syncInterval = 0.05f; // 20 updates por segundo
    
    [Tooltip("Velocidade de interpolação para carros remotos")]
    public float interpolationSpeed = 15f;
    
    [Header("Componentes")]
    public CarPhysics carPhysics;
    public Rigidbody carBody;
    
    [Header("Debug")]
    public bool showDebugLogs = false;
    
    // Controle de sincronização
    private float lastSyncTime;
    
    // Para interpolação de carros remotos
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 targetVelocity;
    
    void Start()
    {
        // Se não tem CarPhysics atribuído, tenta encontrar
        if (carPhysics == null)
            carPhysics = GetComponent<CarPhysics>();
            
        if (carBody == null && carPhysics != null)
            carBody = carPhysics.carBody;
        
        // Registra este carro no RaceNetworkManager
        if (RaceNetworkManager.Instance != null)
        {
            RaceNetworkManager.Instance.RegisterCar(this);
        }
        
        // Se for carro local, define o ownerId
        if (isLocalCar && NetworkManager.Instance != null)
        {
            carOwnerId = NetworkManager.Instance.GetPlayerNetworkId();
            if (showDebugLogs)
                Debug.Log($"[NetworkCarSync] Carro local registrado com ID: {carOwnerId}");
        }
        
        // Configura CarPhysics para permitir/bloquear controle
        if (carPhysics != null)
        {
            carPhysics.canMove = isLocalCar;
            if (showDebugLogs)
                Debug.Log($"[NetworkCarSync] CarPhysics.canMove = {isLocalCar}");
        }
        
        // Inicializa posição alvo (para carros remotos)
        if (!isLocalCar && carBody != null)
        {
            targetPosition = carBody.position;
            targetRotation = carBody.rotation;
        }
    }
    
    void Update()
    {
        if (isLocalCar)
        {
            // Carro local: envia posição periodicamente
            if (Time.time - lastSyncTime >= syncInterval)
            {
                SendPositionUpdate();
                lastSyncTime = Time.time;
            }
        }
        else
        {
            // Carro remoto: interpola para a posição alvo
            InterpolateToTarget();
        }
    }
    
    /// <summary>
    /// Envia atualização de posição/rotação para a rede (apenas carro local)
    /// </summary>
    void SendPositionUpdate()
    {
        if (carBody == null || NetworkManager.Instance == null)
            return;
            
        if (!NetworkManager.Instance.IsOnlineMode())
            return;
        
        var syncData = new CarSyncData
        {
            carId = carOwnerId,
            posX = carBody.position.x,
            posY = carBody.position.y,
            posZ = carBody.position.z,
            rotX = carBody.rotation.x,
            rotY = carBody.rotation.y,
            rotZ = carBody.rotation.z,
            rotW = carBody.rotation.w,
            velX = carBody.linearVelocity.x,
            velY = carBody.linearVelocity.y,
            velZ = carBody.linearVelocity.z,
            timestamp = Time.time
        };
        
        string payload = JsonUtility.ToJson(syncData);
        NetworkManager.Instance.SendNetworkMessage("car_sync", payload);
        
        if (showDebugLogs && Time.frameCount % 60 == 0) // Log a cada 60 frames
            Debug.Log($"[NetworkCarSync] Enviando posição: {carBody.position}");
    }
    
    /// <summary>
    /// Aplica atualização de posição recebida da rede (apenas carro remoto)
    /// </summary>
    public void ApplyRemoteUpdate(CarSyncData data)
    {
        if (isLocalCar)
            return; // Não aplica updates em carros locais
            
        targetPosition = new Vector3(data.posX, data.posY, data.posZ);
        targetRotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
        targetVelocity = new Vector3(data.velX, data.velY, data.velZ);
        
        if (showDebugLogs && Time.frameCount % 60 == 0)
            Debug.Log($"[NetworkCarSync] Recebendo posição remota: {targetPosition}");
    }
    
    /// <summary>
    /// Interpola suavemente para a posição/rotação alvo (carros remotos)
    /// </summary>
    void InterpolateToTarget()
    {
        if (carBody == null)
            return;
        
        // Interpola posição
        carBody.position = Vector3.Lerp(carBody.position, targetPosition, 
            Time.deltaTime * interpolationSpeed);
        
        // Interpola rotação
        carBody.rotation = Quaternion.Slerp(carBody.rotation, targetRotation, 
            Time.deltaTime * interpolationSpeed);
        
        // Aplica velocidade (para física mais realista)
        carBody.linearVelocity = Vector3.Lerp(carBody.linearVelocity, targetVelocity, 
            Time.deltaTime * interpolationSpeed);
    }
    
    /// <summary>
    /// Define se este carro é local ou remoto
    /// </summary>
    public void SetIsLocal(bool isLocal, string ownerId = "")
    {
        isLocalCar = isLocal;
        carOwnerId = ownerId;
        
        if (carPhysics != null)
            carPhysics.canMove = isLocal;
        
        if (!isLocal && carBody != null)
        {
            targetPosition = carBody.position;
            targetRotation = carBody.rotation;
        }
        
        if (showDebugLogs)
            Debug.Log($"[NetworkCarSync] SetIsLocal({isLocal}) para carro {ownerId}");
    }
}

/// <summary>
/// Estrutura de dados para sincronização de carro
/// </summary>
[Serializable]
public class CarSyncData
{
    public string carId;
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;
    public float velX, velY, velZ;
    public float timestamp;
}
