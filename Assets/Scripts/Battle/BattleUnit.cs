using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Représente un combattant dans le système de combat tour par tour.
/// Contient toutes les stats, la gestion HP/MP/Limit Break et les événements associés.
/// </summary>
public class BattleUnit : MonoBehaviour
{
    // ── Identité ─────────────────────────────────────────────────
    [Header("Identité")]
    public string unitName   = "Unit";
    public bool   isPlayer   = false;
    public Sprite portrait;

    // ── Stats de base ────────────────────────────────────────────
    [Header("Stats")]
    public int maxHP      = 100;
    public int maxMP      = 50;
    public int attack     = 15;
    public int defense    = 5;
    public int speed      = 10;   // détermine l'ordre d'initiative

    // ── Compétences ──────────────────────────────────────────────
    [Header("Compétences")]
    public List<SkillData> skills = new();

    // ── Limit Break ──────────────────────────────────────────────
    [Header("Limit Break")]
    public int    limitBreakMax  = 100;
    public SkillData limitBreakSkill;

    // ── Runtime ──────────────────────────────────────────────────
    public int   CurrentHP       { get; private set; }
    public int   CurrentMP       { get; private set; }
    public int   LimitBreak      { get; private set; }
    public bool  IsLimitReady    => LimitBreak >= limitBreakMax;
    public bool  IsDead          => CurrentHP <= 0;
    public float HPRatio         => (float)CurrentHP / maxHP;
    public float MPRatio         => (float)CurrentMP / maxMP;
    public float LimitRatio      => (float)LimitBreak / limitBreakMax;

    // Status effects
    public bool IsDefending      { get; private set; }

    // ── Events ───────────────────────────────────────────────────
    public event Action<int, int>   OnHPChanged;      // (current, max)
    public event Action<int, int>   OnMPChanged;      // (current, max)
    public event Action<float>      OnLimitChanged;   // (ratio 0-1)
    public event Action             OnDeath;
    public event Action             OnLimitReady;

    // ─────────────────────────────────────────────────────────────

    private void Awake() => ResetStats();

    public void ResetStats()
    {
        CurrentHP    = maxHP;
        CurrentMP    = maxMP;
        LimitBreak   = 0;
        IsDefending  = false;
    }

    // ── Dégâts ───────────────────────────────────────────────────

    /// <summary>Applique des dégâts en tenant compte de la défense et de l'état de garde.</summary>
    public int TakeDamage(int rawAmount, bool ignoreDefense = false)
    {
        int def    = ignoreDefense ? 0 : defense;
        int reduced = IsDefending ? def * 2 : def;
        int actual = Mathf.Max(1, rawAmount - reduced);
        actual     = Mathf.Min(actual, CurrentHP);

        CurrentHP -= actual;
        OnHPChanged?.Invoke(CurrentHP, maxHP);

        // Limit Break se charge en prenant des dégâts
        GainLimit(Mathf.RoundToInt(actual * 0.8f));

        if (CurrentHP <= 0) OnDeath?.Invoke();
        return actual;
    }

    /// <summary>Applique des dégâts directs sans calcul de défense.</summary>
    public int TakeDirectDamage(int amount)
    {
        int actual = Mathf.Min(amount, CurrentHP);
        CurrentHP -= actual;
        OnHPChanged?.Invoke(CurrentHP, maxHP);
        if (CurrentHP <= 0) OnDeath?.Invoke();
        return actual;
    }

    // ── Soins ────────────────────────────────────────────────────

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(CurrentHP + amount, maxHP);
        OnHPChanged?.Invoke(CurrentHP, maxHP);
    }

    // ── MP ───────────────────────────────────────────────────────

    public bool CanUseSkill(SkillData skill) => CurrentMP >= skill.mpCost;

    public void SpendMP(int amount)
    {
        CurrentMP = Mathf.Max(0, CurrentMP - amount);
        OnMPChanged?.Invoke(CurrentMP, maxMP);
    }

    public void RestoreMP(int amount)
    {
        CurrentMP = Mathf.Min(CurrentMP + amount, maxMP);
        OnMPChanged?.Invoke(CurrentMP, maxMP);
    }

    // ── Limit Break ──────────────────────────────────────────────

    public void GainLimit(int amount)
    {
        if (IsLimitReady) return;
        bool wasFull = IsLimitReady;
        LimitBreak = Mathf.Min(LimitBreak + amount, limitBreakMax);
        OnLimitChanged?.Invoke(LimitRatio);
        if (!wasFull && IsLimitReady) OnLimitReady?.Invoke();
    }

    public void ConsumeLimit()
    {
        LimitBreak = 0;
        OnLimitChanged?.Invoke(0f);
    }

    // ── Défense ──────────────────────────────────────────────────

    public void SetDefending(bool value) => IsDefending = value;

    // ── Initiative ───────────────────────────────────────────────

    /// <summary>
    /// Calcule la valeur d'initiative pour ordonner les tours.
    /// Ajoute un léger aléatoire pour éviter les ex-aequo parfaits.
    /// </summary>
    public float GetInitiative() => speed + UnityEngine.Random.Range(0f, 2f);
}
