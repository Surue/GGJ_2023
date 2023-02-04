using UnityEngine;

public enum EAttackType
{
    Front,
    FrontAndBack,
    FrontLine,
    NoAttack
}

[CreateAssetMenu(fileName = "NewAttack", menuName = "ScriptableObjects/Attack")]
public class AttackScriptable : ScriptableObject
{
    [SerializeField] private EAttackType _attackType;
    [SerializeField] private int _attackDamage;
    [SerializeField] private int _attackCharge;
    
    public EAttackType AttackType => _attackType;
    public int AttackDamage => _attackDamage;
    public int AttackCharge => _attackCharge;
}
