# Sistema Multiplayer para Jogo de Corrida

## 📋 Visão Geral

Sistema de sincronização multiplayer baseado em WebSocket para jogos de corrida. Cada jogador controla seu próprio carro com física local, e a posição/rotação é sincronizada via rede para os outros jogadores.

## 🎯 Como Funciona

### Arquitetura
- **Carro Local**: Física completa (CarPhysics), envia posição a cada 0.05s (20 updates/segundo)
- **Carros Remotos**: Recebem posição/rotação e interpolam suavemente, sem física ativa
- **Servidor**: Apenas repassa mensagens entre clientes (servidor genérico WebSocket)

### Componentes Principais

1. **NetworkCarSync** - Em cada carro
   - Gerencia se é local ou remoto
   - Envia/recebe dados de posição

2. **RaceNetworkManager** - Um por cena
   - Coordena todos os carros
   - Spawna carros remotos
   - Distribui mensagens

3. **NetworkManager** - Singleton persistente
   - Gerencia conexão WebSocket
   - Controla modo local/online

## 🚀 Setup Rápido

### Passo 1: Adicionar na Cena

Crie 3 GameObjects vazios:
```
Scene/
├── NetworkManager (NetworkManager.cs)
├── WebSocketConnection (WSSampleConn.cs)
└── RaceNetworkManager (RaceNetworkManager.cs)
```

### Passo 2: Configurar WebSocket

No Inspector do `WebSocketConnection`:
- **Server URL**: `ws://genericserverwebsocket.onrender.com/:8080`
- **Game**: `corrida` (ou nome do seu jogo)
- **Auto Connect On Start**: `false`
- **Auto Reconnect**: `true`

### Passo 3: Configurar NetworkManager

No Inspector do `NetworkManager`:
- Arraste `WebSocketConnection` para o campo `webSocketConnection`

### Passo 4: Configurar Carro Local

No GameObject do carro que o jogador controla:
1. Adicione o componente `NetworkCarSync`
2. Configure no Inspector:
   - ✅ **Is Local Car**: `true`
   - **Car Physics**: Arraste o componente CarPhysics
   - **Car Body**: Arraste o Rigidbody do corpo do carro
   - **Sync Interval**: `0.05` (20 updates/seg)
   - **Interpolation Speed**: `15`

### Passo 5: Criar Prefab do Carro Remoto

1. Duplique o prefab do carro local
2. Renomeie para `RemoteCar`
3. No `NetworkCarSync`:
   - ❌ **Is Local Car**: `false`
4. No `CarPhysics`:
   - Desmarque ou remova scripts de controle

### Passo 6: Configurar RaceNetworkManager

No Inspector do `RaceNetworkManager`:
- **Car Prefab**: Arraste o prefab `RemoteCar`
- **Spawn Points**: Crie Transforms vazios nas posições de largada e arraste para este array
- **Max Players**: `8` (ou quantos quiser)

### Passo 7: Adicionar Helper (Opcional)

Crie um GameObject vazio `RaceSetup` e adicione `RaceSetupHelper.cs`:
- **Local Player Car**: Arraste o NetworkCarSync do carro local
- **Race Start Delay**: `3` segundos

## 🎮 Iniciar o Jogo

### Modo Local (Teste)
```csharp
NetworkManager.Instance.StartLocalGame();
```

### Modo Online
```csharp
NetworkManager.Instance.StartOnlineGame();
```

### Iniciar Corrida
```csharp
// Após conectar e spawnar todos os carros
RaceNetworkManager.Instance.StartRace();
```

## 📝 Exemplo de Integração com UI

```csharp
public class MenuController : MonoBehaviour
{
    public void OnClickLocalGame()
    {
        NetworkManager.Instance.StartLocalGame();
        RaceNetworkManager.Instance.StartRace();
    }
    
    public void OnClickOnlineGame()
    {
        NetworkManager.Instance.StartOnlineGame();
        
        // Aguarda todos conectarem
        var ws = NetworkManager.Instance.webSocketConnection;
        ws.OnSessionReady += () => {
            // Contagem regressiva de 3 segundos
            StartCoroutine(CountdownAndStart());
        };
    }
    
    IEnumerator CountdownAndStart()
    {
        Debug.Log("3...");
        yield return new WaitForSeconds(1);
        Debug.Log("2...");
        yield return new WaitForSeconds(1);
        Debug.Log("1...");
        yield return new WaitForSeconds(1);
        Debug.Log("GO!");
        RaceNetworkManager.Instance.StartRace();
    }
}
```

## 🔧 Ajustes de Performance

### Reduzir Bandwidth
Diminua a frequência de sincronização no `NetworkCarSync`:
```csharp
public float syncInterval = 0.1f; // 10 updates/seg (ao invés de 20)
```

### Suavizar Movimento Remoto
Aumente a interpolação no `NetworkCarSync`:
```csharp
public float interpolationSpeed = 20f; // Mais suave
```

### Limitar Jogadores
No `RaceNetworkManager`:
```csharp
public int maxPlayers = 4; // Limite para melhor performance
```

## 🐛 Troubleshooting

### Carro não se move
- Verifique se `CarPhysics.canMove = true`
- Verifique se `NetworkCarSync.isLocalCar = true`
- Verifique se `RaceNetworkManager.StartRace()` foi chamado

### Carro remoto não aparece
- Verifique se o prefab está configurado em `RaceNetworkManager.carPrefab`
- Verifique se os spawn points existem
- Verifique se o outro jogador conectou (logs)

### Movimento remoto travado/jerky
- Aumente `interpolationSpeed` no NetworkCarSync
- Reduza `syncInterval` para enviar mais updates
- Verifique latência da conexão

### Não conecta ao servidor
- Verifique a URL do servidor
- Verifique se o servidor está online
- Veja os logs no Console do Unity

## 📊 Formato de Mensagens

### CarSyncData (enviado a cada syncInterval)
```json
{
  "carId": "player-uuid",
  "posX": 10.5, "posY": 0.2, "posZ": 5.3,
  "rotX": 0.0, "rotY": 0.7, "rotZ": 0.0, "rotW": 0.7,
  "velX": 5.0, "velY": 0.0, "velZ": 10.0,
  "timestamp": 123.456
}
```

## 🎯 Próximos Passos

Para um sistema de produção, considere adicionar:
- [ ] Predição de movimento (client-side prediction)
- [ ] Reconciliação de estado (server reconciliation)
- [ ] Compressão de dados (enviar apenas deltas)
- [ ] Dead reckoning (extrapolação quando perder pacotes)
- [ ] Lag compensation
- [ ] Servidor autoritativo (validar colisões no servidor)
