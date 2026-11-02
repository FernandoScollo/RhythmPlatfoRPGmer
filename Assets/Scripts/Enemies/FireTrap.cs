using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrap : MonoBehaviour
{
    [SerializeField] private float damage = 1;

    [Header("Firetrap Timers")]
    [SerializeField] private float activationDelay = 2;
    [SerializeField] private float activeTime = 2;

    private Animator anim;
    private SpriteRenderer spriteRend;

    private bool triggered; //Cuando la trampa se activa
    private bool active; //Cuando la trampa esta activada y puede herir al player

    private void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            if(!triggered)
            {
                StartCoroutine(ActiveFiretrap());
            }
            if(active)
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
    }

    private IEnumerator ActiveFiretrap()
    {
        triggered = true;
        spriteRend.color = Color.red;

        // Espera y activa todas las componentes
        yield return new WaitForSeconds(activationDelay);
        spriteRend.color = Color.white;
        active = true;
        anim.SetBool("activated", true);

        //Espera y desactiva las componentes y resetea la trampa
        yield return new WaitForSeconds(activeTime);
        active = false;
        triggered = false;
        anim.SetBool("activated", false);
        //
    }
}
