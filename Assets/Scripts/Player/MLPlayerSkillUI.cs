using UnityEngine;
using UnityEngine.UI;

// UI only: availability and cooldown remain owned by each skill.
[DisallowMultipleComponent]
public class MLPlayerSkillUI : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Image guitarTop;
    [SerializeField] private Image bomboTop;
    [Header("Create missing bars under this Canvas")]
    [SerializeField] private Sprite bottomSprite;
    [SerializeField] private Sprite topSprite;
    [SerializeField] private Sprite guitarIcon;
    [SerializeField] private Sprite bomboIcon;
    [SerializeField] private Vector2 topRightOffset = new Vector2(-30f, -35f);
    [SerializeField] private Vector2 barSize = new Vector2(140f, 28f);
    [SerializeField] private float rowSpacing = 52f;
    private PlayerDashSkill guitar;
    private PlayerBomboDashSkill bombo;

    private void Awake()
    {
        if (player != null)
        {
            guitar = player.GetComponent<PlayerDashSkill>();
            bombo = player.GetComponent<PlayerBomboDashSkill>();
        }
        if (guitar == null || bombo == null)
            Debug.LogWarning("MLPlayerSkillUI requires a player with both dash skills.", this);
        if (bomboTop == null) bomboTop = CreateBar("Bombo Cooldown", bomboIcon, 0);
        if (guitarTop == null) guitarTop = CreateBar("Guitar Cooldown", guitarIcon, 1);
        Configure(guitarTop); Configure(bomboTop);
        Refresh();
    }
    private static void Configure(Image image)
    {
        if (image == null) return;
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = (int)Image.OriginHorizontal.Left;
        image.raycastTarget = false;
    }
    private Image CreateBar(string label, Sprite icon, int row)
    {
        var root = new GameObject(label, typeof(RectTransform));
        root.transform.SetParent(transform, false);
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
        rect.sizeDelta = barSize;
        rect.anchoredPosition = topRightOffset + Vector2.down * rowSpacing * row;
        Image bottom = CreateImage("Botton UI", rect, bottomSprite);
        if (bottomSprite == null) bottom.color = Color.gray;
        Image top = CreateImage("Top UI", rect, topSprite);
        if (topSprite == null) top.color = Color.yellow;
        if (icon != null)
        {
            Image emblem = CreateImage("Instrument", rect, icon);
            var r = emblem.rectTransform;
            r.anchorMin = r.anchorMax = new Vector2(0f, 0.5f);
            r.sizeDelta = new Vector2(40f, 40f);
            r.anchoredPosition = new Vector2(-35f, 0f);
            emblem.preserveAspect = true;
        }
        return top;
    }
    private static Image CreateImage(string label, Transform parent, Sprite sprite)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.sprite = sprite; image.raycastTarget = false;
        var rect = image.rectTransform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return image;
    }
    private void LateUpdate() { Refresh(); }
    private void Refresh()
    {
        if (guitarTop != null) guitarTop.fillAmount = guitar != null ? guitar.CooldownProgress : 0f;
        if (bomboTop != null) bomboTop.fillAmount = bombo != null ? bombo.CooldownProgress : 0f;
    }
}
