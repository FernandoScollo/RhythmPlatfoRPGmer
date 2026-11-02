using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class Currency : MonoBehaviour
{
    [Header("Currency")]
    [SerializeField, Min(0)] private int amount = 1;

    public static event Action<Currency, Collider2D> CurrencyTouched;

    public int Amount => amount;
    public bool IsCollected { get; private set; }

    private void Reset()
    {
        Collider2D currencyCollider = GetComponent<Collider2D>();

        if (currencyCollider != null)
            currencyCollider.isTrigger = true;
    }

    private void OnEnable()
    {
        IsCollected = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsCollected)
            return;

        CurrencyTouched?.Invoke(this, other);
    }

    public void Collect()
    {
        if (IsCollected)
            return;

        IsCollected = true;
        gameObject.SetActive(false);
    }
}
