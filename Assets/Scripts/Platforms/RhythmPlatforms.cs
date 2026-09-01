using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmPlatforms : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoxCollider2D platformCollider;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Player Ejection")]
    [SerializeField] private float ejectionDuration = 0.15f;
    [SerializeField] private float pushSpeed = 8f;
    [SerializeField] private float exitMargin = 0.2f;

    private Rigidbody2D playerBody;
    private Collider2D playerCollider;

    private bool hasEjectedPlayer;
    private Coroutine ejectionCoroutine;

    private void Awake()
    {
        if (platformCollider == null)
        {
            platformCollider = GetComponent<BoxCollider2D>();
        }

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;

            playerBody =
                playerObject.GetComponent<Rigidbody2D>();

            playerCollider =
                playerObject.GetComponent<Collider2D>();
        }
    }

    // =====================================================
    // PREPARE FOR ACTIVATION
    // =====================================================

    public bool PrepareForActivation()
    {
        if (hasEjectedPlayer)
            return false;

        if (player == null)
            return false;

        if (platformCollider == null)
            return false;

        if (playerCollider == null)
            return false;

        Bounds platformBounds = GetPlatformBounds();

        if (!IsPlayerInside(platformBounds))
            return false;

        hasEjectedPlayer = true;

        ejectionCoroutine =
            StartCoroutine(EjectPlayer(platformBounds));

        return true;
    }

    // =====================================================
    // RESET
    // =====================================================

    public void ResetActivation()
    {
        hasEjectedPlayer = false;

        if (ejectionCoroutine != null)
        {
            StopCoroutine(ejectionCoroutine);
            ejectionCoroutine = null;
        }

        if (platformCollider != null)
        {
            platformCollider.enabled = false;
        }
    }

    // =====================================================
    // GET PLATFORM BOUNDS
    // =====================================================

    private Bounds GetPlatformBounds()
    {
        Vector2 worldCenter =
            transform.TransformPoint(
                platformCollider.offset
            );

        Vector2 worldSize =
            Vector2.Scale(
                platformCollider.size,
                transform.lossyScale
            );

        return new Bounds(
            worldCenter,
            worldSize
        );
    }

    // =====================================================
    // PLAYER INSIDE
    // =====================================================

    private bool IsPlayerInside(Bounds platformBounds)
    {
        Bounds playerBounds =
            playerCollider.bounds;

        return platformBounds.Intersects(
            playerBounds
        );
    }

    // =====================================================
    // EJECT PLAYER
    // =====================================================

    private IEnumerator EjectPlayer(
        Bounds platformBounds)
    {
        float distanceToLeft =
            Mathf.Abs(
                player.position.x -
                platformBounds.min.x
            );

        float distanceToRight =
            Mathf.Abs(
                platformBounds.max.x -
                player.position.x
            );

        int direction;

        if (distanceToLeft < distanceToRight)
        {
            direction = -1;
        }
        else
        {
            direction = 1;
        }

        // Calculamos cuánto necesita recorrer
        // el Player para quedar completamente
        // fuera de la plataforma.

        float playerHalfWidth =
            playerCollider.bounds.extents.x;

        float requiredDistance;

        if (direction < 0)
        {
            requiredDistance =
                player.position.x -
                platformBounds.min.x +
                playerHalfWidth +
                exitMargin;
        }
        else
        {
            requiredDistance =
                platformBounds.max.x -
                player.position.x +
                playerHalfWidth +
                exitMargin;
        }

        // Calculamos la velocidad necesaria para
        // recorrer esa distancia durante la duración
        // de la expulsión.

        float requiredSpeed =
            requiredDistance /
            ejectionDuration;

        float finalSpeed =
            Mathf.Max(
                pushSpeed,
                requiredSpeed
            );

        float originalVelocityX =
            playerBody.velocity.x;

        float elapsedTime = 0f;

        // La plataforma todavía NO tiene collider.
        // El Player puede atravesar su espacio
        // durante esta expulsión.

        while (elapsedTime < ejectionDuration)
        {
            playerBody.velocity =
                new Vector2(
                    direction * finalSpeed,
                    playerBody.velocity.y
                );

            elapsedTime += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        // Nos aseguramos de que el Player haya
        // salido completamente de la plataforma.

        float finalX;

        if (direction < 0)
        {
            finalX =
                platformBounds.min.x -
                playerHalfWidth -
                exitMargin;
        }
        else
        {
            finalX =
                platformBounds.max.x +
                playerHalfWidth +
                exitMargin;
        }

        // Solo corregimos si por alguna razón
        // la física no llegó suficientemente lejos.
        // Esto evita el atrapamiento.

        if (direction < 0 &&
            playerBody.position.x > finalX)
        {
            playerBody.position =
                new Vector2(
                    finalX,
                    playerBody.position.y
                );
        }
        else if (direction > 0 &&
                 playerBody.position.x < finalX)
        {
            playerBody.position =
                new Vector2(
                    finalX,
                    playerBody.position.y
                );
        }

        // Recuperamos el movimiento normal.
        playerBody.velocity =
            new Vector2(
                originalVelocityX,
                playerBody.velocity.y
            );

        // Ahora sí aparece físicamente la plataforma.
        platformCollider.enabled = true;

        ejectionCoroutine = null;
    }
}
