public interface INetworkManager
{
	void OnConnected(string playerId, string sessionId);
	void OnDisconnected(string reason);
	void OnMessageReceived(string json);
	void SendMessage(string json);
	void OnError(string errorMsg);
}
