using System;
using System.Collections.Generic;
using UnityEngine;

public enum EBoardLineType
{
    Front,
    Back
}

public class Player : MonoBehaviour
{
    // Settings
    [Header("Settings")]
    [SerializeField] protected DeckScriptable _deckScriptable;
    [SerializeField] protected GameRulesScriptables _gameRules;
    [SerializeField] protected EPlayerType _playerType;
    
    // Board objects
    [Header("Object on board")]
    [SerializeField] protected GameObject _deckObject;
    [SerializeField] protected GameObject _discardObject;
    [SerializeField] protected GameObject _selectionObject;
    [SerializeField] protected GameObject _cardParent;
    [SerializeField] protected List<GameObject> _handSlots;
    [SerializeField] protected List<SlotController> _boardSlots;
    
    // Deck
    protected Queue<CardController> _cardsInDeck;
    protected List<CardController> _cardsInHand;
    protected List<CardController> _cardsOnBoard;
    protected List<CardController> _cardsDiscarded;
    
    
    // Health
    private int _currentHealth;
    public int CurrentHealth => _currentHealth;

    public Action<int, int> OnHealthChanged;
    
    // Mana
    private int _previousManaGain;
    protected int _currentMana;
    public Action<int, int> OnManaChanged;
    
    // Other player
    private Player _otherPlayer;

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
        _cardsDiscarded = new List<CardController>();

        _currentHealth = _gameRules.MaxHealth;
        _currentMana = _gameRules.InitialMana;
        _previousManaGain = _currentMana;

        foreach (var player in FindObjectsOfType<Player>())
        {
            if(player == this) continue;

            _otherPlayer = player;
        }

        for (var i = 0; i < _boardSlots.Count; i++)
        {
            _boardSlots[i].Setup(i);
        }
    }

    private void Init()
    {
        _cardsInDeck = new Queue<CardController>();

        _deckScriptable.FillList(ref _cardsInDeck, _deckObject.transform, _discardObject.transform, _selectionObject.transform);

        foreach (var cardController in _cardsInDeck)
        {
            cardController.transform.parent = _cardParent.transform;
        }
        
        FillHand();

        OnHealthChanged(_currentHealth, _gameRules.MaxHealth);
        OnManaChanged(_currentMana, _gameRules.MaxMana);
    }

    protected void ResetCardStartTurn()
    {
        foreach (var cardController in _cardsOnBoard)
        {
            cardController.ResetStartTurn();
        }
    }

    protected void AddManaStartTurn()
    {
        _currentMana = _previousManaGain;
        
        OnManaChanged(_currentMana, _gameRules.MaxMana);
        
        if (_previousManaGain < _gameRules.MaxMana)
        {
            _previousManaGain++;
        }
    }

    protected void FillHand()
    {
        var cardToAdd = _gameRules.MaxCardInHand - _cardsInHand.Count;

        for (int i = 0; i < cardToAdd; i++)
        {
            var card = _cardsInDeck.Dequeue();
            card.SetHandSlot(_handSlots[i].transform);
            card.SetCardState(CardController.CardState.inHand);
            _cardsInHand.Add(card);
        }
    }

    private void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        OnHealthChanged(_currentHealth, _gameRules.MaxHealth);
        
        if(_currentHealth <= 0)
        {
            GameManager.Instance.PlayerDeath(_playerType);
        }
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
    
    protected void MoveCardOnBoard(CardController cardToMove, SlotController slot)
    {
        cardToMove.UpdatePreviousSlot(slot);

        cardToMove.SetCardState(CardController.CardState.onDesk);
        cardToMove.PlayAnimationCard("IdleAnim");

        UseMana(_gameRules.CardMoveManaCost);
    }

    protected void DropCardOnBoard(CardController cardToMove, SlotController slot)
    {
        cardToMove.UpdatePreviousSlot(slot);

        cardToMove.SetCardState(CardController.CardState.onDesk);
        cardToMove.PlayAnimationCard("IdleAnim");
        
        _cardsInHand.Remove(cardToMove);
        _cardsOnBoard.Add(cardToMove);

        UseMana(cardToMove.cardManaCost);
    }

    protected void SwapCardOnBoard(CardController card1, CardController card2)
    {
        card1.UpdatePreviousSlot(card2.slotController);
        card2.UpdatePreviousSlot(card1.previousSlotController);
        card2.moveJumpHeight = 0.15f;
        card1.moveJumpHeight = 0.5f;
        card1.SetCardState(CardController.CardState.onDesk);
        card1.PlayAnimationCard("IdleAnim");

        card2.SetCardState(CardController.CardState.onDesk);
        
        UseMana(_gameRules.CardSwapManaCost);
    }

    protected void SetCardWaiting(CardController cardController)
    {
        cardController.SetCardState(CardController.CardState.isWaiting);
        cardController.PlayAnimationCard("ActiveAnim");
        currentHandState = HandState.CardSelectedInHand;

        cardController.sortingGroup.sortingOrder = 999;
    }

    protected void UseMana(int manaCost)
    {
        _currentMana -= manaCost;
        OnManaChanged(_currentMana, _gameRules.MaxMana);
    }

    protected void AttackOtherPlayer(CardController attackingCard)
    {
        attackingCard.Attack();
        attackingCard.SetCardState(CardController.CardState.onDesk);
        
        _otherPlayer.TakeDamage(attackingCard.cardAttack);
    }

    protected void AttackOtherCard(CardController attackingCard, CardController defendingCard)
    {
        attackingCard.Attack();
        attackingCard.SetCardState(CardController.CardState.onDesk);
        
        attackingCard.CardTakeDamage(defendingCard.cardAttack);
        defendingCard.CardTakeDamage(attackingCard.cardAttack);

        if (attackingCard.cardHealth <= 0)
        {
            attackingCard.slotController.containCard = false;
            attackingCard.slotController.cardController = null;

            attackingCard.slotController = null;
            
            _cardsOnBoard.Remove(attackingCard);
            _cardsDiscarded.Add(attackingCard);
        }
        
        if (defendingCard.cardHealth <= 0)
        {
            defendingCard.slotController.containCard = false;
            defendingCard.slotController.cardController = null;

            defendingCard.slotController = null;
            
            _otherPlayer._cardsOnBoard.Remove(defendingCard);
            _otherPlayer._cardsDiscarded.Add(defendingCard);
        }
    }
    
    protected List<CardController> GetPossibleCardToAttack(CardController attackingCard)
    {
        // TODO handle all type of attack
        return GetColumnInFront(attackingCard);
    }
    
    #region Get Cards On Board
   
    protected List<CardController> GetLine(EBoardLineType boardLineType)
    {
        var result = new List<CardController>();
        switch (boardLineType)
        {
            case EBoardLineType.Front:
                for (int i = 0; i < 4; i++)
                {
                    if (_boardSlots[i].containCard)
                    {
                        result.Add(_boardSlots[i].cardController);
                    }
                }
                break;
            case EBoardLineType.Back:
                for (int i = 4; i < _boardSlots.Count; i++)
                {
                    if (_boardSlots[i].containCard)
                    {
                        result.Add(_boardSlots[i].cardController);
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(boardLineType), boardLineType, null);
        }

        return result;
    }

    protected List<CardController> GetColumn(int columnId)
    {
        var result = new List<CardController>();

        for (int i = 0; i < 2; i++)
        {
            var slot = _boardSlots[i * 4 + columnId];
            if (!slot.containCard) continue;
            result.Add(slot.cardController);
        }
        
        return result;
    }
    
    protected List<CardController> GetColumnInFront(CardController cardController)
    {
        return _otherPlayer.GetColumn(GetColumnIdOfOtherPlayer(cardController.slotController.columnID));
    }
    
    protected bool IsInFront(CardController cardController)
    {
        return cardController.slotController.boardLineType == EBoardLineType.Front;
    }
    
    protected bool IsInBack(CardController cardController)
    {
        return cardController.slotController.boardLineType == EBoardLineType.Back;
    }

    protected bool TryGetCardInFront(CardController cardController, out CardController result)
    {
        if (cardController.slotController.boardLineType == EBoardLineType.Back)
        {
            result = _boardSlots[cardController.slotController.columnID].cardController;
            return _boardSlots[cardController.slotController.columnID].containCard;
        }
        else
        {
            var cardsOnColum = _otherPlayer.GetColumn(GetColumnIdOfOtherPlayer(cardController.slotController.columnID));
            if (cardsOnColum.Count > 0)
            {
                foreach (var controller in cardsOnColum)
                {
                    if (controller.slotController.boardLineType == EBoardLineType.Front)
                    {
                        result = controller;
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }
    }
    
    protected bool TryGetCardInBack(CardController cardController, out CardController result)
    {
        if (cardController.slotController.boardLineType == EBoardLineType.Front)
        {
            result = _boardSlots[cardController.slotController.columnID + 4].cardController;
            return _boardSlots[cardController.slotController.columnID + 4].containCard;
        }
        
        result = null;
        return false;
    }

    private static int GetColumnIdOfOtherPlayer(int selfColumnID)
    {
        return Mathf.Abs(3 - selfColumnID);
    }

    protected List<CardController> GetAllNeighbors(CardController cardController)
    {
        var result = new List<CardController>();

        var columnID = cardController.slotController.columnID;

        if (cardController.slotController.boardLineType == EBoardLineType.Back)
        {
            if (TryGetCardInFront(cardController, out var neighbor))
            {
                result.Add(neighbor);
            }
        }
        else
        {
            if (TryGetCardInBack(cardController, out var neighbor))
            {
                result.Add(neighbor);
            }
        }

        if (columnID == 0)
        {
            result.AddRange(GetColumn(columnID + 1));    
        }
        else if (columnID == 3)
        {
            result.AddRange(GetColumn(columnID - 1));    
        }
        else
        {
            result.AddRange(GetColumn(columnID + 1));    
            result.AddRange(GetColumn(columnID - 1));    
        }
        
        return result;
    }

    protected List<CardController> GetCrossNeighbors(CardController cardController)
    {
        var result = new List<CardController>();

        var columnID = cardController.slotController.columnID;

        if (cardController.slotController.boardLineType == EBoardLineType.Back)
        {
            if (TryGetCardInFront(cardController, out var neighbor))
            {
                result.Add(neighbor);
            }
        }
        else
        {
            if (TryGetCardInBack(cardController, out var neighbor))
            {
                result.Add(neighbor);
            }
        }

        if (columnID == 0)
        {
            if (TryGetNeighbor(cardController, NeighborDirection.Right, out var rightCard))
            {
                result.Add(rightCard);
            }
        }
        else if (columnID == 3)
        {
            if (TryGetNeighbor(cardController, NeighborDirection.Left, out var leftCard))
            {
                result.Add(leftCard);
            }    
        }
        else
        {
            if (TryGetNeighbor(cardController, NeighborDirection.Left, out var leftCard))
            {
                result.Add(leftCard);
            }

            if (TryGetNeighbor(cardController, NeighborDirection.Right, out var rightCard))
            {
                result.Add(rightCard);
            }
        }
        
        return result;
    }

    private enum NeighborDirection
    {
        Left,
        Right
    }

    private bool TryGetNeighbor(CardController cardController, NeighborDirection direction, out CardController result)
    {
        var columnID = cardController.slotController.columnID;
        var slotID = cardController.slotController.slotID;
        
        switch (direction)
        {
            case NeighborDirection.Left:
                if (columnID == 0)
                {
                    result = null;
                    return false;
                }
                else
                {
                    var slot = _boardSlots[slotID - 1];
                    result = slot.cardController;
                    return slot.containCard;
                }
                break;
            case NeighborDirection.Right:
                if (columnID >= 4)
                {
                    result = null;
                    return false;
                }
                else
                {
                    var slot = _boardSlots[slotID + 1];
                    result = slot.cardController;
                    return slot.containCard;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }
    #endregion
}
