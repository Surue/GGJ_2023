using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Settings
    [Header("Settings")]
    [SerializeField] protected DeckScriptable _deckScriptable;
    [SerializeField] protected GameRulesScriptables _gameRules;
    
    // Board objects
    [Header("Object on board")]
    [SerializeField] protected GameObject _deckObject;
    [SerializeField] protected GameObject _discardObject;
    [SerializeField] protected GameObject _cardParent;
    [SerializeField] protected List<GameObject> _handSlots;
    
    // Deck
    protected Queue<CardController> _cardsInDeck;
    protected List<CardController> _cardsInHand;
    protected List<CardController> _cardsOnBoard;
    
    // Finish turn 
    protected bool _hasFinishedTurn;
    
    // Health
    private int _currentHealth;
    public Action<int, int> OnHealthChanged;
    
    // Mana
    private int _previousManaGain;
    private int _currentMana;
    public Action<int, int> OnManaChanged;
    
    // Other player
    private Player _otherPlayer;
    
    public bool HasFinishedTurn => _hasFinishedTurn;

    public enum HandState
    {
        Free, 
        CardSelectedOnBoard, 
        CardSelectedInHand, 
        WaitingTurn
    }
    public HandState currentHandState = HandState.Free;

    private void Awake()
    {
        GameManager.Instance.onGameInit += Init;

        _cardsInHand = new List<CardController>();
        _cardsOnBoard = new List<CardController>();

        _currentHealth = _gameRules.MaxHealth;
        _currentMana = _gameRules.InitialMana;

        foreach (var player in FindObjectsOfType<Player>())
        {
            if(player == this) continue;

            _otherPlayer = player;
        }
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

        OnHealthChanged(_currentHealth, _gameRules.MaxHealth);
        OnManaChanged(_currentMana, _gameRules.MaxMana);
    }

    protected void AddManaStartTurn()
    {
        if (_previousManaGain < _gameRules.MaxMana)
        {
            _previousManaGain++;
        }

        _currentMana += _previousManaGain;
        _currentMana = Mathf.Min(_currentMana, _gameRules.MaxMana);
        
        OnManaChanged(_currentMana, _gameRules.MaxMana);
    }

    protected void FillHand()
    {
        var cardToAdd = _gameRules.MaxCardInHand - _cardsInHand.Count;

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
        OnHealthChanged(_currentHealth, _gameRules.MaxHealth);
    }

    protected bool CanSwapCards()
    {
        return _currentMana >= _gameRules.CardSwapManaCost;
    }
    
    protected bool CanDropCardOnBoard(CardController cardToDrop)
    {
        return _currentMana >= cardToDrop.cardManaCost;
    }

    protected bool CanMoveCardOnBoard()
    {
        return _currentMana >= _gameRules.CardMoveManaCost;
    }
    
    protected void MoveCardOnBoard(CardController cardToMove, BoardController slot)
    {
        cardToMove.UpdatePreviousSlot(slot);

        cardToMove.CardStateSwitch(CardController.CardState.onDesk);
        cardToMove.PlayAnimationCard("IdleAnim");

        UseMana(_gameRules.CardMoveManaCost);
    }

    protected void DropCardOnBoard(CardController cardToMove, BoardController slot)
    {
        cardToMove.UpdatePreviousSlot(slot);

        cardToMove.CardStateSwitch(CardController.CardState.onDesk);
        cardToMove.PlayAnimationCard("IdleAnim");
        
        _cardsInHand.Remove(cardToMove);
        _cardsOnBoard.Add(cardToMove);

        UseMana(cardToMove.cardManaCost);
    }

    protected void SwapCardOnBoard(CardController card1, CardController card2)
    {
        card1.UpdatePreviousSlot(card2.boardController);
        card2.UpdatePreviousSlot(card1.previousBoardController);
        card2.moveJumpHeight = 0.15f;
        card1.moveJumpHeight = 0.5f;
        card1.CardStateSwitch(CardController.CardState.onDesk);
        card1.PlayAnimationCard("IdleAnim");

        card2.CardStateSwitch(CardController.CardState.onDesk);
        
        UseMana(_gameRules.CardSwapManaCost);
    }

    protected void UseMana(int manaCost)
    {
        _currentMana -= manaCost;
        OnManaChanged(_currentMana, _gameRules.MaxMana);
    }

    protected void AttackOtherPlayer(CardController attackingCard)
    {
        attackingCard.Attack();
        
        _otherPlayer.TakeDamage(attackingCard.cardAttack);
    }

    protected void AttackOtherCard(CardController attackingCard, CardController defendingCard)
    {
        attackingCard.Attack();
    }
}
