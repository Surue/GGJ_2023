using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [Header("Human")]
    [SerializeField] private Button _nextTurn;
    [SerializeField] private TMP_Text _manaTextPlayer;
    [SerializeField] private TMP_Text _healthTextPlayer;
    [Header("CPU")]
    [SerializeField] private TMP_Text _manaTextCpu;
    [SerializeField] private TMP_Text _healthTextCpu;

    [Header("Turn Transition")] 
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _textTurnTransition;
    [SerializeField] private Image _imageTurnTransition;
    [SerializeField] private float _timeFadeTurnTransition;
    [SerializeField] private float _timeOnScreenTurnTransition;

    private HumanPlayer _humanPlayer;
    private CpuPlayer _cpuPlayer;
    
    void Start()
    {
        var gameManager = GameManager.Instance;
        gameManager.onGameInit += OnGameInit;
    }

    private void OnGameInit()
    {
        var gameManager = GameManager.Instance;
        gameManager.onCpuTurnStarted += OnCpuStartTurn;
        gameManager.onHumanTurnStarted += OnHumanStartTurn;
        gameManager.onGameEnded += EndGame;
        
        _humanPlayer = FindObjectOfType<HumanPlayer>();
        
        gameManager.gameState.humanPlayer.OnHealthChanged += UpdateHealthHuman;
        gameManager.gameState.RegisterCallbackOnManaChange(EPlayerType.Human, UpdateManaHuman);
        
        _nextTurn.onClick.AddListener(_humanPlayer.NextTurn);

        _cpuPlayer = FindObjectOfType<CpuPlayer>();

        gameManager.gameState.cpuPlayer.OnHealthChanged += UpdateHealthCpu;
        gameManager.gameState.RegisterCallbackOnManaChange(EPlayerType.CPU, UpdateManaCpu);
    }

    private void UpdateHealthHuman(int newHealth)
    {
        _healthTextPlayer.SetText($"{newHealth}");  
    }

    private void UpdateManaHuman(int newMana)
    {
        _manaTextPlayer.SetText($"{newMana}");  
    }

    private void UpdateHealthCpu(int newHealth)
    {
        _healthTextCpu.SetText($"{newHealth}");
    }

    private void UpdateManaCpu(int newMana)
    {
        _manaTextCpu.SetText($"{newMana}");
    }

    private void EndGame(EPlayerType winningPlayerType)
    {
        StartCoroutine(DisplayEndGameScreen(winningPlayerType));
    }

    private IEnumerator DisplayEndGameScreen(EPlayerType winningPlayerType)
    {
        _textTurnTransition.SetText(winningPlayerType == EPlayerType.Human ? "You won!" : "You lose...");
        
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.DOFade(1f, _timeFadeTurnTransition);
        yield return new WaitForSeconds(2.5f);
        canvasGroup.DOFade(0f, _timeFadeTurnTransition*1.5f).OnComplete(() =>
        {
            canvasGroup.gameObject.SetActive(false);
        });
    }

    private void OnHumanStartTurn()
    {
        StartCoroutine(DisplayTurnTransition("Your turn"));
    }

    private void OnCpuStartTurn()
    {
        StartCoroutine(DisplayTurnTransition("Enemy turn"));
    }

    private IEnumerator DisplayTurnTransition(string textToDisplay)
    {
        _textTurnTransition.SetText(textToDisplay);
        
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.DOFade(1f, _timeFadeTurnTransition);
        yield return new WaitForSeconds(1.5f);
        canvasGroup.DOFade(0f, _timeFadeTurnTransition*1.5f).OnComplete(() =>
        {
            canvasGroup.gameObject.SetActive(false);
        });
    }
}
