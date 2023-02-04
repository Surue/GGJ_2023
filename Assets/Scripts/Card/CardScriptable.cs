using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "New_Character_Card", menuName = "Cards/Character")]
public class CardScriptable : ScriptableObject
{
    public enum CardType
    {
        Character, Spell
    }
    public enum AttackType
    {
        Distance, Melee
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
    public AttackType attackType;
    public Sprite attackDistanceSprite;
    public Sprite attackMeleeSprite;
    public int initialHealth;

    public int initialAttack;
    
    // // ILLUSTRATION
    // [Header("Card Illustration")]
    // public Sprite characterSprite;
    // public Sprite backgroundElement;
    // public Color backgroundColor;
    // [Space]
    //
    // // CARD ILLUSTRATION
    // [Header("Card Aspect")]
    // public Sprite cardSprite;
}
