using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

	public Transform FollowTarget;
	public Camera CameraRec;
	public float FollowSpeed;
	public float dif;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		//transform.DOMove(FollowTarget.position, FollowSpeed).SetSpeedBased(true);
		transform.position = Vector3.Lerp(transform.position, FollowTarget.position, FollowSpeed);

		dif = (transform.position - FollowTarget.position).magnitude * 6;
		CameraRec.fieldOfView = 45 + dif;
	}
}
