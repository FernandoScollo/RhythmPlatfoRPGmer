using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class MLPlayerStats : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 8f;
    [Tooltip("Initial vertical velocity, not height in world units.")]
    [SerializeField, Min(0f)] private float jumpForce = 16f;
    [SerializeField, Min(0f)] private float jumpGravity = 4.5f;
    [Header("Bombo Jump")]
    [Tooltip("Total jumps including the ground jump. 2 = double jump.")]
    [SerializeField, Min(1)] private int maxJumps = 2;
    [SerializeField, Min(0f)] private float bomboJumpForce = 16f;
    [Header("Guitar Dash")]
    [SerializeField, Min(0f)] private float guitarDashForce = 10f;
    [SerializeField, Min(0f)] private float guitarVerticalDashForce = 2f;
    [SerializeField, Min(0f)] private float guitarDashDuration = 0.7f;
    [SerializeField, Min(0f)] private float guitarDashBrakeStrength = 20f;
    [Header("Bombo Dash")]
    [SerializeField, Min(0f)] private float bomboDashForce = 12f;
    [SerializeField, Min(0f)] private float bomboVerticalDashForce = 4f;
    [SerializeField, Min(0f)] private float bomboDashDuration = 0.7f;
    [SerializeField, Min(0f)] private float bomboDashBrakeStrength = 20f;

    [Header("Dash Cooldowns")]
    [SerializeField, Min(0f)] private float guitarDashCooldown = 2f;
    [SerializeField, Min(0f)] private float bomboDashCooldown = 2f;
    public float GuitarDashCooldown => guitarDashCooldown;
    public float BomboDashCooldown => bomboDashCooldown;
    [Header("Skill Projectile Damage")]
    [SerializeField, Min(0f)] private float guitarProjectileDamage = 1f;
    [SerializeField, Min(0f)] private float bomboDashProjectileDamage = 1f;
    [SerializeField, Min(0f)] private float airJumpProjectileDamage = 1f;
    public float GuitarProjectileDamage => guitarProjectileDamage;
    public float BomboDashProjectileDamage => bomboDashProjectileDamage;
    public float AirJumpProjectileDamage => airJumpProjectileDamage;

    [Header("Events")]
    public UnityEvent OnStatsChanged = new UnityEvent();

    public float MoveSpeed => moveSpeed;
    public float JumpForce => jumpForce;
    public float JumpGravity => jumpGravity;
    public int MaxJumps => maxJumps;
    public float BomboJumpForce => bomboJumpForce;
    public float GuitarDashForce => guitarDashForce;
    public float GuitarVerticalDashForce => guitarVerticalDashForce;
    public float GuitarDashDuration => guitarDashDuration;
    public float GuitarDashBrakeStrength => guitarDashBrakeStrength;
    public float BomboDashForce => bomboDashForce;
    public float BomboVerticalDashForce => bomboVerticalDashForce;
    public float BomboDashDuration => bomboDashDuration;
    public float BomboDashBrakeStrength => bomboDashBrakeStrength;

    // Apply one pickup as a single change, then notify listeners once.
    public void ApplyModifiers(
        float moveSpeed = 0f,
        float jumpForce = 0f,
        float jumpGravity = 0f,
        float guitarDashForce = 0f,
        float guitarVerticalDashForce = 0f,
        float guitarDashDuration = 0f,
        float guitarDashBrakeStrength = 0f,
        float bomboDashForce = 0f,
        float bomboVerticalDashForce = 0f,
        float bomboDashDuration = 0f,
        float bomboDashBrakeStrength = 0f,
        int maxJumps = 0,
        float bomboJumpForce = 0f,
        float guitarProjectileDamage = 0f,
        float bomboDashProjectileDamage = 0f,
        float airJumpProjectileDamage = 0f,
        float guitarDashCooldown = 0f,
        float bomboDashCooldown = 0f)
    {
        this.moveSpeed = Mathf.Max(0f, this.moveSpeed + moveSpeed);
        this.guitarDashCooldown = Mathf.Max(0f, this.guitarDashCooldown + guitarDashCooldown);
        this.bomboDashCooldown = Mathf.Max(0f, this.bomboDashCooldown + bomboDashCooldown);
        this.jumpForce = Mathf.Max(0f, this.jumpForce + jumpForce);
        this.jumpGravity = Mathf.Max(0f, this.jumpGravity + jumpGravity);
        this.maxJumps = Mathf.Max(1, this.maxJumps + maxJumps);
        this.bomboJumpForce = Mathf.Max(0f, this.bomboJumpForce + bomboJumpForce);
        this.guitarDashForce = Mathf.Max(0f, this.guitarDashForce + guitarDashForce);
        this.guitarVerticalDashForce = Mathf.Max(0f, this.guitarVerticalDashForce + guitarVerticalDashForce);
        this.guitarDashDuration = Mathf.Max(0f, this.guitarDashDuration + guitarDashDuration);
        this.guitarDashBrakeStrength = Mathf.Max(0f, this.guitarDashBrakeStrength + guitarDashBrakeStrength);
        this.bomboDashForce = Mathf.Max(0f, this.bomboDashForce + bomboDashForce);
        this.bomboVerticalDashForce = Mathf.Max(0f, this.bomboVerticalDashForce + bomboVerticalDashForce);
        this.bomboDashDuration = Mathf.Max(0f, this.bomboDashDuration + bomboDashDuration);
        this.bomboDashBrakeStrength = Mathf.Max(0f, this.bomboDashBrakeStrength + bomboDashBrakeStrength);
        this.guitarProjectileDamage = Mathf.Max(0f, this.guitarProjectileDamage + guitarProjectileDamage);
        this.bomboDashProjectileDamage = Mathf.Max(0f, this.bomboDashProjectileDamage + bomboDashProjectileDamage);
        this.airJumpProjectileDamage = Mathf.Max(0f, this.airJumpProjectileDamage + airJumpProjectileDamage);
        OnStatsChanged?.Invoke();
    }

    // Inspector changes during play notify UI and other listeners.
    private void OnValidate()
    {
        guitarDashCooldown = Mathf.Max(0f, guitarDashCooldown);
        bomboDashCooldown = Mathf.Max(0f, bomboDashCooldown);
        guitarProjectileDamage = Mathf.Max(0f, guitarProjectileDamage);
        bomboDashProjectileDamage = Mathf.Max(0f, bomboDashProjectileDamage);
        airJumpProjectileDamage = Mathf.Max(0f, airJumpProjectileDamage);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        jumpForce = Mathf.Max(0f, jumpForce);
        jumpGravity = Mathf.Max(0f, jumpGravity);
        maxJumps = Mathf.Max(1, maxJumps);
        bomboJumpForce = Mathf.Max(0f, bomboJumpForce);
        guitarDashForce = Mathf.Max(0f, guitarDashForce);
        guitarVerticalDashForce = Mathf.Max(0f, guitarVerticalDashForce);
        guitarDashDuration = Mathf.Max(0f, guitarDashDuration);
        guitarDashBrakeStrength = Mathf.Max(0f, guitarDashBrakeStrength);
        bomboDashForce = Mathf.Max(0f, bomboDashForce);
        bomboVerticalDashForce = Mathf.Max(0f, bomboVerticalDashForce);
        bomboDashDuration = Mathf.Max(0f, bomboDashDuration);
        bomboDashBrakeStrength = Mathf.Max(0f, bomboDashBrakeStrength);
        if (Application.isPlaying)
            OnStatsChanged?.Invoke();
    }
}
