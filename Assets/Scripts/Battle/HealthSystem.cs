using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("Stats")]
    public string unitName = "Unit";
    public int maxHP = 100;
    public int currentHP;

    public event Action<int, int> OnHPChanged; // (current, max)
    public event Action OnDeath;

    public bool IsDead => currentHP <= 0;
    public float Ratio => (float)currentHP / maxHP;

    void Awake() => currentHP = maxHP;

    public int TakeDamage(int amount)
    {
        int actual = Mathf.Min(amount, currentHP);
        currentHP -= actual;
        OnHPChanged?.Invoke(currentHP, maxHP);
        if (currentHP <= 0) OnDeath?.Invoke();
        return actual;
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        OnHPChanged?.Invoke(currentHP, maxHP);
    }
}
