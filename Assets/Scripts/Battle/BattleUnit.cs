using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    [Header("Stats")]
    public string unitName = "Unit";
    public int maxHP = 100;
    public int currentHP;
    public AttackData[] attacks;

    public bool IsDead => currentHP <= 0;

    void Awake()
    {
        currentHP = maxHP;
    }

    public int TakeDamage(int amount)
    {
        int actual = Mathf.Min(amount, currentHP);
        currentHP -= actual;
        return actual;
    }
}
