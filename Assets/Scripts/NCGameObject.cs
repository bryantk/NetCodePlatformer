using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class NCGameObject : MonoBehaviour {
// Base entity class to interface with our net code aware gameobjects

	public int ID;

	public Vector3 NetPosition
	{
		get { return transform.position; }
		set
		{
			transform.position = value;
			Servicer.Instance.TrackedObjects.AddPositionRequest(ID, Servicer.Instance.Netcode.ConnectionID, value);
		}
	}

	void Start()
	{
		var added = Servicer.Instance.TrackedObjects.TrackObject(ID, gameObject);
		if (!added)
		{
			Debug.LogErrorFormat("Tracked Objects already contains ID '{0}'. '{1}' could not be tracked.", ID, name);
		}
	}

}
