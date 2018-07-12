using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

	public Transform FollowTarget;
	public float FollowSpeed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		//transform.DOMove(FollowTarget.position, FollowSpeed).SetSpeedBased(true);
		transform.position = Vector3.Lerp(transform.position, FollowTarget.position, FollowSpeed);
	}
}
