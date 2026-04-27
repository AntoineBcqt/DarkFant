using UnityEngine;

public enum SkillType
{
    Attack,       // Dégâts sur l'ennemi
    Heal,         // Soins sur soi-même
    Buff,         // Augmente une stat temporairement
    Debuff,       // Réduit une stat ennemie
    LimitBreak,   // Compétence ultime
}

public enum DamageType
{
    Physical,
    Magical,
    True,         // Ignore la défense
}

/// <summary>
/// ScriptableObject définissant une compétence/attaque.
/// Créer via : clic droit → Create → DarkFant → Skill
/// </summary>
[CreateAssetMenu(fileName = "NewSkill", menuName = "DarkFant/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Identité")]
    public string skillName    = "Attaque";
    public Sprite icon;
    [TextArea] public string description = "";

    [Header("Type")]
    public SkillType  skillType  = SkillType.Attack;
    public DamageType damageType = DamageType.Physical;

    [Header("Coût")]
    public int mpCost = 0;

    [Header("Puissance")]
    [Tooltip("Multiplicateur appliqué à la stat d'attaque/soin de l'unité.")]
    public float power = 1.0f;

    [Tooltip("Valeur fixe ajoutée aux dégâts/soins (indépendante des stats).")]
    public int flatBonus = 0;

    [Header("Effets visuels")]
    public Color  skillColor   = Color.white;
    public string animTrigger  = "";   // Trigger Animator si dispo

    [Header("Dialogue")]
    [TextArea] public string useMessage = "";   // Message affiché quand utilisée

    // ── Calcul des dégâts/soins ──────────────────────────────────

    /// <summary>Calcule la valeur brute à partir de la stat d'attaque/magie de l'unité.</summary>
    public int Calculate(int statValue)
        => Mathf.RoundToInt(statValue * power) + flatBonus;
}
