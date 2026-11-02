using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCollectible : MonoBehaviour
{
    [Header ("Health")]
    [SerializeField] private float healthValue;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            MLHealth mlHealth = collision.GetComponentInParent<MLHealth>();

            if (mlHealth != null && mlHealth.Heal(healthValue))
            {
                gameObject.SetActive(false);
                return;
            }

            Health health = collision.GetComponentInParent<Health>();

            if (health != null)
            {
                health.AddHealth(healthValue);
                gameObject.SetActive(false);
            }
        }
    }
}
