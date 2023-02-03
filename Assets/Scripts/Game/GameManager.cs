using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EGameState
{
    Init,
    StartHumanTurn,
    WaitHumanTurnEnd,
    StartCpuTurn,
    WaitCpuTurnEnd,
    EndTurn,
    EndGame
}

public enum EPlayerType
{
    Human,
    CPU
}

public class GameManager : MonoBehaviour
{
    private EGameState _gameState = EGameState.Init;

    public Action onGameInit;
    public Action onHumanTurnStarted;
    public Action onHumanTurnFinished;
    public Action onCpuTurnStarted;
    public Action onCpuTurnFinished;
    public Action onGameEnded;

    private EPlayerType _currentPlayer;
    private bool _gameFinished;

    // Init state
    private bool _hasFinishedInit;
    private bool _isInitiatingGame;

    // Start turn 
    private bool _hasFinishedStartingTurn;
    private bool _isStartingTurn;

    // Wait Turn
    private bool _hasFinishedWaiting;

    // Singleton
    private static bool _isInit = false;
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (!_isInit)
            {
                _instance = FindObjectOfType<GameManager>();
                _isInit = true;
            }

            return _instance;
        }
    }

    private void Update()
    {
        switch (_gameState)
        {
            case EGameState.Init:
                if (!_isInitiatingGame)
                {
                    InitGame();
                    _isInitiatingGame = true;
                }

                if (_hasFinishedInit)
                {
                    _gameState = EGameState.StartHumanTurn;
                    _isInitiatingGame = false;
                }
                break;
            case EGameState.StartHumanTurn:
                if (!_isStartingTurn)
                {
                    _currentPlayer = EPlayerType.Human;
                    StartTurn(EPlayerType.Human);
                    _isStartingTurn = true;
                }

                if (_hasFinishedInit)
                {
                    _gameState = EGameState.WaitHumanTurnEnd;
                    _hasFinishedInit = false;
                    _hasFinishedWaiting = false;
                }
                break;
            case EGameState.WaitHumanTurnEnd:
                if (_hasFinishedWaiting)
                {
                    _gameState = EGameState.EndTurn;
                }
                else
                {
                    WaitTurnEnd(EPlayerType.Human);
                }

                break;
            case EGameState.StartCpuTurn:
                if (!_isStartingTurn)
                {
                    _currentPlayer = EPlayerType.CPU;
                    StartTurn(EPlayerType.CPU);
                    _isStartingTurn = true;
                }
                
                if (_hasFinishedInit)
                {
                    _gameState = EGameState.WaitHumanTurnEnd;
                    _hasFinishedInit = false;
                    _hasFinishedWaiting = false;
                }
                break;
            case EGameState.WaitCpuTurnEnd:
                if (_hasFinishedWaiting)
                {
                    _gameState = EGameState.EndTurn;
                }
                else
                {
                    WaitTurnEnd(EPlayerType.CPU);
                }
                break;
            case EGameState.EndTurn:
                EndTurn(_currentPlayer);

                if (_gameFinished)
                {
                    _gameState = EGameState.EndGame;
                }
                else
                {
                    switch (_currentPlayer)
                    {
                        case EPlayerType.Human:
                            _gameState = EGameState.StartCpuTurn;
                            break;
                        case EPlayerType.CPU:
                            _gameState = EGameState.StartHumanTurn;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                break;
            case EGameState.EndGame:
                EndGame();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void InitGame()
    {
        Debug.Log("[GameManager] InitGame");
        onGameInit?.Invoke();

        _hasFinishedInit = true;
    }

    private void StartTurn(EPlayerType playerType)
    {
        Debug.Log("[GameManager] StartTurn of " + playerType);

        switch (playerType)
        {
            case EPlayerType.Human:
                onHumanTurnStarted?.Invoke();
                break;
            case EPlayerType.CPU:
                onCpuTurnStarted?.Invoke();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerType), playerType, null);
        }
    }

    private void WaitTurnEnd(EPlayerType playerType)
    {
        Debug.Log("[GameManager] Wait en turn of " + playerType);
    }

    private void EndTurn(EPlayerType playerType)
    {
        Debug.Log("[GameManager] End Turn of " + playerType);

        switch (playerType)
        {
            case EPlayerType.Human:
                onHumanTurnFinished?.Invoke();
                break;
            case EPlayerType.CPU:
                onCpuTurnFinished?.Invoke();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerType), playerType, null);
        }
    }

    private void EndGame()
    {
        Debug.Log("[GameManager] EndGames");
        onGameEnded?.Invoke();
    }
}
