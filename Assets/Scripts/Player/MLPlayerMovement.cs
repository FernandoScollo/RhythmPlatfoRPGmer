using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MLPlayerMovement : MonoBehaviour
{
    [Header("Movements")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float wallJumpCooldown;
    [SerializeField] private float wallJumpForce;
    [SerializeField] private float jumpGravity;
    [SerializeField] private float wallJumpGravity;

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

    public bool IsDashActive { get; private set; }



    public bool IsMoving =>
        Mathf.Abs(horizontalInput) > 0.01f;

    public float HorizontalInput =>
        horizontalInput;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        innitialLocalScaleX = transform.localScale.x;

        IsGrounded = isGrounded();
        wasGrounded = IsGrounded;
    }

    public void SetDashActive(bool active)
    {
        IsDashActive = active;
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

        // Flip player when moving left-right
        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(
                innitialLocalScaleX,
                innitialLocalScaleX,
                innitialLocalScaleX
            );
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(
                -innitialLocalScaleX,
                innitialLocalScaleX,
                innitialLocalScaleX
            );
        }

        // Wall jump logic
        if (wallJumpCooldown > 0.2f)
        {
            if (!IsDashActive)
            {
                body.velocity = new Vector2(
                horizontalInput * speed,
                body.velocity.y);
            }

            if (onWall() && !IsGrounded)
            {
                bool pressingTowardsWall =
                    (horizontalInput > 0 && transform.localScale.x > 0) ||
                    (horizontalInput < 0 && transform.localScale.x < 0);

                if (pressingTowardsWall)
                {
                    body.gravityScale = 0;
                    body.velocity = Vector2.zero;
                }
                else
                {
                    body.gravityScale = jumpGravity;
                }
            }
            else
            {
                body.gravityScale = jumpGravity;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                Jump();
            }
        }
        else
        {
            wallJumpCooldown += Time.deltaTime;
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
        else if (onWall() && !IsGrounded)
        {
            if (horizontalInput == 0)
            {
                body.velocity = new Vector2(
                    -Mathf.Sign(transform.localScale.x) * wallJumpForce,
                    0
                );

                transform.localScale = new Vector3(
                    -Mathf.Sign(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
            else
            {
                body.velocity = new Vector2(
                    -Mathf.Sign(transform.localScale.x) * 3,
                    wallJumpForce
                );
            }

            wallJumpCooldown = 0;

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

    private bool onWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            0,
            new Vector2(transform.localScale.x, 0),
            0.1f,
            wallLayer
        );

        return raycastHit.collider != null;
    }

    public bool CanAttack()
    {
        return !IsMoving &&
               IsGrounded &&
               !onWall();
    }
}