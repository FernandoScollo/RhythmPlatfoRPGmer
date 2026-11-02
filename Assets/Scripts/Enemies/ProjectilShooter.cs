using UnityEngine;
using UnityEngine.Events;

// Rhythm gating is independent of the boss's melee state machine.
[DisallowMultipleComponent]
public class ProjectilShooter : MonoBehaviour
{
    [SerializeField] private RangeFrecuencyObtainer rhythm;
    [SerializeField] private RhythmPlatformController movementRhythm;
    [SerializeField] private Transform muzzle;
    [SerializeField] private Vector2 localSpawnOffset = new Vector2(0f, 3f);
    [SerializeField, Min(0.01f)] private float interval = 0.5f;
    [SerializeField, Min(1)] private int poolSize = 10;
    [Tooltip("Seconds of active movement; paused when platforms A are not active.")]
    [SerializeField, Min(0.01f)] private float lifetime = 3f;
    [SerializeField, Min(0.1f)] private float speed = 7f;
    [SerializeField, Min(0f)] private float turnSpeed = 180f;
    [SerializeField, Min(0f)] private float damage = 1f;
    [SerializeField, Min(0.01f)] private float radius = 0.2f;
    [SerializeField] private LayerMask obstacleLayers = (1 << 6) | (1 << 7) | (1 << 8) | (1 << 9) | (1 << 10);
    [Header("Placeholder visuals")]
    [SerializeField] private Sprite projectileSprite;
    [SerializeField] private Material projectileMaterial;
    [SerializeField] private Color color = new Color(1f, 0.65f, 0.12f, 1f);
    [SerializeField, Min(0f)] private float trailTime = 0.3f;
    public UnityEvent OnShot = new UnityEvent();
    private ProjectilePool pool;
    private Transform target;
    private MLHealth health;
    private bool firingEnabled;
    private float nextShot;

    private void Awake()
    {
        // World-space root: never inherits the boss's scale, movement or facing.
        GameObject root = new GameObject(name + " Projectile Pool");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, gameObject.scene);
        pool = root.AddComponent<ProjectilePool>();
        pool.Initialize(Mathf.Max(1, poolSize), projectileSprite, projectileMaterial, color, radius, trailTime);
    }

    public void Activate(Transform player)
    {
        target = player;
        health = player != null ? player.GetComponent<MLHealth>() : null;
        if (movementRhythm == null) movementRhythm = RhythmPlatformController.Instance;
        if (rhythm == null && RhythmPlatformController.Instance != null)
            rhythm = RhythmPlatformController.Instance.channelC;
        firingEnabled = target != null && health != null && rhythm != null;
        nextShot = Time.time;
        if (!firingEnabled) Debug.LogWarning("ProjectilShooter needs a player with MLHealth and a rhythm source.", this);
    }

    private void Update()
    {
        if (!firingEnabled) return;
        if (target == null || health == null || health.IsDead) { Deactivate(); return; }
        if (rhythm == null || !rhythm.isActiveAndEnabled || !rhythm.SoundON() || Time.time < nextShot) return;
        // Keep the deadline through threshold toggles; no catch-up bursts after a slow frame.
        nextShot = Time.time + Mathf.Max(0.01f, interval);
        HomingProjectile shot = pool.Rent();
        if (shot == null) return;
        Vector2 position = muzzle != null ? (Vector2)muzzle.position :
            (Vector2)transform.position + new Vector2(localSpawnOffset.x * Mathf.Sign(transform.localScale.x), localSpawnOffset.y);
        shot.Launch(position, target, transform, lifetime, speed, turnSpeed, damage, obstacleLayers, movementRhythm);
        OnShot.Invoke();
    }

    public void Deactivate()
    {
        firingEnabled = false;
        if (pool != null) pool.ReturnAll();
    }
    private void OnDisable() { Deactivate(); }
    private void OnDestroy() { if (pool != null) Destroy(pool.gameObject); }
}
