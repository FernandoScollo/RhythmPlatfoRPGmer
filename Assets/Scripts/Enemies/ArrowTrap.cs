using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] arrows;
    private float cooldownTimer;

    private void Update()
    {
        cooldownTimer += Time.deltaTime;
        if (cooldownTimer >= attackCooldown)
            Attack();
    }
    private void Attack()
    {
        cooldownTimer = 0;

        arrows[FindArrow()].transform.position = firePoint.position;
        arrows[FindArrow()].GetComponent<EnemieProjectile>().ActivateProjectile();

    }

    private int FindArrow()
    {
        for (int i=0;i < arrows.Length; i++)
        {
            if (!arrows[i].activeInHierarchy)
                return 1;

        }
        return 0;
    }

}
