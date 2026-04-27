using UnityEngine;
using UnityEngine.UI;

/// <summary>HUD joueur — barre de vie mise à jour en temps réel.</summary>
public class PlayerHUD : MonoBehaviour
{
    public PlayerCombat playerCombat;
    public Image        hpFill;

    private void Update()
    {
        if (playerCombat == null || hpFill == null) return;
        float ratio    = playerCombat.CurrentHP / playerCombat.maxHP;
        hpFill.fillAmount = ratio;
        hpFill.color      = Color.Lerp(
            new Color(0.85f, 0.12f, 0.12f),
            new Color(0.18f, 0.80f, 0.28f),
            ratio);
    }
}
