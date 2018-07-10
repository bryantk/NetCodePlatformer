using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BatchedPositionData
{
	public List<Vector3> Positions;

}

public class TrackedPool : MonoBehaviour {

	private Dictionary<int, GameObject> _PositionTrackedObjects = new Dictionary<int, GameObject>();

	public List<GameObject> Tracked;


	void Update()
	{
		if (!Servicer.Instance.Netcode.IsServer) return;

		var toSend = new BatchedPositionData { Positions = Tracked.Select(x => x.transform.position).ToList() };
		SetPositions(JsonUtility.ToJson(toSend), true);
	}


	public void SetPositions(string message, bool broadcast = false)
	{
		var data = JsonUtility.FromJson<BatchedPositionData>(message);
		for (int i = 0; i < Tracked.Count; i++)
		{
			Tracked[i].transform.position = data.Positions[i];
		}

		if (broadcast)
		{
			Servicer.Instance.Netcode.SendData(
				(short)NetcodeMsgType.BatchedPositionUpdate,
				message);
		}
	}

}
