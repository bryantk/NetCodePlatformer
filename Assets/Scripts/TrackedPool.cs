using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BatchedPositionData
{
	public List<PriorityVector> Positions;

}

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

	public void TrackObject(int id, NCGameObject NetGo)
	{
		PositionTrackedObjects[id] = NetGo;
	}

	void LateUpdate()
	{
		// Do we have local position changes to send this frame?
		if (PositionBatch.Count != 0)
		{
			if (Servicer.Instance.Netcode.IsServer)
			{
				var list = PositionBatch.Select(entry => new PriorityVector()
				{
					Priority = entry.Key,
					Value = entry.Value.Value
				}).ToList();
				var data = new BatchedPositionData { Positions = list };
				Debug.Log("Sent: " + JsonUtility.ToJson(data));
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

		//Debug.LogErrorFormat("Updating {0} Positions", PositionBatch.Count);
		//var toSend = new BatchedPositionData { Positions = PositionBatch.Values.ToList() };
		//SetPositions(JsonUtility.ToJson(toSend), true);
	}

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

	public void SetPositions(string message, bool broadcast = false)
	{

		// convert message to list
		Debug.Log("GOT: " + message);
		BatchedPositionData data = JsonUtility.FromJson<BatchedPositionData>(message);
		foreach (var entry in data.Positions)
		{
			PositionTrackedObjects[entry.Priority].transform.position = entry.Value;
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
