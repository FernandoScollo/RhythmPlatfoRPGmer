using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RhythmPlatforms))]
public class MLDamageAtRhythmPlatforms : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField, Min(0f)] private float damage = 1f;

    private RhythmPlatforms rhythmPlatform;
    private MLHealth playerHealth;

    private void Awake()
    {
        rhythmPlatform = GetComponent<RhythmPlatforms>();
    }

    private void OnEnable()
    {
        if (rhythmPlatform == null)
            rhythmPlatform = GetComponent<RhythmPlatforms>();

        rhythmPlatform.OnPlayerEjected.AddListener(
            HandlePlayerEjected
        );
    }

    private void OnDisable()
    {
        if (rhythmPlatform == null)
            return;

        rhythmPlatform.OnPlayerEjected.RemoveListener(
            HandlePlayerEjected
        );
    }

    private void HandlePlayerEjected(GameObject playerObject)
    {
        if (playerObject == null)
            return;

        playerHealth =
            playerObject.GetComponentInParent<MLHealth>();

        if (playerHealth == null)
            return;

        playerHealth.TakeDamage(damage);
    }
}
