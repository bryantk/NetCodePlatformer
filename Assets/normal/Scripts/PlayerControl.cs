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
	public float Cooldown = 1;
	private float _nextFireTime = 0;
	private float _currentJumpFactor;
	[SerializeField]
	private float _jumpPower = 15f;
	[SerializeField]
	private float _jumpGravity = 20f;

	// Use this for initialization
	void Start()
	{
		if (!isLocalPlayer)
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
		if (playerController.isGrounded && Input.GetKeyDown(KeyCode.Space))
		{
			_currentJumpFactor = _jumpPower;
		}
		else if (playerController.isGrounded)
		{
			_currentJumpFactor = Physics.gravity.y * Time.deltaTime;
		}
		// Check for Inputs and change movement/fire vectors based on them
		Vector3 movementDirection = (new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"))).normalized * movementSpeed;
		//if (_currentJumpFactor > 0)
		movementDirection.y = _currentJumpFactor;
		playerController.Move(movementDirection * Time.deltaTime);
		_currentJumpFactor -= Time.deltaTime * _jumpGravity;
		Vector3 fireDirection = (new Vector3(Input.GetAxis("HorizontalFire"), 0, Input.GetAxis("VerticalFire"))).normalized;
		if (fireDirection.sqrMagnitude > .01f && Time.time > _nextFireTime)
		{
			// FIRE!!!
			//Network.Instantiate(bulletPrefab, transform.position + fireDirection + ((Vector3.up * .15f) * Random.Range(-1f, 1f)), Quaternion.LookRotation(fireDirection, Vector3.up), playerControllerId);
			CmdFire(fireDirection);
			_nextFireTime = Time.time + Cooldown;
		}

		//transform.DOMove(FollowTarget.position, FollowSpeed).SetSpeedBased(true);
		CamerTransform.position = Vector3.Lerp(CamerTransform.position, transform.position, 0.1f);

	}

	[Command]
	void CmdFire(Vector3 fireDirection)
	{
		var go = Instantiate(bulletPrefab, transform.position + fireDirection + ((Vector3.up * .15f) * Random.Range(-1f, 1f)), Quaternion.LookRotation(fireDirection, Vector3.up));
		NetworkServer.Spawn(go);
	}
}
