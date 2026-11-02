using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] protected float damage;

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        MLHealth mlHealth = collision.GetComponentInParent<MLHealth>();

        if (mlHealth != null)
        {
            mlHealth.TakeDamage(damage);
        }
        else
        {
            collision.GetComponentInParent<Health>()?.TakeDamage(damage);
        }
    }
}
