using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(MLPlayerStats))]
public class PlayerSkillProjectileEmitter : MonoBehaviour
{
    [SerializeField, Min(1)] private int poolSize = 16;
    [SerializeField, Min(0.01f)] private float lifetime = 3f;
    [SerializeField, Min(0f)] private float speed = 10f;
    [SerializeField, Min(0.01f)] private float radius = 0.15f;
    [SerializeField, Min(0f)] private float muzzleGap = 0.05f;
    [SerializeField] private LayerMask obstacleLayers = 1984;
    [SerializeField] private LayerMask targetLayers = 16384; // BossBody; add enemy hurtbox layers here.
    [SerializeField] private Sprite sprite;
    [SerializeField] private Material material;
    [SerializeField] private Color color = new Color(1f, 0.85f, 0.25f, 1f);
    [SerializeField, Min(0f)] private float trailTime = 0.2f;
    private readonly List<PlayerSkillProjectile> pool = new List<PlayerSkillProjectile>();
    private GameObject poolRoot;
    private Material ownedMaterial;
    private MLPlayerStats stats;
    private MLHealth health;
    private Collider2D playerCollider;
    private PlayerDashSkill guitar;
    private PlayerBomboDashSkill bombo;
    private PlayerBomboJumpSkill jump;

    private void Awake()
    {
        stats = GetComponent<MLPlayerStats>(); health = GetComponent<MLHealth>();
        playerCollider = GetComponent<Collider2D>();
        guitar = GetComponent<PlayerDashSkill>(); bombo = GetComponent<PlayerBomboDashSkill>();
        jump = GetComponent<PlayerBomboJumpSkill>();
        poolRoot = new GameObject(name + " Skill Projectile Pool");
        SceneManager.MoveGameObjectToScene(poolRoot, gameObject.scene);
        if (material == null) material = ownedMaterial = new Material(Shader.Find("Sprites/Default"));
        for (int i = 0; i < Mathf.Max(1, poolSize); i++)
        {
            var go = new GameObject("Player Skill Projectile " + i);
            go.SetActive(false); go.transform.SetParent(poolRoot.transform, false);
            var projectile = go.AddComponent<PlayerSkillProjectile>();
            projectile.Initialize(sprite, material, color, radius, trailTime);
            pool.Add(projectile);
        }
    }

    private void OnEnable()
    {
        if (guitar != null) guitar.OnDashStarted.AddListener(FireGuitar);
        if (bombo != null) bombo.OnDashStarted.AddListener(FireBombo);
        if (jump != null) jump.OnAirJump.AddListener(FireJump);
        if (health != null) health.OnDied.AddListener(ReturnAll);
    }
    private void OnDisable()
    {
        if (guitar != null) guitar.OnDashStarted.RemoveListener(FireGuitar);
        if (bombo != null) bombo.OnDashStarted.RemoveListener(FireBombo);
        if (jump != null) jump.OnAirJump.RemoveListener(FireJump);
        if (health != null) health.OnDied.RemoveListener(ReturnAll);
        ReturnAll();
    }
    private void FireGuitar() => Fire(Vector2.left * guitar.DashDirection, stats.GuitarProjectileDamage);
    private void FireBombo() => Fire(Vector2.left * bombo.DashDirection, stats.BomboDashProjectileDamage);
    private void FireJump() => Fire(Vector2.down, stats.AirJumpProjectileDamage);
    private void Fire(Vector2 direction, float damage)
    {
        if (health != null && health.IsDead) return;
        Bounds bounds = playerCollider != null ? playerCollider.bounds : new Bounds(transform.position, Vector3.zero);
        float extent = Mathf.Abs(direction.x) * bounds.extents.x + Mathf.Abs(direction.y) * bounds.extents.y;
        Vector2 start = bounds.center;
        Vector2 muzzle = start + direction * (extent + radius + muzzleGap);
        foreach (var projectile in pool)
        {
            if (projectile.InUse) continue;
            projectile.Launch(transform, start, muzzle, direction, damage, speed, lifetime, obstacleLayers, targetLayers);
            return;
        }
    }
    private void ReturnAll() { foreach (var projectile in pool) if (projectile != null) projectile.ReturnToPool(); }
    private void OnDestroy()
    {
        if (poolRoot != null) Destroy(poolRoot);
        if (ownedMaterial != null) Destroy(ownedMaterial);
    }
}
