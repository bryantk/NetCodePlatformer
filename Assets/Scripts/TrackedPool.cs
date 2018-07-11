using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BatchedPositionData
{
	public List<PriorityValue<Vector3>> Positions;

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

	public Dictionary<int, GameObject> PositionTrackedObjects = new Dictionary<int, GameObject>();

	public Dictionary<int, PriorityValue<Vector3>> PositionBatch = new Dictionary<int, PriorityValue<Vector3>>();

	void LateUpdate()
	{
		if (PositionBatch.Count == 0) return;

		var toSend = new BatchedPositionData { Positions = PositionBatch.Values.ToList() };
		Servicer.Instance.Netcode.SendDataUnreliable(
				(short)NetcodeMsgType.PositionUpdateRequest,
				JsonUtility.ToJson(toSend),
				targetConn: 0);
	}


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

	}

}
