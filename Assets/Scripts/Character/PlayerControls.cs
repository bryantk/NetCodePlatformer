using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerControls : MonoBehaviour
{

	public NCGameObject NcGO;

    [SerializeField]
    private CharacterController playerController;
    [SerializeField]
    private Transform playerTrans;
    public float movementSpeed = 30f;

    public GameObject bulletPrefab;
    private float _nextFireTime = 0;
    private float _currentJumpFactor;
    [SerializeField]
    private float _jumpPower = 15f;
    [SerializeField]
    private float _jumpGravity = 20f;
    // Use this for initialization
    void Start()
    {
        if (playerTrans == null)
            playerTrans = transform;
        if (playerController == null)
            playerController = gameObject.GetComponent<CharacterController>();
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
            Instantiate(bulletPrefab, transform.position + fireDirection + ((Vector3.up * .15f ) * Random.Range(-1f, 1f)), Quaternion.LookRotation(fireDirection, Vector3.up));
            _nextFireTime = Time.time + .3f;
        }
	    //NcGO.NetPosition = transform.position;
    }
}
