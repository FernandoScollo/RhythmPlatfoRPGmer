using UnityEngine;
using UnityEngine.Events;

// Dash input and horizontal movement run first.
[DefaultExecutionOrder(10)]
[DisallowMultipleComponent]
[RequireComponent(typeof(MLPlayerMovement), typeof(Rigidbody2D))]
public class PlayerBomboJumpSkill : MonoBehaviour
{
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [Header("Fallback values without MLPlayerStats")]
    [SerializeField, Min(1)] private int maxJumps = 2;
    [SerializeField, Min(0f)] private float bomboJumpForce = 13f;
    [Header("Events")]
    public UnityEvent OnGroundJump = new UnityEvent();
    public UnityEvent OnAirJump = new UnityEvent();
    public UnityEvent OnLanded = new UnityEvent();

    private Rigidbody2D body;
    private MLPlayerMovement movement;
    private MLPlayerStats stats;
    private MLHealth health;
    private int jumpsUsed;
    private bool wasGrounded;
    public int MaxJumps => Mathf.Max(1, stats != null ? stats.MaxJumps : maxJumps);
    public int RemainingJumps => Mathf.Max(0, MaxJumps - jumpsUsed);

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        movement = GetComponent<MLPlayerMovement>();
        stats = GetComponent<MLPlayerStats>();
        health = GetComponent<MLHealth>();
    }

    private void OnEnable()
    {
        ResetJumps();
        if (health != null) health.OnRevived.AddListener(ResetJumps);
    }

    private void OnDisable()
    {
        if (health != null) health.OnRevived.RemoveListener(ResetJumps);
    }

    private void Start()
    {
        // Spawning on ground is not a landing animation.
        wasGrounded = movement.IsGrounded && body.velocity.y <= 0.01f;
        jumpsUsed = wasGrounded ? 0 : 1;
    }

    private void ResetJumps()
    {
        jumpsUsed = 1;
        wasGrounded = false;
    }

    private void Update()
    {
        if (!movement.isActiveAndEnabled || movement.IsPlatformFrozen || (health != null && health.IsDead)) return;
        // Rising inside the ground sensor must not replenish jumps.
        bool grounded = movement.IsGrounded && body.velocity.y <= 0.01f;
        if (grounded)
        {
            jumpsUsed = 0;
            if (!wasGrounded) OnLanded?.Invoke();
        }
        else jumpsUsed = Mathf.Max(1, jumpsUsed); // Walking off consumes the ground jump.
        wasGrounded = grounded;

        if (!Input.GetKeyDown(jumpKey) || movement.IsDashActive || movement.IsKnockbackActive || RemainingJumps == 0) return;
        float force = grounded ? movement.JumpForce :
            (stats != null ? stats.BomboJumpForce : bomboJumpForce);
        if (force <= 0f) return;

        body.velocity = new Vector2(body.velocity.x, force);
        jumpsUsed++;
        wasGrounded = false;
        if (grounded)
        {
            OnGroundJump?.Invoke();
            movement.OnJump?.Invoke(); // Preserve existing Inspector listeners.
        }
        else OnAirJump?.Invoke();
    }
}
