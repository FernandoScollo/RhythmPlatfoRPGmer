using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class MLPlayerKnockback : MonoBehaviour
{
    public UnityEvent OnKnockbackStarted = new UnityEvent();
    public UnityEvent OnKnockbackFinished = new UnityEvent();
    private Rigidbody2D body;
    private MLHealth health;
    private PlayerDashSkill guitarDash;
    private PlayerBomboDashSkill bomboDash;
    private float remainingTime;
    public bool IsActive => remainingTime > 0f;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<MLHealth>();
        guitarDash = GetComponent<PlayerDashSkill>();
        bomboDash = GetComponent<PlayerBomboDashSkill>();
    }

    private void OnEnable()
    {
        if (health != null) health.OnDied.AddListener(Cancel);
    }

    private void OnDisable()
    {
        if (health != null) health.OnDied.RemoveListener(Cancel);
        Cancel();
    }

    public void ApplyKnockback(Vector2 velocity, float duration)
    {
        MLPlayerMovement movement = GetComponent<MLPlayerMovement>();
        if (movement != null && movement.IsPlatformFrozen) return;
        if (!isActiveAndEnabled || (health != null && health.IsDead)) return;
        remainingTime = Mathf.Max(Time.fixedDeltaTime, duration);
        guitarDash?.CancelDash();
        bomboDash?.CancelDash();
        body.velocity = velocity;
        OnKnockbackStarted.Invoke();
    }

    private void FixedUpdate()
    {
        if (!IsActive) return;
        remainingTime -= Time.fixedDeltaTime;
        if (remainingTime <= 0f) OnKnockbackFinished.Invoke();
    }

    public void Cancel()
    {
        bool wasActive = IsActive;
        remainingTime = 0f;
        if (wasActive) OnKnockbackFinished.Invoke();
    }
}
