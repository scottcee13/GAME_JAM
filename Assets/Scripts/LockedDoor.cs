using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LockedDoor : MonoBehaviour
{
    [SerializeField]
    private bool canUse = false;

    public UnityEvent doorOpened = new();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (canUse)
           doorOpened.Invoke();
    }

    public void Unlock()
    {
        if (canUse)
            return;

        Destroy(transform.Find("Lock").gameObject);
        canUse = true;
    }
}
