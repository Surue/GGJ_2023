using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public Action<EPlayerType> onGameEnded;

    private EPlayerType _currentPlayerType;
    private Player _currentPlayer;
    public Player CurrentPlayer => _currentPlayer;
    private Player _enemyPlayer;
    public Player EnemyPlayer => _enemyPlayer;

    // Init state
    private bool _hasFinishedInit;
    private bool _isInitiatingGame;

    // Start turn 
    private bool _hasFinishedStartingTurn;
    private bool _isStartingTurn;

    // Wait Turn
    private bool _hasFinishedWaiting;
    
    // End
    private bool _aPlayerDied;
    private bool _hasFinishedGame;
    private EPlayerType _winningPlayer;
    private float _waitTimeBeforeSceneReload = 4.0f;

    // Players
    private HumanPlayer _humanPlayer => FindObjectOfType<HumanPlayer>();
    private CpuPlayer _cpuPlayer => FindObjectOfType<CpuPlayer>();

    public Player GetPlayer(EPlayerType playerType)
    {
        if (_humanPlayer == null) return null;
        return playerType == EPlayerType.Human ? (Player) _humanPlayer : _cpuPlayer;
    }
    
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

    private void OnDestroy()
    {
        _instance = null;
        _isInit = false;
    }

#if UNITY_EDITOR
    [InitializeOnEnterPlayMode]
    static void OnEnterPlaymodeInEditor(EnterPlayModeOptions options)
    {
        if (options.HasFlag(EnterPlayModeOptions.DisableDomainReload))
            _isInit = false;
    }
#endif
    

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
                    _currentPlayer = GetPlayer(EPlayerType.Human);
                    _enemyPlayer = GetPlayer(EPlayerType.CPU);
                    _hasFinishedStartingTurn = false;
                    _currentPlayerType = EPlayerType.Human;
                    StartTurn(EPlayerType.Human);
                    _isStartingTurn = true;
                }

                if (_hasFinishedStartingTurn)
                {
                    _gameState = EGameState.WaitHumanTurnEnd;
                    _hasFinishedStartingTurn = false;
                    _hasFinishedWaiting = false;
                    _isStartingTurn = false;
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
                    _enemyPlayer = GetPlayer(EPlayerType.Human);
                    _currentPlayer = GetPlayer(EPlayerType.CPU);
                    _hasFinishedStartingTurn = false;
                    _currentPlayerType = EPlayerType.CPU;
                    StartTurn(EPlayerType.CPU);
                    _isStartingTurn = true;
                }
                
                if (_hasFinishedStartingTurn)
                {
                    _gameState = EGameState.WaitCpuTurnEnd;
                    _hasFinishedStartingTurn = false;
                    _hasFinishedWaiting = false;
                    _isStartingTurn = false;
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
                EndTurn(_currentPlayerType);

                if (_aPlayerDied)
                {
                    _gameState = EGameState.EndGame;
                }
                else
                {
                    switch (_currentPlayerType)
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
                if (!_hasFinishedGame)
                {
                    _hasFinishedGame = true;
                    EndGame();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void InitGame()
    {
        onGameInit?.Invoke();

        _hasFinishedInit = true;
    }

    private void StartTurn(EPlayerType playerType)
    {
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
    }

    private void EndTurn(EPlayerType playerType)
    {
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

    public void PlayerDeath(EPlayerType playerType)
    {
        if (playerType == EPlayerType.Human)
        {
            _winningPlayer = EPlayerType.CPU;
        }
        else
        {
            _winningPlayer = EPlayerType.Human;
        }

        _aPlayerDied = true;
    }

    private void EndGame()
    {
        onGameEnded?.Invoke(_winningPlayer);

        StartCoroutine(WaitEndCoroutine());
    }

    private IEnumerator WaitEndCoroutine()
    {
        float timer = 0.0f;

        while (timer < _waitTimeBeforeSceneReload)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadSceneAsync("GameScene");
    }

    public void NextTurn()
    {
        _hasFinishedWaiting = true;
    }

    public void HasFinishedStartingTurn()
    {
        _hasFinishedStartingTurn = true;
    }
}
