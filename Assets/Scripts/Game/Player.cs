using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Settings
    [Header("Settings")]
    [SerializeField] protected DeckScriptable _deckScriptable;
    [SerializeField] protected int _maxNumberOfCardInHand;
    [SerializeField] protected int _maxHealthPoint = 30;
    [SerializeField] protected int _maxManaPoint = 10;
    [SerializeField] protected int _initialManaPoint = 3; 
    
    // Board objects
    [Header("Object on board")]
    [SerializeField] protected GameObject _deckObject;
    [SerializeField] protected GameObject _discardObject;
    [SerializeField] protected GameObject _cardParent;
    [SerializeField] protected List<GameObject> _handSlots;
    
    // Deck
    protected Queue<CardController> _cardsInDeck;
    protected List<CardController> _cardsInHand;
    
    // Finish turn 
    protected bool _hasFinishedTurn;
    
    // Health
    private int _currentHealth;
    public Action<int, int> OnHealthChanged;
    
    // Mana
    private int _previousManaGain;
    private int _currentMana;
    public Action<int, int> OnManaChanged;

    public bool HasFinishedTurn => _hasFinishedTurn;

    public enum HandState
    {
        free, 
        cardSelectedOnBoard, 
        cardSelectedInHand, 
        waitingTurn
    }
    public HandState currentHandState = HandState.free;

    private void Awake()
    {
        GameManager.Instance.onGameInit += Init;

        _cardsInHand = new List<CardController>();

        _currentHealth = _maxHealthPoint;
        _currentMana = _currentMana;
    }

    private void Init()
    {
        _cardsInDeck = new Queue<CardController>();

        _deckScriptable.FillList(ref _cardsInDeck, _deckObject.transform);

        foreach (var cardController in _cardsInDeck)
        {
            cardController.transform.parent = _cardParent.transform;
        }
        
        FillHand();

        OnHealthChanged(_currentHealth, _maxHealthPoint);
        OnManaChanged(_currentMana, _maxManaPoint);
    }

    protected void AddManaStartTurn()
    {
        if (_previousManaGain < _maxManaPoint)
        {
            _previousManaGain++;
        }

        _currentMana += _previousManaGain;
        _currentMana = Mathf.Min(_currentMana, _maxManaPoint);
        
        OnManaChanged(_currentMana, _maxManaPoint);
    }

    protected void FillHand()
    {
        var cardToAdd = _maxNumberOfCardInHand - _cardsInHand.Count;

        for (int i = 0; i < cardToAdd; i++)
        {
            var card = _cardsInDeck.Dequeue();
            card.SetHandSlot(_handSlots[i].transform);
            card.CardStateSwitch(CardController.CardState.inHand);
            _cardsInHand.Add(card);
        }
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
    }
}
