using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MLPlayerMovement : MonoBehaviour
{
    [Header("Movements")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float jumpGravity;

    [Header("Movement Feel")]
    [Tooltip("Horizontal acceleration in units per second squared.")]
    [SerializeField, Min(0.01f)] private float acceleration = 80f;
    [SerializeField, Min(0.01f)] private float braking = 100f;
    [SerializeField, Min(0.01f)] private float turnAcceleration = 120f;
    [Tooltip("Applied only when falling, outside dash and knockback.")]
    [SerializeField, Min(1f)] private float fallGravityMultiplier = 1.5f;
    [Tooltip("Independent gravity during dashes, preserving their original trajectories.")]
    [SerializeField, Min(0f)] private float dashGravityScale = 3f;

    [Header("Collision Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField, Min(0.001f)] private float collisionSkin = 0.02f;
    [SerializeField, Min(0.001f)] private float groundCheckDistance = 0.06f;
    [SerializeField, Range(0f, 1f)] private float minimumGroundNormal = 0.65f;
    private readonly RaycastHit2D[] collisionHits = new RaycastHit2D[32];
    private float measuredHorizontalSpeed;
    private readonly Dictionary<Collider2D, List<Collider2D>> trappingPlatforms = new Dictionary<Collider2D, List<Collider2D>>();
    private readonly List<Collider2D> releasedPlatforms = new List<Collider2D>();
    private RigidbodyConstraints2D constraintsBeforeFreeze;
    private float gravityBeforeFreeze;
    public bool IsPlatformFrozen => trappingPlatforms.Count > 0;

    [Header("Events")]
    public UnityEvent OnJump;
    public UnityEvent OnLand;

    private Rigidbody2D body;
    private BoxCollider2D boxCollider;
    private MLHealth health;
    private MLPlayerKnockback knockback;
    public bool IsKnockbackActive => knockback != null && knockback.IsActive;
    private MLPlayerStats playerStats;
    private float MoveSpeed => playerStats != null ? playerStats.MoveSpeed : speed;
    public float JumpForce => playerStats != null ? playerStats.JumpForce : jumpPower;
    private float JumpGravity => playerStats != null ? playerStats.JumpGravity : jumpGravity;

    public float innitialLocalScaleX;

    private float horizontalInput;

    private bool wasGrounded;
    private bool canMove = true;

    // Estados públicos para otros sistemas
    public bool IsGrounded { get; private set; }

    public bool IsMoving =>
        canMove && !IsPlatformFrozen && Mathf.Abs(measuredHorizontalSpeed) > 0.05f &&
        !IsDashActive && !IsKnockbackActive;

    public bool HasMoveInput => Mathf.Abs(horizontalInput) > 0.01f;

    public float HorizontalInput =>
        horizontalInput;

    // El Dash utiliza esto para tomar temporalmente
    // el control de la velocidad horizontal.
    public bool IsDashActive { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        health = GetComponent<MLHealth>();
        knockback = GetComponent<MLPlayerKnockback>();
        playerStats = GetComponent<MLPlayerStats>();
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        innitialLocalScaleX = transform.localScale.x;

        IsGrounded = isGrounded() && body.velocity.y <= 0.01f;
        wasGrounded = IsGrounded;
    }

    private void OnEnable()
    {
        if (health == null)
            health = GetComponent<MLHealth>();

        if (health == null)
            return;

        health.OnDied.AddListener(HandleDeath);
        health.OnRevived.AddListener(HandleRevived);
    }

    private void OnDisable()
    {
        foreach (Collider2D platform in new List<Collider2D>(trappingPlatforms.Keys))
            ReleasePlatform(platform);
        if (health == null)
            return;

        health.OnDied.RemoveListener(HandleDeath);
        health.OnRevived.RemoveListener(HandleRevived);
    }

    private void Update()
    {
        if (!canMove || IsPlatformFrozen)
        {
            horizontalInput = 0f;
            return;
        }

        horizontalInput = IsKnockbackActive ? 0f : Input.GetAxisRaw("Horizontal");

        // Actualizar estado de grounded
        IsGrounded = isGrounded() && body.velocity.y <= 0.01f;

        // Detectar aterrizaje
        if (!wasGrounded && IsGrounded)
        {
            OnLand?.Invoke();
        }

        wasGrounded = IsGrounded;

        // Flip player cuando se mueve
        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(
                innitialLocalScaleX,
                transform.localScale.y,
                transform.localScale.z
            );
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(
                -innitialLocalScaleX,
                transform.localScale.y,
                transform.localScale.z
            );
        }

    }

    private void FixedUpdate()
    {
        releasedPlatforms.Clear();
        foreach (Collider2D platform in trappingPlatforms.Keys)
            if (platform == null || !platform.enabled || !platform.gameObject.activeInHierarchy)
                releasedPlatforms.Add(platform);
        foreach (Collider2D platform in releasedPlatforms) ReleasePlatform(platform);
        if (IsPlatformFrozen)
        {
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            measuredHorizontalSpeed = 0f;
            body.gravityScale = 0f;
            return;
        }
        // Sample the velocity resolved by physics, before requesting movement again.
        measuredHorizontalSpeed = body.velocity.x;
        UpdateGravity();
        if (!canMove)
        {
            body.velocity = new Vector2(0f, body.velocity.y);
            return;
        }

        // Dash and knockback retain ownership of velocity.
        if (!IsDashActive && !IsKnockbackActive)
        {
            float targetVelocity = horizontalInput * MoveSpeed;
            bool reversing = HasMoveInput && body.velocity.x * horizontalInput < 0f;
            float rate = !HasMoveInput ? braking : reversing ? turnAcceleration : acceleration;
            float horizontalVelocity = Mathf.MoveTowards(body.velocity.x, targetVelocity,
                Mathf.Max(0.01f, rate) * Time.fixedDeltaTime);

            if (IsOnWall())
            {
                horizontalVelocity = 0f;
            }
            body.velocity = new Vector2(
                horizontalVelocity,
                body.velocity.y
            );
        }

    }

    private bool isGrounded()
    {
        return GetGroundCollider() != null;
    }

    public Collider2D GetGroundCollider()
    {
        return FindSurface(Vector2.down, groundLayer, groundCheckDistance, true);
    }

    public void SetDashActive(bool active)
    {
        IsDashActive = active;
        // Dash starts in Update: don't leave the previous falling gravity active.
        if (body != null) UpdateGravity();
    }

    private void UpdateGravity()
    {
        if (IsPlatformFrozen) { body.gravityScale = 0f; return; }
        if (IsDashActive)
        {
            body.gravityScale = Mathf.Max(0f, dashGravityScale);
            return;
        }
        bool falling = canMove && !IsKnockbackActive && body.velocity.y < -0.01f;
        body.gravityScale = JumpGravity * (falling ? Mathf.Max(1f, fallGravityMultiplier) : 1f);
    }

    private void HandleDeath()
    {
        canMove = false;
        horizontalInput = 0f;
        IsDashActive = false;
        body.velocity = new Vector2(0f, body.velocity.y);
    }

    private void HandleRevived()
    {
        canMove = true;
    }

    public bool CanAttack()
    {
        return !IsPlatformFrozen && !HasMoveInput && IsGrounded;
    }

    public void CancelExternalMotion()
    {
        GetComponent<PlayerDashSkill>()?.CancelDash();
        GetComponent<PlayerBomboDashSkill>()?.CancelDash();
        knockback?.Cancel();
        IsDashActive = false;
        horizontalInput = 0f;
        measuredHorizontalSpeed = 0f;
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    public void FreezeByPlatform(Collider2D platform)
    {
        if (platform == null || trappingPlatforms.ContainsKey(platform)) return;
        if (!IsPlatformFrozen)
        {
            constraintsBeforeFreeze = body.constraints;
            gravityBeforeFreeze = body.gravityScale;
            CancelExternalMotion();
        }
        var changedPairs = new List<Collider2D>();
        foreach (Collider2D own in GetComponentsInChildren<Collider2D>())
        {
            if (own.attachedRigidbody != body || Physics2D.GetIgnoreCollision(own, platform)) continue;
            Physics2D.IgnoreCollision(own, platform, true);
            changedPairs.Add(own);
        }
        trappingPlatforms.Add(platform, changedPairs);
        body.constraints = RigidbodyConstraints2D.FreezeAll;
        body.gravityScale = 0f;
        body.velocity = Vector2.zero;
        IsGrounded = false;
        wasGrounded = false;
    }

    public void ReleasePlatform(Collider2D platform)
    {
        if (ReferenceEquals(platform, null) || !trappingPlatforms.TryGetValue(platform, out List<Collider2D> pairs)) return;
        foreach (Collider2D own in pairs)
            if (own != null && platform != null) Physics2D.IgnoreCollision(own, platform, false);
        trappingPlatforms.Remove(platform);
        if (IsPlatformFrozen) return;
        body.constraints = constraintsBeforeFreeze;
        body.gravityScale = gravityBeforeFreeze;
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    private bool IsOnWall()
    {
        if (Mathf.Abs(horizontalInput) < 0.01f)
            return false;

        return FindSurface(Vector2.right * Mathf.Sign(horizontalInput),
            groundLayer.value | wallLayer.value, 0.02f, false) != null;
    }

    private Collider2D FindSurface(Vector2 direction, int layers, float distance, bool ground)
    {
        Bounds bounds = boxCollider.bounds;
        float skin = Mathf.Min(collisionSkin, Mathf.Min(bounds.extents.x, bounds.extents.y) * 0.25f);
        Vector2 size = (Vector2)bounds.size - Vector2.one * (2f * skin);
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(layers);
        filter.useTriggers = false;
        int count = Physics2D.BoxCast(bounds.center, size, 0f, direction,
            filter, collisionHits, distance + skin);
        for (int i = 0; i < count; i++)
        {
            RaycastHit2D hit = collisionHits[i];
            if (hit.collider == null || hit.rigidbody == body || hit.fraction <= 0f)
                continue; // Initial overlaps have synthetic normals, not supporting surfaces.
            if (ground ? hit.normal.y >= minimumGroundNormal :
                Mathf.Abs(hit.normal.y) < minimumGroundNormal && Vector2.Dot(hit.normal, direction) < -0.5f)
                return hit.collider;
        }
        return null;
    }
}
