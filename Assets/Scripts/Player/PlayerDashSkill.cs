using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerDashSkill : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private float dashForce = 12f;
    [SerializeField] private float verticalDashForce = 2f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashBrakeStrength = 50f;

    [Header("Events")]
    public UnityEvent OnDashStarted = new UnityEvent();
    public UnityEvent OnDashFinished = new UnityEvent();

    private Rigidbody2D body;
    private MLPlayerMovement playerMovement;
    private MLHealth health;
    private MLPlayerStats playerStats;
    private float GuitarDashForce => playerStats != null ? playerStats.GuitarDashForce : dashForce;
    private float GuitarVerticalDashForce => playerStats != null ? playerStats.GuitarVerticalDashForce : verticalDashForce;
    private float GuitarDashDuration => playerStats != null ? playerStats.GuitarDashDuration : dashDuration;
    private float GuitarDashBrakeStrength => playerStats != null ? playerStats.GuitarDashBrakeStrength : dashBrakeStrength;

    private bool isDashing;
    private PlayerBomboDashSkill otherDash;
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
        otherDash = GetComponent<PlayerBomboDashSkill>();
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
        if (Input.GetKeyDown(KeyCode.J)) TryDash();
    }

    public bool TryDash()
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0f || isDashing || CooldownRemaining > 0f ||
            !playerMovement.isActiveAndEnabled || playerMovement.IsKnockbackActive ||
            playerMovement.IsPlatformFrozen || (health != null && health.IsDead)) return false;
        // Reject unknown movement locks, but allow replacing the other dash.
        if (playerMovement.IsDashActive && (otherDash == null || !otherDash.IsDashing)) return false;
        if (otherDash != null) otherDash.CancelDash();
        activeCooldown = Mathf.Max(0f, playerStats != null ? playerStats.GuitarDashCooldown : 2f);
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

        // Dash hacia donde mira el Player.
        float dashDirection = DashDirection;

        // Velocidad inicial del dash
        body.velocity = new Vector2(
            dashDirection * GuitarDashForce,
            body.velocity.y + GuitarVerticalDashForce
        );

        float elapsedTime = 0f;

        while (elapsedTime < GuitarDashDuration)
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
                    GuitarDashBrakeStrength * Time.deltaTime
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
