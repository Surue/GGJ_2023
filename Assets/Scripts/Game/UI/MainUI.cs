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
        _humanPlayer = FindObjectOfType<HumanPlayer>();
        
        _humanPlayer.OnHealthChanged += UpdateHealthHuman;
        _humanPlayer.OnManaChanged += UpdateManaHuman;
        
        _nextTurn.onClick.AddListener(_humanPlayer.NextTurn);

        _cpuPlayer = FindObjectOfType<CpuPlayer>();

        _cpuPlayer.OnHealthChanged += UpdateHealthCpu;
        _cpuPlayer.OnManaChanged += UpdateManaCpu;

        GameManager.Instance.onCpuTurnStarted += OnCpuStartTurn;
        GameManager.Instance.onHumanTurnStarted += OnHumanStartTurn;
        GameManager.Instance.onGameEnded += EndGame;
    }

    private void UpdateHealthHuman(int newHealth, int maxHealth)
    {
        _healthTextPlayer.SetText($"{newHealth}");  
    }

    private void UpdateManaHuman(int newMana, int maxMana)
    {
        _manaTextPlayer.SetText($"{newMana}");  
    }

    private void UpdateHealthCpu(int newHealth, int maxHealth)
    {
        _healthTextCpu.SetText($"{newHealth}");
    }

    private void UpdateManaCpu(int newMana, int maxMana)
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
