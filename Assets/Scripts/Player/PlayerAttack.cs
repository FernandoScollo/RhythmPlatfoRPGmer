using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCoolDown;
    [SerializeField] private Animator animations;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private float cooldownTimer = Mathf.Infinity;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] fireBalls;

    private void Update()
    {
        if (Input.GetMouseButton(0) && cooldownTimer > attackCoolDown && playerMovement.CanAttack())
            Attack();

        cooldownTimer += Time.deltaTime;
    }

    private void Attack()
    {
        animations.SetTrigger("attack");
        cooldownTimer = 0;
        // Pool Fireballs
        int fireballNumber = FindFireball();
        fireBalls[fireballNumber].transform.position = firePoint.position;
        fireBalls[fireballNumber].GetComponent<Projectile>().SetDirection(Mathf.Sign(transform.localScale.x));
    }
    private int FindFireball()
    {
        for(int i = 0; i < fireBalls.Length; i++)
        {
            if (!fireBalls[i].activeInHierarchy)
                return i;
        }
        return 0;
    }
}
