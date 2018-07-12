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
	    NcGO.NetPosition = transform.position;

    }
}
