using UnityEngine;
using TMPro;

/// <summary>
/// Applique la police pixel art à tous les TextMeshProUGUI de la scène de combat.
/// Attach sur le CombatManager ou n'importe quel GameObject actif au démarrage.
/// </summary>
public class BattleFontApplier : MonoBehaviour
{
    [Header("Glisse le TMP Font Asset ici (.asset, pas le .ttf)")]
    [SerializeField] private TMP_FontAsset pixelFont;

    private void Awake()
    {
        if (pixelFont == null)
        {
            Debug.LogWarning("[BattleFontApplier] Aucun font assigné !");
            return;
        }

        var allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var tmp in allTexts)
            tmp.font = pixelFont;

        Debug.Log($"[BattleFontApplier] Police appliquée à {allTexts.Length} éléments.");
    }
}
