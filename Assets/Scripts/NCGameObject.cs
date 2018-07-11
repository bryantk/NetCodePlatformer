using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class NCGameObject : MonoBehaviour {
// Base entity class to interface with our net code aware gameobjects

	public int ID;

	public bool TrackPosition;

	public float Bob = 0;

	public Vector3 NetPosition
	{
		get { return transform.position; }
		set
		{
			if (TrackPosition && (_lastPosition - transform.position).magnitude > 0.001f)
			{
				transform.position = value;
				_lastPosition = transform.position;
				// Broadcast
				Servicer.Instance.TrackedObjects.AddPositionRequest(ID, Servicer.Instance.Netcode.ConnectionID, value);
			}
		}
	}
	private Vector3 _lastPosition;


	void Awake()
	{
		ID = GetInstanceID();
		Servicer.Instance.TrackedObjects.TrackObject(ID, this);
		_lastPosition = transform.position + Vector3.one;

		
	}

	// Update is called once per frame
	void Update()
	{
		// TODO - Temporary
		NetPosition = transform.position;
		if (Bob != 0 && Servicer.Instance.Netcode.IsServer)
		{

			transform.DOMoveX(Bob, 2).SetLoops(-1, LoopType.Yoyo);
			Bob = 0;
		}
	}
}
