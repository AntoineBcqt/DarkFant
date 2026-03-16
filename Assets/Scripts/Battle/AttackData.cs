using UnityEngine;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Battle/Attack")]
public class AttackData : ScriptableObject
{
    public string attackName = "Attack";
    [TextArea] public string description = "";
    public int damage = 10;
    public int mpCost = 0;
}
