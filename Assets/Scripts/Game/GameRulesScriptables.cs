using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Rules", menuName = "ScriptableObjects/Game Rules", order = 1)]
public class GameRulesScriptables : ScriptableObject
{
    [Header("Player settings")] 
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _maxMana;
    [SerializeField] private int _initialMana;
    [SerializeField] private int _maxCardInHand;
    
    [Header("Action mana cost")]
    [SerializeField] private int _cardSwapManaCost;
    [SerializeField] private int _cardMoveManaCost;
    
    public int MaxHealth => _maxHealth;
    public int MaxMana => _maxMana;
    public int InitialMana => _initialMana;
    public int MaxCardInHand => _maxCardInHand;
    public int CardSwapManaCost => _cardSwapManaCost;
    public int CardMoveManaCost => _cardMoveManaCost;
}
