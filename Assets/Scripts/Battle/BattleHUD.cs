using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHUD : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public Slider hpSlider;
    public Slider ghostSlider;   // barre fantôme (rouge, derrière)

    private HealthSystem _health;
    private Coroutine _ghostRoutine;
    private Coroutine _dangerRoutine;
    private RectTransform _rt;
    private Vector2 _originPos;

    // ── Setup ─────────────────────────────────────────────────────

    public void Setup(HealthSystem health)
    {
        _health = health;
        _rt = GetComponent<RectTransform>();
        _originPos = _rt.anchoredPosition;

        nameText.text = health.unitName;
        hpSlider.maxValue  = health.maxHP;
        if (ghostSlider != null) ghostSlider.maxValue = health.maxHP;
        Refresh();
    }

    // ── Refresh normal (sans animation) ──────────────────────────

    public void Refresh()
    {
        if (_health == null) return;
        float ratio = _health.Ratio;

        hpSlider.value = _health.currentHP;
        hpText.text = $"{_health.currentHP} / {_health.maxHP}";

        // Couleur barre principale : vert → orange → rouge
        SetFillColor(hpSlider, HealthColor(ratio));

        // Danger pulsé sous 20%
        if (ratio < 0.20f)
            StartDangerPulse();
        else
            StopDangerPulse();
    }

    // ── Prise de dégâts avec ghost bar ───────────────────────────

    public void AnimateDamage(bool shake = false)
    {
        if (_health == null) return;

        float oldValue = hpSlider.value;
        float newValue = _health.currentHP;

        // Barre principale : descente instantanée
        hpSlider.value = newValue;
        hpText.text = $"{(int)newValue} / {_health.maxHP}";
        SetFillColor(hpSlider, HealthColor(_health.Ratio));

        // Ghost bar : reste à l'ancienne valeur, descend en 1s
        if (ghostSlider != null)
        {
            ghostSlider.value = oldValue;
            if (_ghostRoutine != null) StopCoroutine(_ghostRoutine);
            _ghostRoutine = StartCoroutine(DrainGhost(oldValue, newValue));
        }

        if (shake) StartCoroutine(ShakeHUD());

        if (_health.Ratio < 0.20f) StartDangerPulse();
    }

    // ── Ghost bar coroutine ───────────────────────────────────────

    IEnumerator DrainGhost(float from, float to)
    {
        yield return new WaitForSeconds(0.35f);
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            ghostSlider.value = Mathf.Lerp(from, to, elapsed);
            yield return null;
        }
        ghostSlider.value = to;
    }

    // ── HUD Shake ─────────────────────────────────────────────────

    public IEnumerator ShakeHUD(float intensity = 6f, float duration = 0.35f)
    {
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

    // ── Danger pulse (<20% HP) ────────────────────────────────────

    void StartDangerPulse()
    {
        if (_dangerRoutine != null) return;
        _dangerRoutine = StartCoroutine(DangerPulse());
    }

    void StopDangerPulse()
    {
        if (_dangerRoutine == null) return;
        StopCoroutine(_dangerRoutine);
        _dangerRoutine = null;
        SetFillColor(hpSlider, HealthColor(_health?.Ratio ?? 1f));
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

    // ── Helpers ───────────────────────────────────────────────────

    static Color HealthColor(float ratio)
        => Color.Lerp(new Color(0.85f, 0.12f, 0.12f), new Color(0.18f, 0.80f, 0.28f), ratio);

    static void SetFillColor(Slider slider, Color color)
    {
        var fill = slider.fillRect?.GetComponent<Image>();
        if (fill != null) fill.color = color;
    }
}
