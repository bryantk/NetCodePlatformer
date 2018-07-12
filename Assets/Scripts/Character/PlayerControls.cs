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
	    NcGO.NetPosition = transform.position;
    }
}
