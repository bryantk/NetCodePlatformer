using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NCGameObject : MonoBehaviour {
// Base entity class to interface with our net code aware gameobjects

	public int ID;

	public bool TrackPosition;

	public Vector3 NetPosition
	{
		get { return transform.position; }
		set
		{
			if ((_lastPosition - transform.position).magnitude > 0.001f)
			{
				transform.position = value;
				_lastPosition = transform.position;
				// Broadcast
				Servicer.Instance.TrackedObjects.AddPositionRequest(ID, 0, value);
			}
		}
	}
	private Vector3 _lastPosition;


	void Start()
	{
		_lastPosition = transform.position + Vector3.one;
	}

	// Update is called once per frame
	void Update()
	{
		NetPosition = transform.position;
	}
}
