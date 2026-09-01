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
    public UnityEvent OnDashStarted;
    public UnityEvent OnDashFinished;

    private Rigidbody2D body;
    private MLPlayerMovement playerMovement;

    private bool isDashing;

    public bool IsDashing => isDashing;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<MLPlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J) && !isDashing && !playerMovement.IsDashActive)
        {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;

        playerMovement.SetDashActive(true);

        OnDashStarted?.Invoke();

        // Dirección contraria a donde mira el Player
        float dashDirection = -Mathf.Sign(transform.localScale.x);

        // Velocidad inicial del dash
        body.velocity = new Vector2(
            dashDirection * dashForce,
            body.velocity.y + verticalDashForce
        );

        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
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
                    dashBrakeStrength * Time.deltaTime
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