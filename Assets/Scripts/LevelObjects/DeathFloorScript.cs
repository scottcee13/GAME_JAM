using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathFloorScript : MonoBehaviour
{
    [SerializeField] private string _deathMessage;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.death.Invoke(_deathMessage);
        }
        
    }
}
