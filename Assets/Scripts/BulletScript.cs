using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField]
    private float bulletSpeed = 10.0f;

    private Rigidbody2D rb;  

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();       
    }

    public void Launch(Vector2 direction)
    {
        rb.AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject); 
    }
}
