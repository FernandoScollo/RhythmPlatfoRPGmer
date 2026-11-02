using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerBomboDashSkill : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private KeyCode dashKey = KeyCode.K;
    [SerializeField] private float dashForce = 12f;
    [SerializeField] private float verticalDashForce = 4f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashBrakeStrength = 50f;

    [Header("Events")]
    public UnityEvent OnDashStarted = new UnityEvent();
    public UnityEvent OnDashFinished = new UnityEvent();

    private Rigidbody2D body;
    private MLPlayerMovement playerMovement;
    private MLHealth health;
    private MLPlayerStats playerStats;
    private float BomboDashForce => playerStats != null ? playerStats.BomboDashForce : dashForce;
    private float BomboVerticalDashForce => playerStats != null ? playerStats.BomboVerticalDashForce : verticalDashForce;
    private float BomboDashDuration => playerStats != null ? playerStats.BomboDashDuration : dashDuration;
    private float BomboDashBrakeStrength => playerStats != null ? playerStats.BomboDashBrakeStrength : dashBrakeStrength;

    private bool isDashing;
    private PlayerDashSkill otherDash;
    private float readyAt, activeCooldown;
    public float CooldownRemaining => Mathf.Max(0f, readyAt - Time.time);
    public float CooldownProgress => activeCooldown <= 0f ? 1f : 1f - Mathf.Clamp01(CooldownRemaining / activeCooldown);

    public bool IsDashing => isDashing;
    public float DashDirection { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<MLPlayerMovement>();
        health = GetComponent<MLHealth>();
        playerStats = GetComponent<MLPlayerStats>();
        otherDash = GetComponent<PlayerDashSkill>();
    }

    private void OnEnable()
    {
        if (health != null) health.OnDied.AddListener(CancelDash);
    }

    private void OnDisable()
    {
        if (health != null) health.OnDied.RemoveListener(CancelDash);
        CancelDash();
    }

    private void Update()
    {
        if (Input.GetKeyDown(dashKey)) TryDash();
    }

    public bool TryDash()
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0f || isDashing || CooldownRemaining > 0f ||
            !playerMovement.isActiveAndEnabled || playerMovement.IsKnockbackActive ||
            playerMovement.IsPlatformFrozen || (health != null && health.IsDead)) return false;
        // Reject unknown movement locks, but allow replacing the other dash.
        if (playerMovement.IsDashActive && (otherDash == null || !otherDash.IsDashing)) return false;
        if (otherDash != null) otherDash.CancelDash();
        activeCooldown = Mathf.Max(0f, playerStats != null ? playerStats.BomboDashCooldown : 2f);
        readyAt = Time.time + activeCooldown;
        StartCoroutine(Dash());
        return true;
    }

    public void CancelDash()
    {
        if (!isDashing) return;
        StopAllCoroutines();
        isDashing = false;
        playerMovement.SetDashActive(false);
        OnDashFinished?.Invoke();
    }

    private IEnumerator Dash()
    {
        isDashing = true;

        playerMovement.SetDashActive(true);

        DashDirection = Mathf.Sign(transform.localScale.x);
        OnDashStarted?.Invoke();

        // Dash hacia donde mira el Player
        float dashDirection = DashDirection;

        // Velocidad inicial del dash
        body.velocity = new Vector2(
            dashDirection * BomboDashForce,
            body.velocity.y + BomboVerticalDashForce
        );

        float elapsedTime = 0f;

        while (elapsedTime < BomboDashDuration)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");

            // Si el jugador presiona en dirección contraria
            // al dash, frenamos progresivamente.
            if (horizontalInput != 0 &&
                Mathf.Sign(horizontalInput) != Mathf.Sign(dashDirection))
            {
                float newVelocityX = Mathf.MoveTowards(
                    body.velocity.x,
                    0f,
                    BomboDashBrakeStrength * Time.deltaTime
                );

                body.velocity = new Vector2(
                    newVelocityX,
                    body.velocity.y
                );
            }

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        playerMovement.SetDashActive(false);

        isDashing = false;

        OnDashFinished?.Invoke();
    }
}
