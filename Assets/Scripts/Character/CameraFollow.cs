using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform cameraTarget;
    //public float followSpeed = 0;
    // Use this for initialization
    void Start()
    {
        transform.position = cameraTarget.position;
        transform.parent = cameraTarget;
    }
}
