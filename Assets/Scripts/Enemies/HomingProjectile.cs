using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)] // After the platform controller has completed A/B transitions.
public class HomingProjectile : MonoBehaviour
{
    public bool InUse { get; private set; }
    private ProjectilePool pool;
    private Rigidbody2D body;
    private TrailRenderer trail;
    private Transform target, owner;
    private MLHealth targetHealth;
    private Collider2D targetCollider;
    private Vector2 direction;
    private float remaining, speed, turnSpeed, damage, radius;
    private LayerMask obstacles;
    private RhythmPlatformController movementRhythm;
    private bool CanMove => movementRhythm != null && movementRhythm.ProjectileMovementAllowed;
    private readonly List<Collider2D> overlaps = new List<Collider2D>(16);
    private readonly List<RaycastHit2D> hits = new List<RaycastHit2D>(16);

    public void Initialize(ProjectilePool source, Sprite sprite, Material material, Color color, float size, float trailTime)
    {
        pool = source; radius = size;
        body = gameObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
        collider.radius = radius; collider.isTrigger = true;
        // The explicit casts below handle hits; trigger bodies never physically shove the player.
        GameObject visual = new GameObject("Sprite");
        visual.transform.SetParent(transform, false);
        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite; renderer.color = color;
        renderer.sortingLayerName = "Character"; renderer.sortingOrder = 20;
        if (material != null) renderer.sharedMaterial = material;
        if (sprite != null) visual.transform.localScale = Vector3.one * (radius * 2f / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y));
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.sharedMaterial = material; trail.time = Mathf.Max(0f, trailTime);
        trail.startWidth = radius * 1.6f; trail.endWidth = 0f;
        trail.startColor = color; trail.endColor = new Color(color.r, color.g, color.b, 0f);
        trail.minVertexDistance = 0.04f; trail.numCapVertices = 4;
        trail.sortingLayerName = "Character"; trail.sortingOrder = 19;
        trail.autodestruct = false; trail.emitting = false;
    }
    public void Reserve() { InUse = true; }
    public void Launch(Vector2 position, Transform player, Transform shooter, float life, float velocity, float turning, float hitDamage, LayerMask layers, RhythmPlatformController rhythmController = null)
    {
        movementRhythm = rhythmController != null ? rhythmController : RhythmPlatformController.Instance;
        target = player; owner = shooter;
        targetHealth = target.GetComponent<MLHealth>();
        targetCollider = target.GetComponent<Collider2D>();
        remaining = Mathf.Max(0.01f, life); speed = Mathf.Max(0.1f, velocity);
        turnSpeed = Mathf.Max(0f, turning); damage = Mathf.Max(0f, hitDamage); obstacles = layers;
        transform.position = position; body.position = position; body.velocity = Vector2.zero;
        direction = (AimPoint() - position).normalized;
        if (direction.sqrMagnitude < 0.01f) direction = Vector2.up;
        gameObject.SetActive(true);
        trail.Clear(); trail.emitting = CanMove;
        // A terrain collider may already cover the muzzle.
        CheckOverlap();
    }
    private Vector2 AimPoint() => targetCollider != null ? (Vector2)targetCollider.bounds.center : (Vector2)target.position;
    private ContactFilter2D Filter()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(Physics2D.AllLayers); filter.useTriggers = true; return filter;
    }
    private bool ValidObstacle(Collider2D other)
    {
        if (other == null || other.transform.IsChildOf(transform) ||
            (owner != null && other.transform.IsChildOf(owner)) || other.GetComponentInParent<HomingProjectile>() != null) return false;
        return !other.isTrigger && (obstacles.value & (1 << other.gameObject.layer)) != 0;
    }
    private bool IsPlayer(Collider2D other) => other != null && other.GetComponentInParent<MLHealth>() == targetHealth && targetHealth != null;
    private bool CheckOverlap()
    {
        Physics2D.OverlapCircle(body.position, radius, Filter(), overlaps);
        // Terrain takes priority if a rhythmic platform appeared over the projectile.
        foreach (Collider2D other in overlaps) if (ValidObstacle(other)) { Return(); return true; }
        foreach (Collider2D other in overlaps) if (IsPlayer(other)) { ImpactPlayer(); return true; }
        return false;
    }
    private void FixedUpdate()
    {
        if (!InUse) return;
        if (target == null || !target.gameObject.activeInHierarchy || targetHealth == null || targetHealth.IsDead)
        { Return(); return; }
        if (remaining <= 0f) { Return(); return; }
        if (CheckOverlap()) return;
        // Pausing never disables collisions or target/death cleanup.
        // MovePosition can leave a kinematic velocity from the previous step: clear it.
        body.velocity = Vector2.zero;
        trail.emitting = CanMove;
        if (!CanMove) return;
        float step = Mathf.Min(Time.fixedDeltaTime, remaining);
        Vector2 desired = AimPoint() - body.position;
        if (desired.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.MoveTowardsAngle(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg,
                Mathf.Atan2(desired.y, desired.x) * Mathf.Rad2Deg, turnSpeed * step) * Mathf.Deg2Rad;
            direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
        float distance = speed * step;
        Physics2D.CircleCast(body.position, radius, direction, Filter(), hits, distance);
        Collider2D first = null; float nearest = float.PositiveInfinity;
        foreach (RaycastHit2D hit in hits)
            if ((ValidObstacle(hit.collider) || IsPlayer(hit.collider)) && hit.distance < nearest)
            { first = hit.collider; nearest = hit.distance; }
        if (first != null)
        {
            if (IsPlayer(first)) ImpactPlayer(); else Return();
            return;
        }
        body.MovePosition(body.position + direction * distance);
        remaining = Mathf.Max(0f, remaining - step);
    }
    private void ImpactPlayer() { targetHealth.TakeDamage(damage); Return(); }
    private void Return() { if (InUse) pool.Return(this); }
    public void ResetForPool()
    {
        InUse = false;
        if (trail != null) { trail.emitting = false; trail.Clear(); }
        if (body != null) body.velocity = Vector2.zero;
        target = null; targetHealth = null; targetCollider = null; owner = null;
        movementRhythm = null;
        remaining = 0f;
        gameObject.SetActive(false);
    }
}
