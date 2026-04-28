using System;
using UnityEngine;

/// <summary>
/// Système XP et niveaux du mode Arena.
/// Singleton accessible depuis n'importe quel script.
/// </summary>
public class ArenaXPSystem : MonoBehaviour
{
    public static ArenaXPSystem Instance { get; private set; }

    [Header("Progression")]
    public int   currentLevel = 1;
    public float currentXP   = 0f;

    // XP requis par niveau — chaque palier = base * (niveau ^ scalingPower)
    [SerializeField] private float baseXPRequired  = 100f;
    [SerializeField] private float scalingPower     = 1.25f;

    // Événements
    public event Action<int>   OnLevelUp;   // nouveau niveau
    public LevelUpScreen levelUpScreen;
    public event Action<float, float> OnXPChanged; // xp actuel, xp requis

    public float XPRequired => Mathf.Floor(baseXPRequired * Mathf.Pow(currentLevel, scalingPower));

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>Ajoute de l'XP et déclenche un level up si palier atteint.</summary>
    public void AddXP(float amount)
    {
        currentXP += amount;
        float required = XPRequired;

        while (currentXP >= required)
        {
            currentXP -= required;
            currentLevel++;
            Debug.Log($"[Arena] Level UP ! Niveau {currentLevel}");
            OnLevelUp?.Invoke(currentLevel);
            if (levelUpScreen != null)
            {
                var applier = FindFirstObjectByType<BonusApplier>();
                levelUpScreen.Show(currentLevel, bonus => applier?.Apply(bonus));
            }
            required = XPRequired; // recalcule pour le prochain palier
        }

        OnXPChanged?.Invoke(currentXP, XPRequired);
    }
}
