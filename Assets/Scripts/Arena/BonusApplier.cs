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
        Debug.Log($"[Arena] Bonus appliqué : {bonus.name} ({bonus.id})");
        switch (bonus.id)
        {
            case "dmg_sword":   _pc.swordDamage    *= (1f + bonus.value); break;
            case "speed":       _pc.moveSpeed      *= (1f + bonus.value); break;
            case "hp_up":       _pc.maxHP          += bonus.value;
                                _pc.CurrentHP       = Mathf.Min(_pc.CurrentHP + bonus.value, _pc.maxHP); break;
            case "dash_cd":     _pc.dashCooldown   *= (1f - bonus.value); break;
            case "dmg_proj":    _pc.projectileDamage *= (1f + bonus.value); break;
            case "hp_regen":    _pc.hpRegenPerSec  += bonus.value; break;
            case "multi_proj":  _pc.projectileCount += (int)bonus.value - 1; break;
            case "fan_shot":    _pc.fanShot          = true;
                                _pc.projectileCount  = (int)bonus.value; break;
            case "dash_dmg":    _pc.dashDamage       = bonus.value; break;
            case "lifesteal":   _pc.lifeStealRatio   = bonus.value; break;
            case "aura_dmg":    _pc.auraDamage        = bonus.value; break;
            case "berserker":   _pc.berserkerBonus    = bonus.value; break;
            case "blood_moon":  _pc.swordDamage    *= bonus.value;
                                _pc.projectileDamage *= bonus.value;
                                _pc.maxHP           /= 2f;
                                _pc.CurrentHP        = Mathf.Min(_pc.CurrentHP, _pc.maxHP); break;
            case "void_step":   _pc.dashAlwaysInvincible = true; break;
        }
    }
}
