using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum SpecialAbilities
{
    None, Ability1, Ability2
}

[CreateAssetMenu(fileName = "NewCard", menuName = "ScriptableObjects/Card")]
public class CardScriptable : SerializedScriptableObject
{
    public enum CardType
    {
        Character, Spell
    }
    
    public enum SpecialAbilities
    {
        None, Ability1, Ability2
    }

    // STRING
    [Header("Card Information")]
    public string cardName;
    public string cardDescription;
    [Space]

    // SPECS
    [Header("General Specs")]
    public int initialManaCost;
    public CardType currentType;
    public SpecialAbilities currentAbilty;

    [Space]

    // CHARACTER STATS
    [Header("Character Stats")]
    [SerializeField] private AttackScriptable _attackScriptable;
    public AttackScriptable AttackScriptable => _attackScriptable;
    public int initialHealth;
    
    [Header("Effects")]
    [SerializeField] public List<Effect> EffectsOnInvoke = new List<Effect>();
    [SerializeField] public List<Effect> EffectsOnNewTurn = new List<Effect>();
    [SerializeField] public List<Effect> EffectsPassive = new List<Effect>();
}
