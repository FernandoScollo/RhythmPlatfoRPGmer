using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Attach to any enemy with MLHealth. The world-space bar is created once at runtime.
[DisallowMultipleComponent]
[RequireComponent(typeof(MLHealth))]
public class MLEnemyHealthUI : MonoBehaviour
{
    [SerializeField] private MLHealth health;
    [SerializeField] private Collider2D bodyCollider;
    [Header("World-space layout")]
    [SerializeField, Min(0.1f)] private float width = 3f;
    [SerializeField, Min(0.02f)] private float height = 0.25f;
    [Tooltip("World-space offset above the collider's top, or this transform if no collider exists.")]
    [SerializeField] private Vector2 offset = new Vector2(0f, 0.5f);
    [SerializeField] private Color backgroundColor = new Color(0.12f, 0.06f, 0.08f, 0.9f);
    [SerializeField] private Color fillColor = new Color(0.9f, 0.2f, 0.15f, 1f);
    [SerializeField] private string sortingLayer = "Character";
    [SerializeField] private int sortingOrder = 100;
    private RectTransform barRoot, fillRect;
    private Canvas canvas;
    private Image background, fill;

    private void Awake()
    {
        if (health == null) health = GetComponent<MLHealth>();
        if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
        var root = new GameObject(name + " Health UI", typeof(RectTransform), typeof(Canvas));
        SceneManager.MoveGameObjectToScene(root, gameObject.scene);
        barRoot = root.GetComponent<RectTransform>();
        canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        // Not parented to the enemy: facing changes and scale must not mirror the bar.
        background = CreateImage("Background", barRoot, backgroundColor);
        fill = CreateImage("Health", background.rectTransform, fillColor);
        fillRect = fill.rectTransform;
        ApplyLayout();
    }

    private static Image CreateImage(string label, Transform parent, Color color)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        var rect = image.rectTransform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return image;
    }

    private void OnEnable()
    {
        health.OnHealthChanged.AddListener(HandleHealthChanged);
        health.OnDied.AddListener(Refresh);
        health.OnRevived.AddListener(Refresh);
        Refresh();
    }
    // All Awake calls have completed by Start, including the health initialization.
    private void Start() { Refresh(); }
    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHealthChanged.RemoveListener(HandleHealthChanged);
            health.OnDied.RemoveListener(Refresh);
            health.OnRevived.RemoveListener(Refresh);
        }
        if (barRoot != null) barRoot.gameObject.SetActive(false);
    }
    private void HandleHealthChanged(float current, float maximum) { Refresh(); }
    private void Refresh()
    {
        if (barRoot == null || health == null) return;
        barRoot.gameObject.SetActive(isActiveAndEnabled && !health.IsDead);
        fillRect.anchorMax = new Vector2(Mathf.Clamp01(health.NormalizedHealth), 1f);
        Follow();
    }
    private void LateUpdate() { Follow(); }
    private void Follow()
    {
        if (barRoot == null) return;
        Vector3 position = transform.position;
        if (bodyCollider != null && bodyCollider.enabled)
            position = new Vector3(bodyCollider.bounds.center.x, bodyCollider.bounds.max.y, position.z);
        barRoot.position = position + (Vector3)offset;
    }
    private void ApplyLayout()
    {
        barRoot.sizeDelta = new Vector2(Mathf.Max(0.1f, width), Mathf.Max(0.02f, height));
        canvas.sortingLayerName = sortingLayer; canvas.sortingOrder = sortingOrder;
        background.color = backgroundColor; fill.color = fillColor;
    }
    private void OnValidate()
    {
        width = Mathf.Max(0.1f, width); height = Mathf.Max(0.02f, height);
        if (barRoot != null) { ApplyLayout(); Refresh(); }
    }
    private void OnDestroy() { if (barRoot != null) Destroy(barRoot.gameObject); }
}
