using UnityEngine;

[DisallowMultipleComponent]
public class BossContactDamage : MonoBehaviour
{
    [SerializeField, Min(0f)] private float damage = 2f;
    [SerializeField, Min(0f)] private float horizontalKnockback = 12f;
    [SerializeField, Min(0f)] private float verticalKnockback = 4f;
    [SerializeField, Min(0.02f)] private float knockbackDuration = 0.35f;
    [Tooltip("Transform whose localScale.x determines the push direction.")]
    [SerializeField] private Transform facingTransform;

    private void Awake()
    {
        if (facingTransform == null) facingTransform = transform;
    }

    private void OnTriggerEnter2D(Collider2D other) { Hit(other); }
    private void OnTriggerStay2D(Collider2D other) { Hit(other); }

    private void Hit(Collider2D other)
    {
        if (!isActiveAndEnabled) return;
        MLPlayerMovement player = other.GetComponentInParent<MLPlayerMovement>();
        if (player == null) return;
        MLHealth health = player.GetComponent<MLHealth>();
        if (health == null || !health.TakeDamage(damage) || health.IsDead) return;
        MLPlayerKnockback knockback = player.GetComponent<MLPlayerKnockback>();
        if (knockback != null)
            knockback.ApplyKnockback(new Vector2(
                Mathf.Sign(facingTransform.localScale.x) * horizontalKnockback,
                verticalKnockback), knockbackDuration);
    }
}
