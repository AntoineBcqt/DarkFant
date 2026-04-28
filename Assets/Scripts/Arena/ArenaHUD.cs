using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD Arena — barre HP + barre XP + niveau affiché.
/// </summary>
public class ArenaHUD : MonoBehaviour
{
    [Header("HP Bar")]
    public PlayerCombat   playerCombat;
    public Image          hpFill;

    [Header("XP Bar")]
    public Image          xpFill;
    public TextMeshProUGUI levelText;

    private RectTransform _hpRT;
    private RectTransform _xpRT;

    private void Start()
    {
        if (hpFill != null)
        {
            _hpRT = hpFill.GetComponent<RectTransform>();
            _hpRT.pivot = new Vector2(0f, 0.5f);
        }
        if (xpFill != null)
        {
            _xpRT = xpFill.GetComponent<RectTransform>();
            _xpRT.pivot = new Vector2(0f, 0.5f);
            _xpRT.localScale = new Vector3(0f, 1f, 1f); // commence vide
        }

        if (ArenaXPSystem.Instance != null)
        {
            ArenaXPSystem.Instance.OnXPChanged += UpdateXPBar;
            ArenaXPSystem.Instance.OnLevelUp   += UpdateLevel;
            UpdateLevel(ArenaXPSystem.Instance.currentLevel);
            UpdateXPBar(0f, ArenaXPSystem.Instance.XPRequired);
        }
    }

    private void Update()
    {
        if (playerCombat == null || hpFill == null || _hpRT == null) return;
        float ratio = Mathf.Clamp01(playerCombat.CurrentHP / playerCombat.maxHP);
        _hpRT.localScale = new Vector3(ratio, 1f, 1f);
        hpFill.color = Color.Lerp(
            new Color(0.85f, 0.12f, 0.12f),
            new Color(0.18f, 0.80f, 0.28f),
            ratio);
    }

    private void UpdateXPBar(float currentXP, float requiredXP)
    {
        if (_xpRT == null) return;
        // Se remplit de gauche à droite au fur et à mesure des kills
        float ratio = requiredXP > 0 ? Mathf.Clamp01(currentXP / requiredXP) : 0f;
        _xpRT.localScale = new Vector3(ratio, 1f, 1f);
    }

    private void UpdateLevel(int level)
    {
        if (levelText != null)
            levelText.text = $"LVL {level}";
    }

    private void OnDestroy()
    {
        if (ArenaXPSystem.Instance != null)
        {
            ArenaXPSystem.Instance.OnXPChanged -= UpdateXPBar;
            ArenaXPSystem.Instance.OnLevelUp   -= UpdateLevel;
        }
    }
}
