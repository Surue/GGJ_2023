using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


[CreateAssetMenu(fileName = "NewCard", menuName = "ScriptableObjects/Card")]
public class CardScriptable : SerializedScriptableObject
{
    // STRING
    [Header("Card Information")]
    public string cardName;
    [Multiline]
    public string cardDescription;

    [Space]

    // SPECS
    [Header("General Specs")]
    public int initialManaCost;

    [Space]
    // CHARACTER STATS
    [Header("Character Stats")]
    [SerializeField] private AttackScriptable _attackScriptable;
    public AttackScriptable AttackScriptable => _attackScriptable;
    [SerializeField] private MovementDescriptionScriptable _movementDescriptionScriptable;
    public MovementDescriptionScriptable MovementDescriptionScriptable => _movementDescriptionScriptable;
    public int initialHealth;
    
    [Header("Effects")]
    [SerializeField] public List<Effect> EffectsOnInvoke = new List<Effect>();
    [SerializeField] public List<Effect> EffectsOnNewTurn = new List<Effect>();
    [SerializeField] public List<Effect> EffectsPassive = new List<Effect>();
}
