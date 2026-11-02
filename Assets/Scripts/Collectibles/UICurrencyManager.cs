using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class UICurrencyManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject player;

    [Header("Currency")]
    [SerializeField, Min(0)] private int totalCurrency;

    [Header("UI")]
    [SerializeField] private TMP_Text currencyText;

    public int TotalCurrency => totalCurrency;

    private MLHealth playerHealth;
    private MLHealth subscribedPlayerHealth;

    private void OnEnable()
    {
        Currency.CurrencyTouched += HandleCurrencyTouched;

        FindPlayer();
        SubscribeToPlayerHealth();
        RefreshCurrencyText();
    }

    private void Start()
    {
        FindPlayer();
        SubscribeToPlayerHealth();
        RefreshCurrencyText();
    }

    private void OnDisable()
    {
        Currency.CurrencyTouched -= HandleCurrencyTouched;
        UnsubscribeFromPlayerHealth();
    }

    private void FindPlayer()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            playerHealth = player.GetComponentInParent<MLHealth>();
    }

    private void SubscribeToPlayerHealth()
    {
        if (playerHealth == null ||
            subscribedPlayerHealth == playerHealth)
        {
            return;
        }

        UnsubscribeFromPlayerHealth();

        subscribedPlayerHealth = playerHealth;
        subscribedPlayerHealth.OnDied.AddListener(
            HandlePlayerDied
        );

        if (subscribedPlayerHealth.IsDead)
            ResetCurrency();
    }

    private void UnsubscribeFromPlayerHealth()
    {
        if (subscribedPlayerHealth == null)
            return;

        subscribedPlayerHealth.OnDied.RemoveListener(
            HandlePlayerDied
        );

        subscribedPlayerHealth = null;
    }

    private void HandleCurrencyTouched(
        Currency currency,
        Collider2D other)
    {
        if (currency == null ||
            currency.IsCollected ||
            !ColliderBelongsToPlayer(other))
        {
            return;
        }

        AddCurrency(currency.Amount);
        currency.Collect();
    }

    private bool ColliderBelongsToPlayer(Collider2D other)
    {
        if (player == null || other == null)
            return false;

        Transform otherTransform = other.transform;
        Transform playerTransform = player.transform;

        return otherTransform == playerTransform ||
               otherTransform.IsChildOf(playerTransform);
    }

    public void AddCurrency(int amount)
    {
        if (amount <= 0)
            return;

        totalCurrency += amount;
        RefreshCurrencyText();
    }

    public void ResetCurrency()
    {
        totalCurrency = 0;
        RefreshCurrencyText();
    }

    private void HandlePlayerDied()
    {
        ResetCurrency();
    }

    private void RefreshCurrencyText()
    {
        if (currencyText != null)
            currencyText.text = totalCurrency.ToString();
    }
}
