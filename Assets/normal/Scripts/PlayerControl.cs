using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Networking;

public class PlayerControl : NetworkBehaviour
{

	public CharacterController playerController;
	public Transform playerTrans;
	public float movementSpeed = 30f;

	public GameObject Camera;

	private Transform CamerTransform;

	public GameObject bulletPrefab;
	private float _nextFireTime = 0;
	// Use this for initialization
	void Start()
	{
		if (!localPlayerAuthority)
		{
			Destroy(this);
			return;
		}

		var go = Instantiate(Camera);
		CamerTransform = go.transform;
		CamerTransform.position = transform.position;

	}

	// Update is called once per frame
	void Update()
	{
		// Check for Inputs and change movement/fire vectors based on them
		Vector3 movementDirection = (new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"))).normalized;
		playerController.SimpleMove(movementDirection * movementSpeed);// * Time.deltaTime);
		Vector3 fireDirection = (new Vector3(Input.GetAxis("HorizontalFire"), 0, Input.GetAxis("VerticalFire"))).normalized;
		if (fireDirection.sqrMagnitude > .01f && Time.time > _nextFireTime)
		{
			// FIRE!!!
			Instantiate(bulletPrefab, transform.position + fireDirection, Quaternion.LookRotation(fireDirection, Vector3.up));
			_nextFireTime = Time.time + .3f;
		}



		//transform.DOMove(FollowTarget.position, FollowSpeed).SetSpeedBased(true);
		CamerTransform.position = Vector3.Lerp(CamerTransform.position, transform.position, 0.1f);

	}
}
