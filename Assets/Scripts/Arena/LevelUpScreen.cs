using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpScreen : MonoBehaviour
{
    public CanvasGroup screenGroup;

    private readonly List<BonusCardUI> _cards = new();
    private readonly Dictionary<string, int> _stacks = new();
    private System.Action<BonusData> _onChosen;
    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _cards.Clear();
        foreach (var card in GetComponentsInChildren<BonusCardUI>(true))
            _cards.Add(card);
        Debug.Log($"[LevelUpScreen] Awake — {_cards.Count} cartes trouvées");
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
        if (bonus == null) { Debug.LogWarning("[LevelUpScreen] bonus null !"); return; }
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