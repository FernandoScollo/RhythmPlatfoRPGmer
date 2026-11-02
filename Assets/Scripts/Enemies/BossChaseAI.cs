using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[DefaultExecutionOrder(50)] // Consume B after the controller finishes platform activation.
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class BossChaseAI : MonoBehaviour
{
    public enum ChaseState { Inactive, Chase, ChargePreparation, Charge, StompPreparation, Stomp, Recovery, WaitingRhythmB }
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private RhythmPlatformController rhythmController;
    [SerializeField] private LayerMask groundLayer = 1 << 6;
    [Header("Movement")]
    [SerializeField, Min(0.1f)] private float moveSpeed = 5.5f;
    [SerializeField, Min(0.1f)] private float acceleration = 35f;
    [SerializeField, Min(0.1f)] private float gravityScale = 3f;
    [SerializeField, Min(0.1f)] private float jumpSpeed = 17f;
    [SerializeField, Min(0.1f)] private float jumpCooldown = 1f;
    [Header("Charge")]
    [SerializeField, Min(0f)] private float chargeRange = 6f;
    [SerializeField, Min(0.1f)] private float chargeSpeed = 12f;
    [SerializeField, Min(0.1f)] private float chargeDuration = 2f;
    [Header("Jump attack")]
    [SerializeField, Min(0f)] private float stompRange = 10f;
    [SerializeField, Min(0.1f)] private float stompJumpSpeed = 22f;
    [SerializeField, Min(0.1f)] private float stompHorizontalSpeed = 10f;
    [SerializeField, Min(0.1f)] private float airborneTimeout = 4f;
    [Header("Recovery")]
    [SerializeField, Min(0f)] private float recoveryDuration = 0.8f;
    [SerializeField, Min(0f)] private float attackCooldown = 1.5f;
    [SerializeField] private ChaseState currentState;
    public ChaseState CurrentState => currentState;
    public UnityEvent OnChargePreparing = new UnityEvent();
    public UnityEvent OnChargeStarted = new UnityEvent();
    public UnityEvent OnStompPreparing = new UnityEvent();
    public UnityEvent OnStompStarted = new UnityEvent();
    public UnityEvent OnStompLanded = new UnityEvent();
    public UnityEvent OnRecovery = new UnityEvent();

    private Rigidbody2D body;
    private BoxCollider2D shape;
    private MLHealth targetHealth;
    private readonly RaycastHit2D[] hits = new RaycastHit2D[32];
    private float timer, attackReadyAt, jumpReadyAt, chargeDirection, flightVelocity;
    private float initialScaleX;
    private bool preferStomp, wasAirborne;
    private RhythmPlatformController subscribedRhythm;
    private bool pendingStomp, receivedB;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        shape = GetComponent<BoxCollider2D>();
        initialScaleX = Mathf.Abs(transform.localScale.x);
        body.gravityScale = gravityScale;
    }

    public void BeginChase(Transform player)
    {
        UnsubscribeRhythm();
        subscribedRhythm = rhythmController != null ? rhythmController : RhythmPlatformController.Instance;
        if (subscribedRhythm != null) subscribedRhythm.OnPlatformsBActivated.AddListener(HandleBActivated);
        else Debug.LogWarning("BossChaseAI needs a rhythm controller to start melee attacks.", this);
        receivedB = false;
        target = player;
        targetHealth = player != null ? player.GetComponent<MLHealth>() : null;
        body.velocity = Vector2.zero;
        preferStomp = false;
        attackReadyAt = Time.time + attackCooldown;
        jumpReadyAt = Time.time;
        currentState = target != null ? ChaseState.Chase : ChaseState.Inactive;
    }

    private void OnDisable()
    {
        UnsubscribeRhythm();
        receivedB = false;
        currentState = ChaseState.Inactive;
        if (body != null) body.velocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (target == null || (targetHealth != null && targetHealth.IsDead) || currentState == ChaseState.Inactive)
        {
            receivedB = false;
            currentState = ChaseState.Inactive;
            body.velocity = new Vector2(0f, body.velocity.y);
            return;
        }
        body.gravityScale = gravityScale;
        timer += Time.fixedDeltaTime;
        bool grounded = body.velocity.y <= 0.1f && Surface(Vector2.down, 0.1f, true);
        float dx = target.position.x - shape.bounds.center.x;
        float direction = dx >= 0f ? 1f : -1f;
        float gap = Mathf.Max(0f, Mathf.Abs(dx) - shape.bounds.extents.x);
        switch (currentState)
        {
            case ChaseState.Chase:
                Face(direction);
                bool sameHeight = Mathf.Abs(target.position.y - shape.bounds.min.y) < 2.5f;
                if (grounded && Time.time >= attackReadyAt)
                {
                    if (gap <= stompRange && (preferStomp || !sameHeight) && CanReachTarget())
                    { WaitForB(true); break; }
                    if (gap <= chargeRange && sameHeight)
                    { WaitForB(false); break; }
                }
                bool wall = Surface(Vector2.right * direction, 0.15f, false);
                bool edge = grounded && !FloorAhead(direction);
                if (grounded && (wall || edge || target.position.y > shape.bounds.max.y))
                {
                    if (Time.time >= jumpReadyAt && CanLandAhead(direction))
                    {
                        body.velocity = new Vector2(direction * moveSpeed, jumpSpeed);
                        jumpReadyAt = Time.time + jumpCooldown;
                    }
                    else SetHorizontal(0f);
                }
                else SetHorizontal(wall ? 0f : (gap > 0.1f ? direction * moveSpeed : 0f));
                break;
            case ChaseState.ChargePreparation:
                WaitForB(false); // Compatibility with old serialized enum values.
                break;
            case ChaseState.StompPreparation:
                WaitForB(true);
                break;
            case ChaseState.WaitingRhythmB:
                body.velocity = new Vector2(0f, body.velocity.y);
                Face(direction);
                if (!receivedB) break;
                receivedB = false;
                if (!grounded || !target.gameObject.activeInHierarchy ||
                    (pendingStomp ? gap > stompRange || !CanReachTarget() :
                     gap > chargeRange || Mathf.Abs(target.position.y - shape.bounds.min.y) >= 2.5f ||
                     Surface(Vector2.right * direction, 0.2f, false) || !FloorAhead(direction)))
                { Enter(ChaseState.Chase); break; }
                if (pendingStomp)
                {
                    float time = FlightTime(target.position.y, stompJumpSpeed);
                    flightVelocity = Mathf.Clamp(dx / time, -stompHorizontalSpeed, stompHorizontalSpeed);
                    body.velocity = new Vector2(flightVelocity, stompJumpSpeed);
                    wasAirborne = false; preferStomp = false;
                    Enter(ChaseState.Stomp); OnStompStarted.Invoke();
                }
                else
                {
                    chargeDirection = direction; preferStomp = true;
                    body.velocity = new Vector2(direction * chargeSpeed, body.velocity.y);
                    Enter(ChaseState.Charge); OnChargeStarted.Invoke();
                }
                break;
            case ChaseState.Charge:
                // A wall or cliff interrupts the attack; it never pushes indefinitely.
                if (timer >= chargeDuration || Surface(Vector2.right * chargeDirection, 0.2f, false) ||
                    (grounded && !FloorAhead(chargeDirection))) { Recover(); break; }
                body.velocity = new Vector2(chargeDirection * chargeSpeed, body.velocity.y);
                break;
            case ChaseState.Stomp:
                if (!grounded) wasAirborne = true;
                if (wasAirborne && grounded)
                { OnStompLanded.Invoke(); Recover(); break; }
                if (timer > airborneTimeout) { Recover(); break; }
                if (Surface(Vector2.right * Mathf.Sign(flightVelocity), 0.1f, false)) flightVelocity = 0f;
                body.velocity = new Vector2(flightVelocity, body.velocity.y);
                break;
            case ChaseState.Recovery:
                SetHorizontal(0f);
                if (timer >= recoveryDuration && grounded) Enter(ChaseState.Chase);
                break;
        }
    }

    private void WaitForB(bool stomp)
    {
        pendingStomp = stomp;
        receivedB = false; // Never reuse a beat that happened before this decision.
        body.velocity = new Vector2(0f, body.velocity.y);
        Enter(ChaseState.WaitingRhythmB);
        if (stomp) OnStompPreparing.Invoke(); else OnChargePreparing.Invoke();
    }
    private void HandleBActivated()
    {
        if (isActiveAndEnabled && currentState == ChaseState.WaitingRhythmB)
            receivedB = true;
    }
    private void UnsubscribeRhythm()
    {
        if (subscribedRhythm != null) subscribedRhythm.OnPlatformsBActivated.RemoveListener(HandleBActivated);
        subscribedRhythm = null;
    }
    private void Enter(ChaseState state) { currentState = state; timer = 0f; }
    private void Recover()
    {
        body.velocity = new Vector2(0f, body.velocity.y);
        attackReadyAt = Time.time + recoveryDuration + attackCooldown;
        Enter(ChaseState.Recovery); OnRecovery.Invoke();
    }
    private void Face(float direction)
    {
        Vector3 scale = transform.localScale;
        scale.x = initialScaleX * direction; transform.localScale = scale;
    }
    private void SetHorizontal(float velocity)
    {
        body.velocity = new Vector2(Mathf.MoveTowards(body.velocity.x, velocity,
            acceleration * Time.fixedDeltaTime), body.velocity.y);
    }
    private ContactFilter2D GroundFilter()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer); filter.useTriggers = false; return filter;
    }
    private bool Surface(Vector2 direction, float distance, bool floor)
    {
        Bounds b = shape.bounds;
        int count = Physics2D.BoxCast(b.center, (Vector2)b.size * 0.98f, 0f, direction,
            GroundFilter(), hits, distance + (floor ? b.extents.y : b.extents.x) * 0.02f);
        for (int i = 0; i < count; i++)
            if (hits[i].rigidbody != body && hits[i].fraction > 0f &&
                (floor ? hits[i].normal.y > 0.65f : Vector2.Dot(hits[i].normal, direction) < -0.65f)) return true;
        return false;
    }
    private bool FloorAhead(float direction)
    {
        Bounds b = shape.bounds;
        return GroundBelow(new Vector2(b.center.x + direction * (b.extents.x + 0.5f), b.min.y + 0.3f), 0.8f, out _);
    }
    private bool GroundBelow(Vector2 point, float distance, out RaycastHit2D floor)
    {
        int count = Physics2D.Raycast(point, Vector2.down, GroundFilter(), hits, distance);
        for (int i = 0; i < count; i++)
            if (hits[i].rigidbody != body && hits[i].fraction > 0f && hits[i].normal.y > 0.65f)
            { floor = hits[i]; return true; }
        floor = default; return false;
    }
    private bool CanLandAhead(float direction)
    {
        Bounds b = shape.bounds;
        float gravity = Mathf.Max(0.01f, -Physics2D.gravity.y * gravityScale);
        float height = jumpSpeed * jumpSpeed / (2f * gravity);
        float distance = moveSpeed * (2f * jumpSpeed / gravity) * 0.9f;
        Vector2 probe = new Vector2(b.center.x + direction * distance, b.min.y + height);
        if (!GroundBelow(probe, height + 1f, out RaycastHit2D floor)) return false;
        // Only attempt nearby landings that have room for the boss's body.
        Vector2 center = floor.point + Vector2.up * (b.extents.y + 0.1f);
        return Physics2D.OverlapBox(center, (Vector2)b.size * 0.95f, 0f, groundLayer) == null;
    }
    private float FlightTime(float targetY, float velocity)
    {
        float g = Mathf.Max(0.01f, -Physics2D.gravity.y * gravityScale);
        float delta = targetY - shape.bounds.min.y;
        float discriminant = velocity * velocity - 2f * g * delta;
        return discriminant <= 0f ? -1f : (velocity + Mathf.Sqrt(discriminant)) / g;
    }
    private bool CanReachTarget()
    {
        float time = FlightTime(target.position.y, stompJumpSpeed);
        return time > 0f && Mathf.Abs(target.position.x - shape.bounds.center.x) <= stompHorizontalSpeed * time &&
            GroundBelow((Vector2)target.position + Vector2.up * 0.1f, 30f, out _);
    }
}
