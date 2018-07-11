using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BatchedPositionData
{
	public List<PriorityValue<Vector3>> Positions;

}

public class PositionRequest
{
	public int Id;
	public int Priority;
	public Vector3 Position;
}

public class BatchedPositionRequest
{
	public List<PositionRequest> Requests;
}

[Serializable]
public struct PriorityValue<T>
{
	public int Priority;
	public T Value;

	public PriorityValue(int priority, T value)
	{
		Priority = priority;
		Value = value;
	}
}

public class TrackedPool : MonoBehaviour {

	public Dictionary<int, NCGameObject> PositionTrackedObjects = new Dictionary<int, NCGameObject>();

	public Dictionary<int, PriorityValue<Vector3>> PositionBatch = new Dictionary<int, PriorityValue<Vector3>>();

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
				var list = PositionBatch.Select(entry => new PriorityValue<Vector3>()
				{
					Priority = entry.Key,
					Value = entry.Value.Value
				}).ToList();
				var data = new BatchedPositionData { Positions = list };
				SetPositions(JsonUtility.ToJson(data), true);
			}
			else
			{
				var list = PositionBatch.Select(entry => new PositionRequest
				{
					Id = entry.Key, Priority = entry.Value.Priority, Position = entry.Value.Value
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
				PositionBatch[id] = new PriorityValue<Vector3>(connection, position);
			}
		}
		else
		{
			PositionBatch[id] = new PriorityValue<Vector3>(connection, position);
		}
	}

	public void SetPositions(string message, bool broadcast = false)
	{

		// convert message to list

		foreach (var entry in PositionBatch)
		{
			PositionTrackedObjects[entry.Key].transform.position = entry.Value.Value;
		}
		if (broadcast)
		{
			var toSend = new BatchedPositionData { Positions = PositionBatch.Values.ToList() };
			Debug.LogWarning(JsonUtility.ToJson(toSend));
			Servicer.Instance.Netcode.SendDataUnreliable(
					(short)NetcodeMsgType.PositionUpdateRequest,
					JsonUtility.ToJson(toSend),
					targetConn: 0);
		}
		PositionBatch.Clear();
	}

}
