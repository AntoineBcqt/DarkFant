using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BonusDatabase
{
    private static readonly List<BonusData> _all = new()
    {
        // ── COMMUNS ───────────────────────────────────────────────────────────
        new BonusData { id="dmg_sword",   name="Lame Aiguisée",   description="+20% dégâts épée",            rarity=Rarity.Commun,     bonusType=BonusType.Stat,  value=0.20f },
        new BonusData { id="speed",       name="Agilité",         description="+8% vitesse de déplacement",  rarity=Rarity.Commun,     bonusType=BonusType.Stat,  value=0.08f },
        new BonusData { id="hp_up",       name="Vitalité",        description="+25 HP max",                  rarity=Rarity.Commun,     bonusType=BonusType.Stat,  value=25f   },
        new BonusData { id="atk_speed",   name="Frappe Rapide",   description="-15% cooldown épée",          rarity=Rarity.Commun,     bonusType=BonusType.Stat,  value=0.15f },

        // ── RARES ─────────────────────────────────────────────────────────────
        new BonusData { id="dash_cd",     name="Dash Rapide",     description="-20% cooldown dash",          rarity=Rarity.Rare,       bonusType=BonusType.Stat,  value=0.20f },
        new BonusData { id="dmg_proj",    name="Tir Occulte+",    description="+35% dégâts projectile",      rarity=Rarity.Rare,       bonusType=BonusType.Stat,  value=0.35f },
        new BonusData { id="hp_regen",    name="Régénération",    description="+1 HP toutes les 3 secondes", rarity=Rarity.Rare,       bonusType=BonusType.Stat,  value=1f,   maxStacks=3 },
        new BonusData { id="multi_proj",  name="Tir Double",      description="Tire 2 projectiles",          rarity=Rarity.Rare,       bonusType=BonusType.Skill, value=2f,   maxStacks=1 },

        // ── SUPER RARES ───────────────────────────────────────────────────────
        new BonusData { id="fan_shot",    name="Tir en Éventail", description="Tire 3 projectiles en arc",   rarity=Rarity.SuperRare,  bonusType=BonusType.Skill, value=3f,   maxStacks=1 },
        new BonusData { id="dash_dmg",    name="Dash Offensif",   description="Dash blesse les ennemis",     rarity=Rarity.SuperRare,  bonusType=BonusType.Skill, value=15f,  maxStacks=2 },
        new BonusData { id="lifesteal",   name="Vol de Vie",      description="15% des dégâts épée soignent",rarity=Rarity.SuperRare,  bonusType=BonusType.Skill, value=0.15f,maxStacks=2 },

        // ── ÉPIQUES ───────────────────────────────────────────────────────────
        new BonusData { id="aura_dmg",    name="Aura Sombre",     description="Aura de dégâts autour de toi",rarity=Rarity.Epique,     bonusType=BonusType.Skill, value=6f,   maxStacks=3 },
        new BonusData { id="sword_spin",  name="Épée Tournante",  description="L'épée orbite autour de toi", rarity=Rarity.Epique,     bonusType=BonusType.Skill, value=10f,  maxStacks=3 },
        new BonusData { id="ghost_dash",  name="Dash Fantôme",    description="Dash traverse + blesse",      rarity=Rarity.Epique,     bonusType=BonusType.Skill, value=25f,  maxStacks=1 },

        // ── LÉGENDAIRES ───────────────────────────────────────────────────────
        new BonusData { id="berserker",   name="Berserker",       description="Dégâts +50% sous 30% HP",     rarity=Rarity.Legendaire, bonusType=BonusType.Skill, value=0.50f,maxStacks=2 },
        new BonusData { id="time_slow",   name="Distorsion",      description="Ralentit les ennemis proches",rarity=Rarity.Legendaire, bonusType=BonusType.Skill, value=0.40f,maxStacks=1 },

        // ── OCCULTES ──────────────────────────────────────────────────────────
        new BonusData { id="blood_moon",  name="Lune de Sang",    description="Dégâts x2, HP divisés par 2", rarity=Rarity.Occulte,    bonusType=BonusType.Skill, value=2f,   maxStacks=1 },
        new BonusData { id="void_step",   name="Pas du Néant",    description="Invincible pendant le dash",  rarity=Rarity.Occulte,    bonusType=BonusType.Skill, value=1f,   maxStacks=1 },
    };

    // Probabilités par rareté (sur 100)
    private static readonly Dictionary<Rarity, int> _weights = new()
    {
        { Rarity.Commun,     60 },
        { Rarity.Rare,       25 },
        { Rarity.SuperRare,  10 },
        { Rarity.Epique,      0 }, // inclus dans SuperRare pool pour simplifier
        { Rarity.Legendaire,  4 },
        { Rarity.Occulte,     1 },
    };

    public static List<BonusData> DrawThree(Dictionary<string, int> currentStacks)
    {
        var result = new List<BonusData>();
        var usedIds = new HashSet<string>();
        int attempts = 0;

        while (result.Count < 3 && attempts < 300)
        {
            attempts++;
            var rarity = DrawRarity();
            // Épique se tire dans SuperRare pool
            var pool = _all.Where(b => b.rarity == rarity || (rarity == Rarity.SuperRare && b.rarity == Rarity.Epique)).ToList();
            if (pool.Count == 0) continue;

            var bonus = pool[Random.Range(0, pool.Count)];
            if (usedIds.Contains(bonus.id)) continue;

            // Vérif stack max — exclure si déjà au max
            if (currentStacks.TryGetValue(bonus.id, out int stacks) && stacks >= bonus.maxStacks) continue;

            result.Add(bonus);
            usedIds.Add(bonus.id);
        }

        return result;
    }

    private static Rarity DrawRarity()
    {
        int total = _weights.Values.Sum();
        int roll = Random.Range(0, total);
        int acc = 0;
        foreach (var kv in _weights)
        {
            acc += kv.Value;
            if (roll < acc) return kv.Key;
        }
        return Rarity.Commun;
    }
}