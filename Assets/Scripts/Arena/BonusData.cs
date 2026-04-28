using UnityEngine;

public enum Rarity { Commun, Rare, SuperRare, Epique, Legendaire, Occulte }

public enum BonusType { Stat, Skill }

[System.Serializable]
public class BonusData
{
    public string    id;           // identifiant unique
    public string    name;         // nom affiché
    public string    description;  // description courte
    public Rarity    rarity;
    public BonusType bonusType;
    public Sprite    icon;         // sprite pixel art (optionnel)

    // Valeur du bonus (selon l'id, le PlayerCombat l'applique)
    public float value;
    public int   maxStacks = 3;

    // Couleurs par rareté
    public static Color GetColor(Rarity r) => r switch
    {
        Rarity.Commun     => new Color(0.55f, 0.85f, 0.45f),   // vert
        Rarity.Rare       => new Color(0.25f, 0.55f, 1.00f),   // bleu
        Rarity.SuperRare  => new Color(0.60f, 0.20f, 1.00f),   // violet
        Rarity.Epique     => new Color(0.60f, 0.20f, 1.00f),   // violet foncé
        Rarity.Legendaire => new Color(1.00f, 0.80f, 0.10f),   // jaune
        Rarity.Occulte    => new Color(0.80f, 0.05f, 0.20f),   // rouge-noir
        _                 => Color.white
    };

    public static Color GetBorderColor(Rarity r) => r switch
    {
        Rarity.Commun     => new Color(0.30f, 0.65f, 0.25f),
        Rarity.Rare       => new Color(0.10f, 0.35f, 0.85f),
        Rarity.SuperRare  => new Color(0.45f, 0.05f, 0.85f),
        Rarity.Epique     => new Color(0.35f, 0.00f, 0.70f),
        Rarity.Legendaire => new Color(0.85f, 0.60f, 0.00f),
        Rarity.Occulte    => new Color(0.50f, 0.00f, 0.10f),
        _                 => Color.grey
    };

    public static string GetRarityLabel(Rarity r) => r switch
    {
        Rarity.Commun     => "COMMUN",
        Rarity.Rare       => "RARE",
        Rarity.SuperRare  => "SUPER RARE",
        Rarity.Epique     => "ÉPIQUE",
        Rarity.Legendaire => "LÉGENDAIRE",
        Rarity.Occulte    => "✦ OCCULTE ✦",
        _                 => ""
    };
}
