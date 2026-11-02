using UnityEngine;
using UnityEngine.UI;

public class MLHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MLHealth playerHealth;
    [SerializeField] private Image totalHealthBar;
    [SerializeField] private Image currentHealthBar;

    private void OnEnable()
    {
        FindPlayerHealth();

        if (playerHealth == null)
            return;

        playerHealth.OnHealthChanged.AddListener(
            HandleHealthChanged
        );

        playerHealth.OnDied.AddListener(
            HandlePlayerDied
        );

        RefreshHealthBar();
    }

    private void Start()
    {
        RefreshHealthBar();
    }

    private void OnDisable()
    {
        if (playerHealth == null)
            return;

        playerHealth.OnHealthChanged.RemoveListener(
            HandleHealthChanged
        );

        playerHealth.OnDied.RemoveListener(
            HandlePlayerDied
        );
    }

    private void FindPlayerHealth()
    {
        if (playerHealth != null)
            return;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerHealth =
                playerObject.GetComponentInParent<MLHealth>();
        }
    }

    private void HandleHealthChanged(
        float currentHealth,
        float maxHealth)
    {
        SetFillAmounts(currentHealth, maxHealth);
    }

    private void HandlePlayerDied()
    {
        if (currentHealthBar != null)
            currentHealthBar.fillAmount = 0f;
    }

    private void RefreshHealthBar()
    {
        if (playerHealth == null)
            return;

        SetFillAmounts(
            playerHealth.CurrentHealth,
            playerHealth.MaxHealth
        );
    }

    private void SetFillAmounts(
        float currentHealth,
        float maxHealth)
    {
        if (totalHealthBar != null)
            totalHealthBar.fillAmount = maxHealth > 0f ? 1f : 0f;

        if (currentHealthBar != null)
        {
            currentHealthBar.fillAmount = maxHealth > 0f
                ? Mathf.Clamp01(currentHealth / maxHealth)
                : 0f;
        }
    }
}
