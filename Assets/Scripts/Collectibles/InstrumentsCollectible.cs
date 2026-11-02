using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InstrumentsCollectible : MonoBehaviour
{
    [Header("Movement Modifiers")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpGravity;
    [Header("Guitar Dash Modifiers")]
    [SerializeField] private float guitarDashForce;
    [SerializeField] private float guitarVerticalDashForce;
    [SerializeField] private float guitarDashDuration;
    [SerializeField] private float guitarDashBrakeStrength;
    [Header("Bombo Dash Modifiers")]
    [SerializeField] private float bomboDashForce;
    [SerializeField] private float bomboVerticalDashForce;
    [SerializeField] private float bomboDashDuration;
    [SerializeField] private float bomboDashBrakeStrength;

    private bool collected;
    [Header("Projectile Damage Modifiers")]
    [SerializeField] private float guitarProjectileDamage;
    [SerializeField] private float bomboDashProjectileDamage;
    [SerializeField] private float airJumpProjectileDamage;
    public bool IsCollected => collected;
    public UnityEvent OnCollected = new UnityEvent();

    private void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !isActiveAndEnabled)
            return;

        MLPlayerStats stats = other.GetComponentInParent<MLPlayerStats>();
        if (stats == null)
            return;

        MLHealth health = stats.GetComponent<MLHealth>();
        if (health != null && health.IsDead)
            return;

        // Mark first: multiple player colliders must not apply this twice.
        collected = true;
        stats.ApplyModifiers(
            moveSpeed,
            jumpForce,
            jumpGravity,
            guitarDashForce,
            guitarVerticalDashForce,
            guitarDashDuration,
            guitarDashBrakeStrength,
            bomboDashForce,
            bomboVerticalDashForce,
            bomboDashDuration,
            bomboDashBrakeStrength,
            guitarProjectileDamage: guitarProjectileDamage,
            bomboDashProjectileDamage: bomboDashProjectileDamage,
            airJumpProjectileDamage: airJumpProjectileDamage
        );
        OnCollected.Invoke();
        gameObject.SetActive(false);
    }
}
