using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Settings
    [Header("Settings")]
    [SerializeField] protected DeckScriptable _deckScriptable;
    [SerializeField] protected int _maxNumberOfCardInHand;
    
    // Board objects
    [Header("Object on board")]
    [SerializeField] protected GameObject _deckObject;
    [SerializeField] protected GameObject _discardObject;
    [SerializeField] protected GameObject _cardParent;
    
    // Deck
    protected List<CardController> _cardsInDeck;

    private void Awake()
    {
        GameManager.Instance.onGameInit += Init;
    }

    private void Init()
    {
        _cardsInDeck = new List<CardController>();

        _deckScriptable.FillList(_cardsInDeck, _deckObject.transform);

        foreach (var cardController in _cardsInDeck)
        {
            cardController.transform.parent = _cardParent.transform;
        }
    }
}
