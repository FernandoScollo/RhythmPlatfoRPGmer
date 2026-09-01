using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MLPlayerMovement : MonoBehaviour
{
    [Header("Movements")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float jumpGravity;

    [Header("Collision Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Events")]
    public UnityEvent OnJump;
    public UnityEvent OnLand;

    private Rigidbody2D body;
    private BoxCollider2D boxCollider;

    public float innitialLocalScaleX;

    private float horizontalInput;

    private bool wasGrounded;

    // Estados públicos para otros sistemas
    public bool IsGrounded { get; private set; }

    public bool IsMoving =>
        Mathf.Abs(horizontalInput) > 0.01f;

    public float HorizontalInput =>
        horizontalInput;

    // El Dash utiliza esto para tomar temporalmente
    // el control de la velocidad horizontal.
    public bool IsDashActive { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        innitialLocalScaleX = transform.localScale.x;

        IsGrounded = isGrounded();
        wasGrounded = IsGrounded;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        // Actualizar estado de grounded
        IsGrounded = isGrounded();

        // Detectar aterrizaje
        if (!wasGrounded && IsGrounded)
        {
            OnLand?.Invoke();
        }

        wasGrounded = IsGrounded;

        // Flip player cuando se mueve
        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(
                innitialLocalScaleX,
                transform.localScale.y,
                transform.localScale.z
            );
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(
                -innitialLocalScaleX,
                transform.localScale.y,
                transform.localScale.z
            );
        }

        // Movimiento horizontal
        if (!IsDashActive)
        {
            float horizontalVelocity = horizontalInput * speed;

            if (IsOnWall())
            {
                horizontalVelocity = 0f;
            }
            body.velocity = new Vector2(
                horizontalVelocity,
                body.velocity.y
            );
        }

        // Gravedad
        body.gravityScale = jumpGravity;

        // Salto
        if (Input.GetKey(KeyCode.Space))
        {
            Jump();
        }
    }

    private void Jump()
    {
        if (IsGrounded)
        {
            body.velocity = new Vector2(
                body.velocity.x,
                jumpPower
            );

            OnJump?.Invoke();
        }
    }

    private bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            0,
            Vector2.down,
            0.1f,
            groundLayer
        );

        return raycastHit.collider != null;
    }

    public void SetDashActive(bool active)
    {
        IsDashActive = active;
    }

    public bool CanAttack()
    {
        return !IsMoving && IsGrounded;
    }

    private bool IsOnWall()
    {
        if (Mathf.Abs(horizontalInput) < 0.01f)
            return false;

        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            0,
            Vector2.right * Mathf.Sign(horizontalInput),
            0.05f,
            wallLayer
        );

        return hit.collider != null;
    }
}