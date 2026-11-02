using UnityEngine;
using UnityEngine.Events;

public class RhythmPlatforms : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoxCollider2D platformCollider;
    [SerializeField] private Transform player;
    [Header("Safe separation")]
    [SerializeField, Min(0.01f)] private float exitMargin = 0.05f;
    [SerializeField, Min(0.001f)] private float penetrationTolerance = 0.005f;
    public UnityEvent<GameObject> OnPlayerEjected = new UnityEvent<GameObject>();
    private Rigidbody2D playerBody;
    private Collider2D playerCollider;
    private MLPlayerMovement movement;
    private bool activated;

    private void Awake()
    {
        if (platformCollider == null) platformCollider = GetComponent<BoxCollider2D>();
        ResolvePlayer();
    }
    private void ResolvePlayer()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }
        if (player == null) return;
        playerBody = player.GetComponent<Rigidbody2D>();
        playerCollider = player.GetComponent<Collider2D>();
        movement = player.GetComponent<MLPlayerMovement>();
    }

    // True means activation was handled here, never delayed.
    public bool PrepareForActivation()
    {
        if (platformCollider == null) return false;
        platformCollider.enabled = true;
        if (activated) return true;
        activated = true;
        if (playerBody == null || movement == null) ResolvePlayer();
        if (playerBody == null || playerCollider == null || movement == null) return true;
        Physics2D.SyncTransforms();
        ColliderDistance2D overlap = platformCollider.Distance(playerCollider);
        if (!overlap.isValid || !overlap.isOverlapped || overlap.distance >= -penetrationTolerance)
            return true; // Standing on top is not crushing.

        if (movement.IsPlatformFrozen || !TryFindExit(out Vector2 destination))
            movement.FreezeByPlatform(platformCollider);
        else
        {
            movement.CancelExternalMotion();
            playerBody.position = destination; // Sweep and destination both validated.
            Physics2D.SyncTransforms();
        }
        OnPlayerEjected.Invoke(player.gameObject);
        return true;
    }

    private bool TryFindExit(out Vector2 destination)
    {
        Bounds p = playerCollider.bounds;
        Bounds b = platformCollider.bounds;
        float margin = Mathf.Max(0.01f, exitMargin);
        Vector2[] offsets = {
            Vector2.up * (b.max.y - p.min.y + margin),
            Vector2.left * (p.max.x - b.min.x + margin),
            Vector2.right * (b.max.x - p.min.x + margin),
            Vector2.down * (p.max.y - b.min.y + margin)
        };
        for (int i = 1; i < offsets.Length; i++)
            for (int j = i; j > 0 && offsets[j].sqrMagnitude < offsets[j-1].sqrMagnitude; j--)
            { Vector2 swap = offsets[j]; offsets[j] = offsets[j-1]; offsets[j-1] = swap; }
        int mask = Physics2D.GetLayerCollisionMask(playerCollider.gameObject.layer);
        float skin = Mathf.Min(0.005f, Mathf.Min(p.extents.x, p.extents.y) * 0.1f);
        Vector2 size = (Vector2)p.size - Vector2.one * skin * 2f;
        foreach (Vector2 offset in offsets)
        {
            bool blocked = false;
            foreach (RaycastHit2D hit in Physics2D.BoxCastAll(p.center, size, 0f,
                offset.normalized, offset.magnitude, mask))
                if (Blocks(hit.collider)) { blocked = true; break; }
            if (blocked) continue;
            foreach (Collider2D other in Physics2D.OverlapBoxAll((Vector2)p.center + offset, size, 0f, mask))
                if (Blocks(other)) { blocked = true; break; }
            if (!blocked) { destination = playerBody.position + offset; return true; }
        }
        destination = playerBody.position;
        return false;
    }
    private bool Blocks(Collider2D other)
    {
        return other != null && other != platformCollider && !other.isTrigger &&
            other.attachedRigidbody != playerBody && !Physics2D.GetIgnoreCollision(playerCollider, other);
    }
    public void ResetActivation()
    {
        activated = false;
        if (platformCollider != null) platformCollider.enabled = false;
        if (movement != null) movement.ReleasePlatform(platformCollider);
    }
    private void OnDisable() { ResetActivation(); }
}
