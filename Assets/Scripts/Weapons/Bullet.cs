using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletSpeed = 20;
    public Rigidbody bulletRB;
    void Awake()
    {
        bulletRB.AddForce(bulletSpeed * transform.forward, ForceMode.VelocityChange);
    }

    void OnCollisionEnter(Collision other)
    {
        Debug.Log(other.gameObject.name);
        Destroy(this);
    }
}
