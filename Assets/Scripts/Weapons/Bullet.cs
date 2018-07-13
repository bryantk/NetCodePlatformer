using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bullet : MonoBehaviour
{
    public float bulletSpeed = 20;
    public Rigidbody bulletRB;
    public CapsuleCollider bulletCollider;
    void Awake()
    {
        bulletRB.AddForce(bulletSpeed * transform.forward, ForceMode.VelocityChange);
    }

    void OnCollisionEnter(Collision other)
    {
        //Debug.Log(other.gameObject.name);
        //Destroy(gameObject);

        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            StickIntoWall(other);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            StickIntoWall(other);
        }
    }

    void StickIntoWall(Collision other)
    {
        Vector3 pointOfImpact = other.contacts[0].point;
        bulletRB.isKinematic = true;
        bulletRB.velocity = Vector3.zero;
        Physics.IgnoreCollision(bulletCollider, other.collider);
        transform.position = pointOfImpact;
        transform.DOPunchRotation(new Vector3(20, 20, 0), 1f, 10);
        Destroy(gameObject, 5);
    }
}
