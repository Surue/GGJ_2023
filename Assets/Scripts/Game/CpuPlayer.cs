using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CpuPlayer : Player
{
    private bool _isPlaying;

    private enum ECpuPhase
    {
        InvokeCard,
        InvokingCard,
        Attack,
        Attacking
        ,
        End
    }

    private ECpuPhase _phase;

    private void Start()
    {
        GameManager.Instance.onCpuTurnStarted += StartTurn;
        GameManager.Instance.onCpuTurnStarted += AddManaStartTurn;
        GameManager.Instance.onCpuTurnStarted += FillHand;
        GameManager.Instance.onCpuTurnStarted += ResetCardStartTurn;
        GameManager.Instance.onCpuTurnFinished += EndTurn;

        _isPlaying = false;
    }

    private void Update()
    {
        if(!_isPlaying) return;

        switch (_phase)
        {
            case ECpuPhase.InvokeCard:
                StartCoroutine(InvokeCardsCoroutine());
                break;
            case ECpuPhase.InvokingCard:
                break;
            case ECpuPhase.Attack:
                StartCoroutine(AttackCoroutine());
                break;
            case ECpuPhase.Attacking:
                break;
            case ECpuPhase.End:
                EndTurn();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void StartTurn()
    {
        _isPlaying = true;

        _phase = ECpuPhase.InvokeCard;
        
        GameManager.Instance.HasFinishedStartingTurn();
    }

    private IEnumerator InvokeCardsCoroutine()
    {
        _phase = ECpuPhase.InvokingCard;
        
        var freeSlots = GetFreeSlots();
        var cardsToInvoke = GetCardsToInvoke(freeSlots.Count);

        foreach (var cardController in cardsToInvoke)
        {
            SetCardWaiting(cardController);

            while (cardController.isTweening)
            {
                yield return null;
            }

            var freeSlot = freeSlots[Random.Range(0, freeSlots.Count)];
            freeSlots.Remove(freeSlot);
            
            DropCardOnBoard(cardController, freeSlot);
            
            while (cardController.isTweening)
            {
                yield return null;
            }
        }

        _phase = ECpuPhase.Attack;
    }

    private IEnumerator AttackCoroutine()
    {
        _phase = ECpuPhase.Attacking;
        for (var i = _cardsOnBoard.Count - 1; i >= 0; i--)
        {
            var cardController = _cardsOnBoard[i];
            var cardsToAttack = GetPossibleCardToAttack(cardController);

            if (cardsToAttack.Count > 0)
            {
                if (cardsToAttack.Count == 1)
                {
                    AttackOtherCard(cardController, cardsToAttack[0]);
                }
                else
                {
                    if (cardsToAttack[0].boardController.boardLineType == EBoardLineType.Front)
                    {
                        AttackOtherCard(cardController, cardsToAttack[0]);
                    }
                    else
                    {
                        AttackOtherCard(cardController, cardsToAttack[1]);
                    }
                }
            }
            else
            {
                AttackOtherPlayer(cardController);
            }

            yield return null;
        }

        _phase = ECpuPhase.End;
    }
    
    private void EndTurn()
    {
        _isPlaying = false;
        
        GameManager.Instance.NextTurn();
    }

    #region AI

    private List<CardController> GetCardsToInvoke(int freeSlotCount)
    {
        var possibleCardToInvoke = new List<CardController>();
        if (freeSlotCount == 0) return possibleCardToInvoke;
        
        foreach (var cardController in _cardsInHand)
        {
            if (CanDropCardOnBoard(cardController))
            {
                possibleCardToInvoke.Add(cardController);
            }
        }

        possibleCardToInvoke = possibleCardToInvoke.OrderBy(x => x.cardAttack).Reverse().ToList();

        var cardToInvoke = new List<CardController>();
        
        var mana = _currentMana;

        foreach (var card in possibleCardToInvoke)
        {
            if (mana >= card.cardManaCost)
            {
                cardToInvoke.Add(card);
                mana -= card.cardManaCost;

                if (cardToInvoke.Count >= freeSlotCount)
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        return cardToInvoke;
    }

    private List<BoardController> GetFreeSlots()
    {
        var result = new List<BoardController>();
        foreach (var boardSlot in _boardSlots)
        {
            if (!boardSlot.containCard)
            {
                result.Add(boardSlot);
            }
        }

        return result;
    }

    #endregion
}
