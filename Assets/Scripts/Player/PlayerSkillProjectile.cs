using System.Collections.Generic;
using UnityEngine;

// Query-based collision deliberately does not depend on the BossBody collision matrix.
[DefaultExecutionOrder(100)]
public class PlayerSkillProjectile : MonoBehaviour
{
    private readonly List<RaycastHit2D> hits = new List<RaycastHit2D>(16);
    private readonly List<Collider2D> overlaps = new List<Collider2D>(16);
    private TrailRenderer trail;
    private Transform owner;
    private Vector2 direction;
    private float damage, speed, remaining, radius;
    private LayerMask obstacles, targets;
    public bool InUse => gameObject.activeSelf;

    public void Initialize(Sprite sprite, Material material, Color color, float size, float trailTime)
    {
        radius = Mathf.Max(0.01f, size);
        var visual = new GameObject("Sprite");
        visual.transform.SetParent(transform, false);
        var renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sharedMaterial = material;
        renderer.sortingLayerName = "Character";
        renderer.sortingOrder = 20;
        if (sprite != null) visual.transform.localScale = Vector3.one * (radius * 2f / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y));
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.sharedMaterial = material;
        trail.time = Mathf.Max(0f, trailTime);
        trail.startWidth = radius * 1.6f;
        trail.endWidth = 0f;
        trail.startColor = color;
        trail.endColor = new Color(color.r, color.g, color.b, 0f);
        trail.sortingLayerName = "Character";
        trail.sortingOrder = 19;
        trail.emitting = false;
    }

    public void Launch(Transform shooter, Vector2 start, Vector2 muzzle, Vector2 heading,
        float hitDamage, float velocity, float life, LayerMask terrain, LayerMask enemies)
    {
        owner = shooter; direction = heading.normalized;
        damage = Mathf.Max(0f, hitDamage); speed = Mathf.Max(0f, velocity);
        remaining = Mathf.Max(0.01f, life); obstacles = terrain; targets = enemies;
        transform.position = start;
        gameObject.SetActive(true);
        trail.Clear();
        // Sweep from the player's centre so an offset cannot put a shot through a wall.
        if (CheckOverlap() || Sweep(start, muzzle - start)) return;
        transform.position = muzzle;
        trail.Clear(); trail.emitting = true;
    }

    private ContactFilter2D Filter()
    {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(obstacles.value | targets.value);
        filter.useTriggers = true;
        return filter;
    }

    private bool Classify(Collider2D other, out MLHealth health)
    {
        health = null;
        if (other == null || (owner != null && other.transform.IsChildOf(owner))) return false;
        int bit = 1 << other.gameObject.layer;
        if ((targets.value & bit) != 0)
        {
            health = other.GetComponentInParent<MLHealth>();
            if (health != null && health.isActiveAndEnabled && !health.IsDead) return true;
            health = null;
        }
        return !other.isTrigger && (obstacles.value & bit) != 0;
    }

    private bool CheckOverlap()
    {
        Physics2D.OverlapCircle(transform.position, radius, Filter(), overlaps);
        MLHealth victim = null;
        foreach (var other in overlaps)
        {
            if (!Classify(other, out MLHealth health)) continue;
            if (health == null) { ReturnToPool(); return true; }
            victim = health;
        }
        if (victim == null) return false;
        Impact(victim); return true;
    }

    private bool Sweep(Vector2 start, Vector2 delta)
    {
        if (delta.sqrMagnitude < 0.000001f) return false;
        Physics2D.CircleCast(start, radius, delta.normalized, Filter(), hits, delta.magnitude);
        float nearest = float.PositiveInfinity;
        bool found = false;
        MLHealth victim = null;
        foreach (var hit in hits)
        {
            if (!Classify(hit.collider, out MLHealth health) || hit.distance > nearest) continue;
            // At equal distances, solid terrain wins over a damage receiver.
            if (found && hit.distance == nearest && health != null) continue;
            nearest = hit.distance; victim = health; found = true;
        }
        if (found) Impact(victim);
        return found;
    }

    private void FixedUpdate()
    {
        if (remaining <= 0f || owner == null) { ReturnToPool(); return; }
        if (CheckOverlap()) return;
        float step = Mathf.Min(remaining, Time.fixedDeltaTime);
        Vector2 delta = direction * speed * step;
        if (Sweep(transform.position, delta)) return;
        transform.position += (Vector3)delta;
        remaining -= step;
        if (remaining <= 0f) ReturnToPool();
    }

    private void Impact(MLHealth health)
    {
        float hitDamage = damage;
        ReturnToPool(); // Release before callbacks can disable the encounter.
        if (health != null) health.TakeDamage(hitDamage);
    }

    public void ReturnToPool()
    {
        trail.emitting = false; trail.Clear();
        owner = null; remaining = 0f;
        gameObject.SetActive(false);
    }
}
