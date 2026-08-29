using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float startingHealth;
    [SerializeField] public float currentHealth { get; private set;}
    private Animator anim;

    private bool dead;

    [Header("iFrames")]
    [SerializeField] private float iFramesDuration;
    [SerializeField] private int numberOfFlashes;
    private SpriteRenderer spriteRend;

    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(float _damage)
    {
        currentHealth = Mathf.Clamp(currentHealth-_damage, 0f, startingHealth);
        if(currentHealth >0)
        {
            //Player hurt
            anim.SetTrigger("Hurt");
            StartCoroutine(Invulnerability());
        }
        else
        {
            //Player dead
            if (!dead)
            {
                anim.SetTrigger("Die");
                GetComponent<PlayerMovement>().enabled = false;
                dead = true;
            }
            
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            TakeDamage(1f);
    }

    public void AddHealth(float _value)
    {
        currentHealth = Mathf.Clamp(currentHealth + _value, 0f, startingHealth);
    }

    IEnumerator Invulnerability()
    {
        Physics2D.IgnoreLayerCollision(11, 12, true);
        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRend.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(iFramesDuration / numberOfFlashes);
            spriteRend.color = Color.white;
            yield return new WaitForSeconds(iFramesDuration / numberOfFlashes);
        }
        Physics2D.IgnoreLayerCollision(11, 12, false);
    }
}
