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
    public int maximumMana;
    public int health;

    public int minimumCardInHand;
    public int turn;

    public int cardMoveCost;
    public int cardSwapCost;

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
}