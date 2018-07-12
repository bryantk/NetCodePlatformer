using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;


[Serializable]
public class PositionRequest
{
	public int Id;
	public int Priority;
	public Vector3 Position;
}

[Serializable]
public class BatchedPositionRequest
{
	public List<PositionRequest> Requests;
}

[Serializable]
public struct PriorityVector
{
	public int Priority;
	public Vector3 Value;

	public PriorityVector(int priority, Vector3 value)
	{
		Priority = priority;
		Value = value;
	}
}

public class TrackedPool : MonoBehaviour
{

	public Dictionary<int, NCGameObject> PositionTrackedObjects = new Dictionary<int, NCGameObject>();

	public Dictionary<int, PriorityVector> PositionBatch = new Dictionary<int, PriorityVector>();

	public bool TrackObject(int id, NCGameObject NetGo)
	{
		if (PositionTrackedObjects.ContainsKey(id)) return false;
		PositionTrackedObjects[id] = NetGo;
		return true;
	}

	void Update()
	{
		// Do we have local position changes to send this frame?
		if (PositionBatch.Count != 0)
		{
			if (Servicer.Instance.Netcode.IsServer)
			{
				var list = PositionBatch.Select(entry => new PositionRequest()
				{
					Id = entry.Key,
					Priority = entry.Value.Priority,
					Position = entry.Value.Value
				}).ToList();
				var data = new BatchedPositionRequest { Requests = list };
				SetPositions(JsonUtility.ToJson(data), true);
			}
			else
			{
				var list = PositionBatch.Select(entry => new PositionRequest
				{
					Id = entry.Key,
					Priority = entry.Value.Priority,
					Position = entry.Value.Value
				}).ToList();
				var data = new BatchedPositionRequest { Requests = list };
				Servicer.Instance.Netcode.SendDataUnreliable(
						(short)NetcodeMsgType.PositionUpdateRequest,
						JsonUtility.ToJson(data),
						targetConn: 0);
				PositionBatch.Clear();
			}

		}
	}

	public void SyncAllDynamicPositions(int targetClient)
	{
		var list = PositionTrackedObjects.Select(entry => new PositionRequest()
		{
			Id = entry.Key,
			Priority = 0,
			Position = entry.Value.NetPosition
		}).ToList();
		var data = new BatchedPositionRequest { Requests = list };

		Servicer.Instance.Netcode.SendDataUnreliable(
					(short)NetcodeMsgType.BatchedPositionUpdate,
					JsonUtility.ToJson(data), targetConn: targetClient);
	}

	/// <summary>
	/// Update cached (yet to be sent) position of object
	/// </summary>
	/// <param name="message"></param>
	public void ProcessPositionRequestBatch(string message)
	{
		var data = JsonUtility.FromJson<BatchedPositionRequest>(message);
		foreach (var update in data.Requests)
		{
			AddPositionRequest(update.Id, update.Priority, update.Position);
		}
	}

	/// <summary>
	/// Any client can update their tracked list
	/// </summary>
	public void AddPositionRequest(int id, int connection, Vector3 position)
	{
		if (PositionBatch.ContainsKey(id))
		{
			if (connection <= PositionBatch[id].Priority)
			{
				PositionBatch[id] = new PriorityVector(connection, position);
			}
		}
		else
		{
			PositionBatch[id] = new PriorityVector(connection, position);
		}
	}

	/// <summary>
	/// Set position of objects in message
	/// </summary>
	/// <param name="message"></param>
	/// <param name="broadcast"></param>
	public void SetPositions(string message, bool broadcast = false)
	{
		BatchedPositionRequest data = JsonUtility.FromJson<BatchedPositionRequest>(message);
		foreach (var entry in data.Requests)
		{
			if (entry.Priority == Servicer.Instance.Netcode.ConnectionID) continue;

			PositionTrackedObjects[entry.Id].transform.DOMove(entry.Position, 0.1f);

			//PositionTrackedObjects[entry.Id].transform.position = entry.Position;
		}
		if (broadcast)
		{
			Servicer.Instance.Netcode.SendDataUnreliable(
					(short)NetcodeMsgType.BatchedPositionUpdate,
					message);
		}
		PositionBatch.Clear();
	}

}
