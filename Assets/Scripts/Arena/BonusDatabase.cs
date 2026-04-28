using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Base de données de tous les bonus disponibles en Arena.
/// </summary>
public static class BonusDatabase
{
    private static readonly List<BonusData> _all = new()
    {
        // ── COMMUNS ────────────────────────────────────────────────────────────
        new BonusData { id="dmg_sword",    name="Lame Aiguisée",     description="+20% dégâts épée",          rarity=Rarity.Commun,     bonusType=BonusType.Stat,  value=0.20f },
        new BonusData { id="speed",        name="Agilité",           description="+15% vitesse de déplacement",rarity=Rarity.Commun,     bonusType=BonusType.Stat,  value=0.15f },
        new BonusData { id="hp_up",        name="Vitalité",          description="+25 HP max",                 rarity=Rarity.Commun,     bonusType=BonusType.Stat,  value=25f   },
        new BonusData { id="dash_cd",      name="Dash Rapide",       description="-20% cooldown dash",         rarity=Rarity.Commun,     bonusType=BonusType.Stat,  value=0.20f },

        // ── RARES ──────────────────────────────────────────────────────────────
        new BonusData { id="dmg_proj",     name="Tir Occulte+",      description="+35% dégâts projectile",    rarity=Rarity.Rare,       bonusType=BonusType.Stat,  value=0.35f },
        new BonusData { id="hp_regen",     name="Régénération",      description="+2 HP/sec",                  rarity=Rarity.Rare,       bonusType=BonusType.Stat,  value=2f    },
        new BonusData { id="multi_proj",   name="Tir Double",        description="Tire 2 projectiles",         rarity=Rarity.Rare,       bonusType=BonusType.Skill, value=2f    },

        // ── SUPER RARES ────────────────────────────────────────────────────────
        new BonusData { id="fan_shot",     name="Tir en Éventail",   description="Tire 3 projectiles en arc",  rarity=Rarity.SuperRare,  bonusType=BonusType.Skill, value=3f    },
        new BonusData { id="dash_dmg",     name="Dash Offensif",     description="Dash blesse les ennemis",    rarity=Rarity.SuperRare,  bonusType=BonusType.Skill, value=15f   },
        new BonusData { id="lifesteal",    name="Vol de Vie",        description="+15% des dégâts en HP",      rarity=Rarity.SuperRare,  bonusType=BonusType.Skill, value=0.15f },

        // ── ÉPIQUES ────────────────────────────────────────────────────────────
        new BonusData { id="aura_dmg",     name="Aura Sombre",       description="Aura de dégâts autour de toi",rarity=Rarity.Epique,    bonusType=BonusType.Skill, value=8f    },
        new BonusData { id="sword_spin",   name="Épée Tournante",    description="L'épée orbite autour de toi", rarity=Rarity.Epique,    bonusType=BonusType.Skill, value=12f   },
        new BonusData { id="ghost_dash",   name="Dash Fantôme",      description="Dash traverse les ennemis + dégâts", rarity=Rarity.Epique, bonusType=BonusType.Skill, value=25f },

        // ── LÉGENDAIRES ────────────────────────────────────────────────────────
        new BonusData { id="berserker",    name="Berserker",         description="Dégâts +50% sous 30% HP",    rarity=Rarity.Legendaire, bonusType=BonusType.Skill, value=0.50f },
        new BonusData { id="time_slow",    name="Distorsion",        description="Ralentit les ennemis proches",rarity=Rarity.Legendaire, bonusType=BonusType.Skill, value=0.40f },

        // ── OCCULTES ───────────────────────────────────────────────────────────
        new BonusData { id="blood_moon",   name="Lune de Sang",      description="Dégâts x2, HP divisés par 2",rarity=Rarity.Occulte,   bonusType=BonusType.Skill, value=2f    },
        new BonusData { id="void_step",    name="Pas du Néant",      description="Invincible pendant le dash", rarity=Rarity.Occulte,   bonusType=BonusType.Skill, value=1f    },
    };

    // Probabilités par rareté (sur 1000)
    private static readonly Dictionary<Rarity, int> _weights = new()
    {
        { Rarity.Commun,     400 },
        { Rarity.Rare,       280 },
        { Rarity.SuperRare,  160 },
        { Rarity.Epique,      90 },
        { Rarity.Legendaire,  50 },
        { Rarity.Occulte,     20 },
    };

    /// <summary>Tire 3 bonus aléatoires (raretés aléatoires, pas de doublon de id sauf si stackable).</summary>
    public static List<BonusData> DrawThree(Dictionary<string, int> currentStacks)
    {
        var result = new List<BonusData>();
        var usedIds = new HashSet<string>();

        int attempts = 0;
        while (result.Count < 3 && attempts < 200)
        {
            attempts++;
            var rarity  = DrawRarity();
            var pool    = _all.Where(b => b.rarity == rarity).ToList();
            if (pool.Count == 0) continue;

            var bonus = pool[Random.Range(0, pool.Count)];
            if (usedIds.Contains(bonus.id)) continue;

            // Vérif stack max
            if (currentStacks.TryGetValue(bonus.id, out int stacks) && stacks >= bonus.maxStacks) continue;

            result.Add(bonus);
            usedIds.Add(bonus.id);
        }

        return result;
    }

    private static Rarity DrawRarity()
    {
        int total = _weights.Values.Sum();
        int roll  = Random.Range(0, total);
        int acc   = 0;
        foreach (var kv in _weights)
        {
            acc += kv.Value;
            if (roll < acc) return kv.Key;
        }
        return Rarity.Commun;
    }
}
