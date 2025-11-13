using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Cliente WS nativo (.NET ClientWebSocket) para testar seu servidor.
/// Fluxo: Connect() -> envia {type:"hello", game:"<game>"} -> logs de welcome/peer/event/error.
/// Métodos públicos: Connect, SendText, SendMove, Leave, Disconnect.
/// </summary>
public class WSSampleConn : MonoBehaviour
{
    [Header("Server")]
    [Tooltip("ws://localhost:8080  ou  wss://<seuapp>.onrender.com")]
    public string serverUrl = "ws://genericserverwebsocket.onrender.com/:8080";
    public string game = "xadrez";

    [Header("Behaviour")]
    public bool autoConnectOnStart = true;
    public bool autoReconnect = true;
    public float reconnectDelaySeconds = 2f;
    public float pingIntervalSeconds = 20f;
    public INetworkManager networkManager;

    private ClientWebSocket socket;
    private CancellationTokenSource cts;
    private Task receiveTask;
    private float lastPing;

    private string netId;
    private string sessionId;
    private int sessionSize;
    private int capacity;

    // Para despachar logs/chamadas de volta à thread principal do Unity
    private readonly ConcurrentQueue<Action> mainThread = new ConcurrentQueue<Action>();
    
    // Propriedades públicas para acesso aos dados da sessão
    public string GetSessionId() => sessionId;
    public int GetSessionSize() => sessionSize;
    public int GetCapacity() => capacity;
    
    // Evento para notificar quando a sessão está cheia
    public event Action OnSessionReady;

    void Start()
    {
        Debug.Log("[WSSampleConn] Start chamado. autoConnectOnStart=" + autoConnectOnStart);
        if (autoConnectOnStart) Connect();
    }

    void Update()
    {
        // executa callbacks enfileirados por threads de rede
        while (mainThread.TryDequeue(out var action))
            action?.Invoke();

        // ping de app removido - o servidor não suporta esse tipo
    }

    void OnDestroy()  { _ = Disconnect(); }
    void OnApplicationQuit() { _ = Disconnect(); }

    public async void Connect()
    {
        Debug.Log($"[WSSampleConn] Iniciando conexão com {serverUrl}");
        if (socket != null && (socket.State == WebSocketState.Connecting || socket.State == WebSocketState.Open))
            return;

        cts?.Cancel();
        cts = new CancellationTokenSource();

        socket = new ClientWebSocket();

        Debug.Log($"[WS] Connecting to {serverUrl} …");
        try
        {
            var uri = new Uri(serverUrl);
            await socket.ConnectAsync(uri, cts.Token);
            Debug.Log("[WS] Open");

            var hello = $"{{\"type\":\"hello\",\"game\":\"{Escape(game)}\"}}";
            await SendRawAsync(hello);

            receiveTask = Task.Run(() => ReceiveLoop(cts.Token));
        }
        catch (Exception ex)
        {
            Debug.LogError("[WS] Connect error: " + ex.Message);
            if (autoReconnect) _ = ReconnectLater();
        }
    }

    async Task ReconnectLater()
    {
        await Task.Delay(TimeSpan.FromSeconds(reconnectDelaySeconds));
        if (autoReconnect) Connect();
    }

    async Task ReceiveLoop(CancellationToken token)
    {
        Debug.Log("[WSSampleConn] ReceiveLoop iniciado");
        var buffer = new ArraySegment<byte>(new byte[64 * 1024]); // 64KB por frame
        try
        {
            while (!token.IsCancellationRequested && socket != null && socket.State == WebSocketState.Open)
            {
                var ms = new System.IO.MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await socket.ReceiveAsync(buffer, token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Debug.LogWarning($"[WS] Closed by server: {result.CloseStatus} {result.CloseStatusDescription}");
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
                        if (autoReconnect) _ = ReconnectLater();
                        return;
                    }
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                var json = Encoding.UTF8.GetString(ms.ToArray());
                RouteMessage(json);
            }
        }
        catch (OperationCanceledException) { /* ignore */ }
        catch (Exception ex)
        {
            Debug.LogError("[WS] Receive error: " + ex.Message);
            if (autoReconnect) _ = ReconnectLater();
        }
    }

    void RouteMessage(string json)
    {
        Debug.Log($"[WSSampleConn] RouteMessage chamado. JSON: {json}");
        // Envia para o NetworkManager processar na thread principal
        if (NetworkManager.Instance != null)
        {
            mainThread.Enqueue(() => NetworkManager.Instance.ProcessNetworkMessage(json));
        }
        
        string type = TryReadField(json, "type");

        switch (type)
        {
            case "welcome":
                // parse simples sem alocar models grandes
                netId = TryReadField(json, "net_id");
                sessionId = TryReadField(json, "sessionId");
                var gameName = TryReadField(json, "game");
                var capacityStr = TryReadField(json, "capacity");
                var sizeStr = TryReadField(json, "sessionSize");
                
                // Converte para int
                int.TryParse(capacityStr, out capacity);
                int.TryParse(sizeStr, out sessionSize);
                
                Debug.Log($"[WS] WELCOME net_id={netId} game={gameName} session={sessionId} size={sessionSize}/{capacity}");

                // Seta o ID e o time no NetworkManager
                if (NetworkManager.Instance != null)
                {
                    NetworkManager.Instance.playerNetworkId = netId;
                    if (sessionSize == 1)
                        NetworkManager.Instance.localPlayerTeam = Team.A;
                    else
                        NetworkManager.Instance.localPlayerTeam = Team.B;
                    Debug.Log($"[WSSampleConn] Setando playerNetworkId={netId} localPlayerTeam={NetworkManager.Instance.localPlayerTeam}");
                }
                break;

            case "peer_joined":
                sessionSize++;
                Debug.Log($"[WS] peer_joined: {json} (sessionSize agora é {sessionSize})");
                
                // Se a sessão ficou cheia, notifica
                if (sessionSize >= capacity && capacity > 0)
                {
                    mainThread.Enqueue(() => OnSessionReady?.Invoke());
                }
                break;
                
            case "peer_left":
                sessionSize--;
                Debug.Log($"[WS] peer_left: {json} (sessionSize agora é {sessionSize})");
                break;
                
            case "event":
            case "info":
            case "sessions":
            case "switched":
            case "pong":
                Debug.Log($"[WS] {type}: {json}");
                break;

            case "error":
                Debug.LogError("[WS] " + json);
                break;

            default:
                Debug.Log("[WS] msg: " + json);
                break;
        }
    }

    // —————————— API pública para seus testes ——————————

    public void SendText(string text)
    {
        Debug.Log($"[WSSampleConn] SendText chamado. Payload: {{\"type\":\"event\",\"payload\":{{\"text\":\"{Escape(text)}\"}}}} ");
        var payload = $"{{\"type\":\"event\",\"payload\":{{\"text\":\"{Escape(text)}\"}}}}";
        _ = SendRawAsync(payload);
    }

    public void SendMove(string from = "e2", string to = "e4")
    {
        Debug.Log($"[WSSampleConn] SendMove chamado. Payload: {{\"type\":\"event\",\"payload\":{{\"move\":\"{Escape(from)}{Escape(to)}\"}}}} ");
        var payload = $"{{\"type\":\"event\",\"payload\":{{\"move\":\"{Escape(from)}{Escape(to)}\"}}}}";
        _ = SendRawAsync(payload);
    }
    
    public void SendRawEvent(string eventData)
    {
        Debug.Log($"[WSSampleConn] SendRawEvent chamado. Payload: {{\"type\":\"event\",\"payload\":{eventData}}}");
        var payload = $"{{\"type\":\"event\",\"payload\":{eventData}}}";
        _ = SendRawAsync(payload);
    }

    public void Leave()
    {
        Debug.Log("[WSSampleConn] Leave chamado.");
        _ = SendRawAsync("{\"type\":\"leave\"}");
    }

    public new void SendMessage(string json)
    {
        _ = SendRawAsync(json);
    }

    public async Task Disconnect()
    {
        try
        {
            autoReconnect = false;
            cts?.Cancel();

            if (socket != null && socket.State == WebSocketState.Open)
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
        }
        catch { /* ignore */ }
        finally
        {
            socket?.Dispose();
            socket = null;
        }
    }
    async Task SendRawAsync(string json)
    {
        Debug.Log($"[WSSampleConn] SendRawAsync chamado. JSON: {json}");
        if (socket == null || socket.State != WebSocketState.Open)
        {
            Debug.LogWarning("[WS] send: not open");
            return;
        }
        var bytes = Encoding.UTF8.GetBytes(json);
        var seg = new ArraySegment<byte>(bytes);
        try { await socket.SendAsync(seg, WebSocketMessageType.Text, endOfMessage: true, cancellationToken: CancellationToken.None); }
        catch (Exception ex) { Debug.LogError("[WS] send error: " + ex.Message); }
    }

    static string Escape(string s) => s?.Replace("\\", "\\\\").Replace("\"", "\\\"");

    string TryReadType(string json) => TryReadField(json, "type");

    string TryReadField(string json, string field)
    {
        try
        {
            var key = $"\"{field}\":";
            var i = json.IndexOf(key, StringComparison.Ordinal);
            if (i < 0) return null;
            i += key.Length;

            // pula espaços/aspas
            while (i < json.Length && (json[i] == ' ' || json[i] == '\"')) { if (json[i] == '\"') { i++; break; } i++; }

            var start = i;
            // lê até aspas ou separadores
            while (i < json.Length && json[i] != '\"' && json[i] != ',' && json[i] != '}' && json[i] != '\n' && json[i] != '\r') i++;
            var val = json.Substring(start, i - start);
            return val.Trim('\"');
        }
        catch { return null; }
    }
}
