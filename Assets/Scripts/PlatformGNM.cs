using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


public enum NetcodeMsgType
{
	ChatMessage = MsgType.Highest + 1, // First contact message
	BatchedPositionUpdate = MsgType.Highest + 2,
	PositionUpdateRequest = MsgType.Highest + 3,
}


public class NCMessage : MessageBase
{
	public int SourceClient;
	public string Message;
	public bool Reliable = true;
}


public class PlatformGNM : NetworkManager
{
	[ShowInInspector]
	public int ConnectionID
	{
		get { return _myConnectionId; }
	}
	// -2 = none, -1 = all, 0 = server, etc.
	public int _myConnectionId = -2;

	public bool IsServer { get; private set; }
	public bool IsClient { get; private set; }

	public bool IsRunning { get { return IsServer || IsClient; } }

	// Probably needed
	private Dictionary<int, NCGameObject> _TrackedObjects = new Dictionary<int, NCGameObject>();

	private string ServerIP;

	/// <summary>
	/// Send a message to all clients (or server first if you are a client)
	/// </summary>
	/// <param name="type"></param>
	/// <param name="data"></param>
	/// <param name="sourceConnection"></param>
	public void SendData(short type, string data, int sourceConnection = 0, int targetConn = -1)
	{
		if (!IsServer && client == null) return;

		var message = new NCMessage
		{
			Message = data,
			SourceClient = sourceConnection
		};
		if (IsServer)
		{
			
			if (targetConn == sourceConnection)
			{
				// Handle immediately - Is host client talking to host server
				OnClientMessageRecieved(message, type);
			}
			else if (targetConn == -1)
			{
				Log("Sending server message to all on behalf of " + sourceConnection + ": " + type + " - " + data);
				NetworkServer.SendToAll(type, message);
			}
			else
				NetworkServer.SendToClient(targetConn, type, message);
		}
		else
		{
			Log("CLient sending server message");
			message.SourceClient = _myConnectionId;
			client.Send(type, message);
		}

	}

	public void SendDataUnreliable(short type, string data, int sourceConnection = 0, int targetConn = -1)
	{
		if (!IsServer && client == null) return;

		var message = new NCMessage
		{
			Message = data,
			SourceClient = sourceConnection
		};
		if (IsServer)
		{
			
			if (targetConn == sourceConnection)
			{
				// Handle immediately - Is host client talking to host server
				OnClientMessageRecieved(message, type);
			}
			else if (targetConn == -1)
			{
				Log("Sending server message to all on behalf of " + sourceConnection + ": " + type + " - " + data);
				NetworkServer.SendUnreliableToAll(type, message);
			}
			else
				NetworkServer.SendToClient(targetConn, type, message);
		}
		else
		{
			Log("Client sending server message");
			message.SourceClient = _myConnectionId;
			client.SendUnreliable(type, message);
		}
	}

	/// <summary>
	/// Client got a message from Server. Do it unless it came from yourself.
	/// </summary>
	/// <param name="netMsg"></param>
	public void OnClientMessageRecieved(NetworkMessage netMsg)
	{
		//NCMessage msg = netMsg.ReadMessage<NCMessage>();

		OnClientMessageRecieved(netMsg.ReadMessage<NCMessage>(), netMsg.msgType);
	}

	public void OnClientMessageRecieved(NCMessage msg, short msgType)
	{
		// Don't double do things from yourself
		if (msg.SourceClient == _myConnectionId) return;

		LogWarning("Got message from " + msg.SourceClient + " - " + msg.Message);
		// DO STUFF WITH THE MESSAGE
		// TODO - giant case statement here
		switch (msgType)
		{
			case (short)NetcodeMsgType.ChatMessage:
				Servicer.Instance.ChatManager.ChatMessageReceived(msg.Message, msg.SourceClient);
				break;
			case (short)NetcodeMsgType.BatchedPositionUpdate:
				Servicer.Instance.TrackedObjects.SetPositions(msg.Message);
				break;
			case (short)NetcodeMsgType.PositionUpdateRequest:
				Servicer.Instance.TrackedObjects.ProcessPositionRequestBatch(msg.Message);
				break;
		}

	}

	public void OnServerMessageRecieved(NetworkMessage netMsg)
	{
		NCMessage msg = netMsg.ReadMessage<NCMessage>();
		LogWarning("SENT Server message: " + msg.Message + "  from client " + netMsg.conn.connectionId);
		// Propagate it to clients
		if (msg.Reliable)
		{
			SendData(netMsg.msgType, msg.Message, netMsg.conn.connectionId);
		}
		else
		{
			SendDataUnreliable(netMsg.msgType, msg.Message, netMsg.conn.connectionId);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();

		ServerIP = System.Net.Dns.GetHostName();
		var ipEntry = System.Net.Dns.GetHostEntry(ServerIP);
		var addr = ipEntry.AddressList;
		ServerIP = addr[addr.Length - 1].ToString();


		_TrackedObjects = new Dictionary<int, NCGameObject>();
		// TODO - track stuff authoritivly 
		LogWarning("OnStartServer");
		// REGISTER MESSAGES HERE
		foreach (NetcodeMsgType messageId in Enum.GetValues(typeof(NetcodeMsgType)))
		{
			if ((short)messageId <= MsgType.Highest) continue;
			Debug.Log("Resister Server: " + (short)messageId);
			NetworkServer.RegisterHandler((short)messageId, OnServerMessageRecieved);
		}
		IsServer = true;
		_myConnectionId = 0;
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		IsServer = false;
		_TrackedObjects.Clear();
		ReloadLevel();
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		base.OnServerConnect(conn);
		LogWarning("OnServerConnect " + conn.connectionId);

		// TODO - tell server/players you exist
		string connectionMessage = "Player " + conn.connectionId + " connected to the server";
		Servicer.Instance.ChatManager.ChatMessageReceived(connectionMessage, -1, true);
		// TODO - get world state
		Servicer.Instance.TrackedObjects.SyncAllDynamicPositions(conn.connectionId);
	}

	public override void OnStartClient(NetworkClient c)
	{
		base.OnStartClient(c);
		LogWarning("OnStartClient:");
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		LogWarning("OnClientConnect: " + conn.connectionId);
		// REGISTER MESSAGES HERE
		foreach (NetcodeMsgType messageId in Enum.GetValues(typeof(NetcodeMsgType)))
		{
			if ((short)messageId <= MsgType.Highest) continue;
			Debug.Log("Resister Client: " + (short)messageId);
			client.RegisterHandler((short)messageId, OnClientMessageRecieved);
		}
		base.OnClientConnect(conn);
		_myConnectionId = client.connection.connectionId;
		IsClient = true;
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		base.OnServerConnect(conn);
		var playerId = conn.connectionId;
		LogWarning("OnServerDisconnect " + playerId);
		string connectionMessage = "Player " + playerId + " disconnected from the server";
		Servicer.Instance.ChatManager.ChatMessageReceived(connectionMessage, -1, true);
		// Player left
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		// Called on each client when server disconnects
		base.OnClientDisconnect(conn);
		LogWarning("OnClientDisconnect: " + conn.connectionId);
		if (conn.lastError != NetworkError.Ok)
		{
			Debug.LogError("ClientDisconnected due to error: " + conn.lastError);
		}

		Cleanup();
	}

	public override void OnStopClient()
	{
		// This hook is called when a client is stopped.
		// needed?
		LogWarning("OnStopClient");
		base.OnStopClient();
		Cleanup();
	}

	private void Cleanup()
	{
		// When we kill everything 
		IsClient = false;
		NetworkServer.ClearHandlers();
		_myConnectionId = -2;
		_TrackedObjects.Clear();
		ReloadLevel();
	}

	private void ReloadLevel()
	{
		var scene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(scene.name);
	}


	private void Log(string message)
	{
		Debug.LogFormat("{0} - frame {1}: {2}", DateTime.UtcNow, Time.frameCount, message);
	}
	private void LogWarning(string message)
	{
		Debug.LogWarningFormat("{0} - frame {1}: {2}", DateTime.UtcNow, Time.frameCount, message);
	}

	void OnGUI()
	{
		if (!IsServer) return;

		GUI.Label(new Rect(10, 10, 100, 20), ServerIP);
	}

}

