using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class MLHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField, Min(0.01f)] private float maxHealth = 10f;
    [SerializeField] private bool startAtMaxHealth = true;
    [SerializeField, Min(0f)] private float startingHealth = 10f;
    [SerializeField] private float currentHealth;

    [Header("Invulnerability")]
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 1f;

    [Header("Events")]
    public UnityEvent<float, float> OnHealthChanged = new UnityEvent<float, float>();
    public UnityEvent<float> OnDamaged = new UnityEvent<float>();
    public UnityEvent<float> OnHealed = new UnityEvent<float>();
    public UnityEvent<float> OnInvulnerabilityStarted = new UnityEvent<float>();
    public UnityEvent OnInvulnerabilityEnded = new UnityEvent();
    public UnityEvent OnDied = new UnityEvent();
    public UnityEvent OnRevived = new UnityEvent();

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float NormalizedHealth => maxHealth > 0f ? CurrentHealth / maxHealth : 0f;
    public bool IsInvulnerable { get; private set; }
    public bool IsDead { get; private set; }

    private Coroutine invulnerabilityCoroutine;

    private void Awake()
    {
        maxHealth = Mathf.Max(0.01f, maxHealth);
        currentHealth = startAtMaxHealth
            ? maxHealth
            : Mathf.Clamp(startingHealth, 0f, maxHealth);
        IsDead = CurrentHealth <= 0f;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (IsDead)
            OnDied?.Invoke();
    }

    public bool TakeDamage(float damage)
    {
        if (damage <= 0f || IsDead || IsInvulnerable)
            return false;

        float previousHealth = CurrentHealth;
        currentHealth = Mathf.Max(0f, CurrentHealth - damage);
        float appliedDamage = previousHealth - CurrentHealth;

        OnDamaged?.Invoke(appliedDamage);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0f)
        {
            Die();
        }
        else if (invulnerabilityDuration > 0f)
        {
            BeginInvulnerability();
        }

        return true;
    }

    public bool Heal(float amount)
    {
        if (amount <= 0f || IsDead || CurrentHealth >= maxHealth)
            return false;

        float previousHealth = CurrentHealth;
        currentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        float appliedHealing = CurrentHealth - previousHealth;

        OnHealed?.Invoke(appliedHealing);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        return true;
    }

    public void Kill()
    {
        if (IsDead)
            return;

        float remainingHealth = CurrentHealth;
        currentHealth = 0f;

        if (remainingHealth > 0f)
            OnDamaged?.Invoke(remainingHealth);

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        Die();
    }

    public void Revive()
    {
        Revive(maxHealth);
    }

    public void Revive(float health)
    {
        if (!IsDead)
            return;

        StopInvulnerability();
        currentHealth = Mathf.Clamp(health, 0.01f, maxHealth);
        IsDead = false;

        OnRevived?.Invoke();
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private void Die()
    {
        StopInvulnerability();
        IsDead = true;
        OnDied?.Invoke();
    }

    private void BeginInvulnerability()
    {
        StopInvulnerability();
        invulnerabilityCoroutine = StartCoroutine(InvulnerabilityRoutine());
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        IsInvulnerable = true;
        OnInvulnerabilityStarted?.Invoke(invulnerabilityDuration);

        yield return new WaitForSeconds(invulnerabilityDuration);

        IsInvulnerable = false;
        invulnerabilityCoroutine = null;
        OnInvulnerabilityEnded?.Invoke();
    }

    private void StopInvulnerability()
    {
        if (invulnerabilityCoroutine != null)
        {
            StopCoroutine(invulnerabilityCoroutine);
            invulnerabilityCoroutine = null;
        }

        if (!IsInvulnerable)
            return;

        IsInvulnerable = false;
        OnInvulnerabilityEnded?.Invoke();
    }
}
