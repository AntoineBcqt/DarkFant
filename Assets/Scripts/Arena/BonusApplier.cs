using UnityEngine;

/// <summary>
/// Applique les bonus choisis sur le joueur.
/// Attaché au même GameObject que PlayerCombat.
/// </summary>
public class BonusApplier : MonoBehaviour
{
    private PlayerCombat _pc;

    private void Awake() => _pc = GetComponent<PlayerCombat>();

    public void Apply(BonusData bonus)
    {
        Debug.Log($"[Arena] ✦ Bonus choisi : {bonus.name} ({bonus.id}) rareté:{bonus.rarity}");
        switch (bonus.id)
        {
            case "dmg_sword": _pc.swordDamage *= (1f + bonus.value); Debug.Log($"[Arena] swordDamage → {_pc.swordDamage:F1}"); break;
            case "speed": _pc.moveSpeed *= (1f + bonus.value); Debug.Log($"[Arena] moveSpeed → {_pc.moveSpeed:F1}"); break;
            case "hp_up":
                _pc.maxHP += bonus.value;
                _pc.CurrentHP = Mathf.Min(_pc.CurrentHP + bonus.value, _pc.maxHP); Debug.Log($"[Arena] maxHP → {_pc.maxHP:F1}"); break;
            case "dash_cd": _pc.dashCooldown *= (1f - bonus.value); Debug.Log($"[Arena] dashCooldown → {_pc.dashCooldown:F2}"); break;
            case "dmg_proj": _pc.projectileDamage *= (1f + bonus.value); Debug.Log($"[Arena] projectileDamage → {_pc.projectileDamage:F1}"); break;
            case "hp_regen": _pc.hpRegenPerSec += bonus.value; Debug.Log($"[Arena] hpRegen → {_pc.hpRegenPerSec:F1}/s"); break;
            case "multi_proj": _pc.projectileCount += (int)bonus.value - 1; break;
            case "fan_shot":
                _pc.fanShot = true;
                _pc.projectileCount = (int)bonus.value; break;
            case "dash_dmg": _pc.dashDamage = bonus.value; break;
            case "lifesteal": _pc.lifeStealRatio = bonus.value; break;
            case "aura_dmg": _pc.auraDamage = bonus.value; break;
            case "berserker": _pc.berserkerBonus = bonus.value; break;
            case "blood_moon":
                _pc.swordDamage *= bonus.value;
                _pc.projectileDamage *= bonus.value;
                _pc.maxHP /= 2f;
                _pc.CurrentHP = Mathf.Min(_pc.CurrentHP, _pc.maxHP); break;
            case "void_step": _pc.dashAlwaysInvincible = true; Debug.Log("[Arena] dashAlwaysInvincible activé"); break;
            case "time_slow": Debug.Log("[Arena] Distorsion — pas encore implémenté en gameplay"); break;
            case "sword_spin": Debug.Log("[Arena] Épée Tournante — pas encore implémenté en gameplay"); break;
            case "ghost_dash": Debug.Log("[Arena] Dash Fantôme — pas encore implémenté en gameplay"); break;
            default: Debug.LogWarning($"[Arena] Bonus id '{bonus.id}' non géré dans BonusApplier !"); break;
        }
    }
}