using System;
using System.Collections.Generic;
using UnityEngine;

public struct CardState
{
    // Initial values
    public int health;
    public int manaCost;
    
    // Attack
    public int attack;
    public int attackCharge;
    public EAttackType attackType;

    // Movement
    public int maxMovementLineCount;
    public int maxMovementColumnCount;
    public int freeMovementCount;
    public int turnMovementCount;
    
    // Index
    public const int NullCardIndex = -1;
    public int cardIndex;

    public bool HasFreeMovement()
    {
        return turnMovementCount < freeMovementCount;
    }
}

public struct DeckState
{
    public List<CardState> cardsState;
    
    public List<int> cardInHand;
    public List<int> cardOnBoard;
    public List<int> cardInDeck;
}

/**
 * 7 6 5 4 
 * 3 2 1 0
 * 
 * 0 1 2 3
 * 4 5 6 7
 */
public struct SlotState
{
    public int columnID;
    public int rowID;
    
    public int cardIndex;

    public bool HasCard()
    {
        return cardIndex != CardState.NullCardIndex;
    }
}

public struct BoardState
{
    public SlotState[] slots;
}

public struct PlayerState
{
    public DeckState deckState;
    public BoardState boardState;

    // Mana
    public int mana;
    public int manaNextTurn;
    public int maximumMana; // TODO Const
    public int health;

    public int minimumCardInHand; // TODO Const
    public int turn;

    public int cardMoveCost; // TODO Const
    public int cardSwapCost; // TODO Const

    public EPlayerType type;

    public void StartTurn()
    {
        // Increase mana
        mana = manaNextTurn;
        manaNextTurn = Mathf.Min(maximumMana, manaNextTurn + 1);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
    
    public bool CanDropCardOnBoard(int cardToDropManaCost)
    {
        return mana >= cardToDropManaCost;
    }
        
    public bool CanMoveCard()
    {
        return mana >= cardMoveCost;
    }
        
    public bool CanSwapCard()
    {
        return mana > cardSwapCost;
    }
}

public class GameState
{
    // Player
    public PlayerState humanPlayer;
    public PlayerState cpuPlayer;

    public void StartPlayerTurn(EPlayerType type)
    {
        switch (type)
        {
            case EPlayerType.Human:
                humanPlayer.StartTurn();
                break;
            case EPlayerType.CPU:
                cpuPlayer.StartTurn();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public int GetPlayerHealth(EPlayerType type)
    {
        switch (type)
        {
            case EPlayerType.Human:
                return humanPlayer.health;
            case EPlayerType.CPU:
                return cpuPlayer.health;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
    
    public int GetPlayerMana(EPlayerType type)
    {
        switch (type)
        {
            case EPlayerType.Human:
                return humanPlayer.mana;
            case EPlayerType.CPU:
                return cpuPlayer.mana;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public void PlayerUseMana(EPlayerType type, int manaUsed)
    {
        switch (type)
        {
            case EPlayerType.Human:
                humanPlayer.mana -= manaUsed;
                break;
            case EPlayerType.CPU: 
                cpuPlayer.mana -= manaUsed;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public void PlayerTakeDamage(EPlayerType type, int damage)
    {
        switch (type)
        {
            case EPlayerType.Human:
                humanPlayer.TakeDamage(damage);
                break;
            case EPlayerType.CPU:
                cpuPlayer.TakeDamage(damage);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public bool IsPlayerDead(EPlayerType type)
    {
        switch (type)
        {
            case EPlayerType.Human:
                return humanPlayer.health <= 0;
            case EPlayerType.CPU:
                return cpuPlayer.health <= 0;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
    
    public bool CanPlayerDropCardOnBoard(EPlayerType type, int cardToDropManaCost)
    {
        switch (type)
        {
            case EPlayerType.Human:
                return humanPlayer.CanDropCardOnBoard(cardToDropManaCost);
            case EPlayerType.CPU:
                return cpuPlayer.CanDropCardOnBoard(cardToDropManaCost);
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
        
    public bool CanPlayerMoveCard(EPlayerType type)
    {
        switch (type)
        {
            case EPlayerType.Human:
                return humanPlayer.CanMoveCard();
            case EPlayerType.CPU:
                return cpuPlayer.CanMoveCard();
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
        
    public bool CanPlayerSwapCard(EPlayerType type)
    {
        switch (type)
        {
            case EPlayerType.Human:
                return humanPlayer.CanSwapCard();
            case EPlayerType.CPU:
                return cpuPlayer.CanSwapCard();
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}