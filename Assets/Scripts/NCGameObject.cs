using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class NCGameObject : MonoBehaviour {
// Base entity class to interface with our net code aware gameobjects

	public int ID;

	public bool TrackPosition;

	public float Bob = 0;
	public int owner = 0;

	[Button]
	public void FoceNetPosUpdate()
	{
		NetPosition = transform.position;
	}

	public Vector3 NetPosition
	{
		get { return transform.position; }
		set
		{
			transform.position = value;
			if (TrackPosition)
			{
				Servicer.Instance.TrackedObjects.AddPositionRequest(ID, Servicer.Instance.Netcode.ConnectionID, value);
			}
		}
	}

	void Start()
	{
		var added = Servicer.Instance.TrackedObjects.TrackObject(ID, this);
		if (!added)
		{
			Debug.LogErrorFormat("Tracked Objects already contains ID '{0}'. '{1}' could not be tracked.", ID, name);
		}
	}

	// Update is called once per frame
	void Update()
	{
		// TODO - Temporary
		if (owner == Servicer.Instance.Netcode.ConnectionID)
			NetPosition = transform.position;

		if (Bob != 0 && Servicer.Instance.Netcode.IsServer)
		{
			var end = transform.position;
			end.x = Bob;
			DOTween.To(() => NetPosition, x => NetPosition = x, end, 2).SetLoops(-1, LoopType.Yoyo);
			Bob = 0;
		}
	}
}
