using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Spawner : MonoBehaviour
{

	public GameObject Player;
	public GameObject DummyPlayer;

	public void SpawnPlayer(int id, Vector3 pos)
	{
		var go = Instantiate(Player);
		go.GetComponentInChildren<NCGameObject>().ID = id;
		go.transform.position = pos;
	}

	public void SpawnDummy(int id, Vector3 pos, bool broadcast = false)
	{
		var data = new PositionRequest {Id = id, Position = pos};
		SpawnDummy(JsonUtility.ToJson(data));
	}

	public void SpawnDummy(string message, bool broadcast = false)
	{
		var data = JsonUtility.FromJson<PositionRequest>(message);
		if (data.Id != Servicer.Instance.Netcode.ConnectionID)
		{
			var go = Instantiate(DummyPlayer);
			go.transform.position = data.Position;
			Servicer.Instance.TrackedObjects.TrackObject(data.Id, go);

		}


		if (broadcast)
		{
			Servicer.Instance.Netcode.SendData(
				(short)NetcodeMsgType.SpawnDummyPlayer,
				message
				);
		}
	}
}
