using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpScreen : MonoBehaviour
{
    public CanvasGroup screenGroup;

    private readonly List<BonusCardUI>       _cards   = new();
    private readonly Dictionary<string, int> _stacks  = new();
    private System.Action<BonusData>         _onChosen;
    private Canvas                           _canvas;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
    }

    public void Show(int level, System.Action<BonusData> onChosen)
    {
        _onChosen = onChosen;
        var bonuses = BonusDatabase.DrawThree(_stacks);

        gameObject.SetActive(true);
        Time.timeScale = 0f;

        if (screenGroup != null) screenGroup.alpha = 0f;

        for (int i = 0; i < _cards.Count; i++)
        {
            _cards[i].gameObject.SetActive(i < bonuses.Count);
            if (i < bonuses.Count)
                _cards[i].Setup(bonuses[i]);
        }

        StartCoroutine(FadeIn());
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        // Chercher la caméra du canvas
        Camera cam = (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceCamera)
            ? _canvas.worldCamera
            : null; // null = ScreenSpaceOverlay

        foreach (var card in _cards)
        {
            if (!card.gameObject.activeSelf) continue;
            var rt = card.GetComponent<RectTransform>();
            if (rt == null) continue;

            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, Input.mousePosition, cam, out localPoint))
            {
                if (rt.rect.Contains(localPoint))
                {
                    Choose(card.Data);
                    return;
                }
            }
        }
    }

    private void Choose(BonusData bonus)
    {
        if (!_stacks.ContainsKey(bonus.id)) _stacks[bonus.id] = 0;
        _stacks[bonus.id]++;
        Time.timeScale = 1f;
        gameObject.SetActive(false);
        _onChosen?.Invoke(bonus);
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < 0.25f)
        {
            t += Time.unscaledDeltaTime;
            if (screenGroup != null) screenGroup.alpha = Mathf.Clamp01(t / 0.25f);
            yield return null;
        }
        if (screenGroup != null) screenGroup.alpha = 1f;
    }

    public void AddCard(BonusCardUI card) => _cards.Add(card);
}

public class BonusCardUI : MonoBehaviour
{
    public Image           cardBG;
    public Image           iconImage;
    public TextMeshProUGUI rarityLabel;
    public TextMeshProUGUI bonusName;
    public TextMeshProUGUI description;

    public BonusData Data { get; private set; }

    public void Setup(BonusData data)
    {
        Data = data;
        var col = BonusData.GetColor(data.rarity);

        if (cardBG      != null) cardBG.color  = new Color(col.r * 0.12f, col.g * 0.12f, col.b * 0.12f, 0.97f);
        if (iconImage   != null) iconImage.color = new Color(col.r * 0.4f, col.g * 0.4f, col.b * 0.4f, 0.9f);
        if (rarityLabel != null) { rarityLabel.text = BonusData.GetRarityLabel(data.rarity); rarityLabel.color = col; }
        if (bonusName   != null) { bonusName.text = data.name; bonusName.color = Color.white; }
        if (description != null) { description.text = data.description; description.color = new Color(0.78f, 0.72f, 0.88f); }
    }
}
