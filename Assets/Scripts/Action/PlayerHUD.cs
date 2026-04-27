using UnityEngine;
using UnityEngine.UI;

/// <summary>HUD joueur — barre de vie via scale RectTransform.</summary>
public class PlayerHUD : MonoBehaviour
{
    public PlayerCombat playerCombat;
    public Image hpFill;

    private RectTransform _fillRT;
    private float _fullWidth;

    private void Start()
    {
        if (hpFill != null)
        {
            _fillRT = hpFill.GetComponent<RectTransform>();
            _fullWidth = _fillRT.rect.width;
            if (_fullWidth <= 0) _fullWidth = 300f; // fallback
        }
    }

    private void Update()
    {
        if (playerCombat == null || hpFill == null || _fillRT == null) return;
        float ratio = Mathf.Clamp01(playerCombat.CurrentHP / playerCombat.maxHP);

        // Méthode scale X — marche sans sprite Filled
        _fillRT.localScale = new Vector3(ratio, 1f, 1f);
        // Couleur verte → rouge selon la vie
        hpFill.color = Color.Lerp(
            new Color(0.85f, 0.12f, 0.12f),  // rouge
            new Color(0.18f, 0.80f, 0.28f),  // vert
            ratio);
    }
}