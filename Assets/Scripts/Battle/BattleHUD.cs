using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD d'un combattant : HP avec ghost bar, MP, Limit Break, et animations.
/// </summary>
public class BattleHUD : MonoBehaviour
{
    [Header("Nom")]
    public TextMeshProUGUI nameText;

    [Header("HP")]
    public Slider          hpSlider;
    public Slider          ghostSlider;
    public TextMeshProUGUI hpText;

    [Header("MP")]
    public Slider          mpSlider;
    public TextMeshProUGUI mpText;

    [Header("Limit Break")]
    public Slider          limitSlider;
    public TextMeshProUGUI limitText;
    public GameObject      limitReadyFX;   // optionnel : flash/glow quand prête

    // ── Runtime ──────────────────────────────────────────────────
    private Coroutine    _ghostRoutine;
    private Coroutine    _dangerRoutine;
    private Coroutine    _limitPulse;
    private RectTransform _rt;
    private Vector2       _originPos;

    // ─────────────────────────────────────────────────────────────

    public void Setup(BattleUnit unit)
    {
        _rt        = GetComponent<RectTransform>();
        _originPos = _rt != null ? _rt.anchoredPosition : Vector2.zero;

        if (nameText != null) nameText.text = unit.unitName;

        // HP
        if (hpSlider   != null) { hpSlider.maxValue   = unit.maxHP;         hpSlider.value   = unit.CurrentHP; }
        if (ghostSlider != null) { ghostSlider.maxValue = unit.maxHP;        ghostSlider.value = unit.CurrentHP; }

        // MP
        if (mpSlider != null) { mpSlider.maxValue = unit.maxMP; mpSlider.value = unit.CurrentMP; }

        // Limit
        if (limitSlider != null) { limitSlider.maxValue = unit.limitBreakMax; limitSlider.value = 0; }
        if (limitReadyFX != null) limitReadyFX.SetActive(false);

        RefreshHP(unit);
        RefreshMP(unit);
        RefreshLimit(unit);
    }

    // ── Refresh ──────────────────────────────────────────────────

    public void RefreshHP(BattleUnit unit)
    {
        if (hpSlider != null) hpSlider.value = unit.CurrentHP;
        if (hpText   != null) hpText.text    = $"{unit.CurrentHP} / {unit.maxHP}";
        SetFillColor(hpSlider, HealthColor(unit.HPRatio));

        if (unit.HPRatio < 0.20f) StartDangerPulse();
        else                      StopDangerPulse(unit);
    }

    public void RefreshMP(BattleUnit unit)
    {
        if (mpSlider != null) mpSlider.value = unit.CurrentMP;
        if (mpText   != null) mpText.text    = $"{unit.CurrentMP} / {unit.maxMP}";
    }

    public void RefreshLimit(BattleUnit unit)
    {
        if (limitSlider != null) limitSlider.value = unit.LimitBreak;
        if (limitText   != null) limitText.text    = unit.IsLimitReady ? "PRÊTE !" : $"{unit.LimitBreak} / {unit.limitBreakMax}";
        SetFillColor(limitSlider, unit.IsLimitReady
            ? new Color(1f, 0.85f, 0.1f)
            : new Color(0.6f, 0.3f, 0.9f));
    }

    public void OnLimitReady()
    {
        if (limitReadyFX != null) limitReadyFX.SetActive(true);
        if (_limitPulse  != null) StopCoroutine(_limitPulse);
        _limitPulse = StartCoroutine(LimitPulse());
    }

    // ── Dégâts animés ────────────────────────────────────────────

    public void AnimateDamage(BattleUnit unit, bool shake = false)
    {
        float oldHP = ghostSlider != null ? ghostSlider.value : unit.CurrentHP;

        RefreshHP(unit);

        if (ghostSlider != null)
        {
            ghostSlider.value = oldHP;
            if (_ghostRoutine != null) StopCoroutine(_ghostRoutine);
            _ghostRoutine = StartCoroutine(DrainGhost(oldHP, unit.CurrentHP));
        }

        if (shake && _rt != null) StartCoroutine(ShakeHUD());
    }

    // ── Coroutines ───────────────────────────────────────────────

    IEnumerator DrainGhost(float from, float to)
    {
        yield return new WaitForSeconds(0.35f);
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            if (ghostSlider != null) ghostSlider.value = Mathf.Lerp(from, to, elapsed);
            yield return null;
        }
        if (ghostSlider != null) ghostSlider.value = to;
    }

    public IEnumerator ShakeHUD(float intensity = 6f, float duration = 0.35f)
    {
        if (_rt == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(intensity, 0f, elapsed / duration);
            _rt.anchoredPosition = _originPos + Random.insideUnitCircle * strength;
            yield return null;
        }
        _rt.anchoredPosition = _originPos;
    }

    void StartDangerPulse()
    {
        if (_dangerRoutine != null) return;
        _dangerRoutine = StartCoroutine(DangerPulse());
    }

    void StopDangerPulse(BattleUnit unit)
    {
        if (_dangerRoutine == null) return;
        StopCoroutine(_dangerRoutine);
        _dangerRoutine = null;
        SetFillColor(hpSlider, HealthColor(unit.HPRatio));
    }

    IEnumerator DangerPulse()
    {
        Color danger = new Color(0.9f, 0.1f, 0.1f);
        Color bright = new Color(1f, 0.4f, 0.1f);
        while (true)
        {
            float t = Mathf.PingPong(Time.time * 2.5f, 1f);
            SetFillColor(hpSlider, Color.Lerp(danger, bright, t));
            yield return null;
        }
    }

    IEnumerator LimitPulse()
    {
        Color a = new Color(1f, 0.85f, 0.1f);
        Color b = new Color(1f, 0.5f, 0.0f);
        while (true)
        {
            float t = Mathf.PingPong(Time.time * 3f, 1f);
            SetFillColor(limitSlider, Color.Lerp(a, b, t));
            yield return null;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────

    static Color HealthColor(float ratio)
        => Color.Lerp(new Color(0.85f, 0.12f, 0.12f), new Color(0.18f, 0.80f, 0.28f), ratio);

    static void SetFillColor(Slider slider, Color color)
    {
        if (slider == null) return;
        var fill = slider.fillRect?.GetComponent<Image>();
        if (fill != null) fill.color = color;
    }
}
