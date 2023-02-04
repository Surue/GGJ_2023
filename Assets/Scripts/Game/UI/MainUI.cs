using System;
using System.Collections;
using System.Collections.Generic;
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
        _panel.SetActive(true);
        
        _textTurnTransition.SetText("");

        float timer = 0;

        Color currentColor = Color.black;

        while (timer < _timeFadeTurnTransition)
        {
            timer += Time.deltaTime;
            currentColor = _imageTurnTransition.color;
            currentColor.a = timer;
            _imageTurnTransition.color = currentColor;
            yield return null;
        }

        currentColor.a = 1;
        _imageTurnTransition.color = currentColor;
        
        
        switch (winningPlayerType)
        {
            case EPlayerType.Human:
                _textTurnTransition.SetText("You Win");
                break;
            case EPlayerType.CPU:
                _textTurnTransition.SetText("Bah alors Martin, on est un looser?");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(winningPlayerType), winningPlayerType, null);
        }

        yield return new WaitForSeconds(_timeOnScreenTurnTransition);
        
        _textTurnTransition.SetText("");
        
        timer = 0;
        while (timer < _timeFadeTurnTransition)
        {
            timer += Time.deltaTime;
            currentColor = _imageTurnTransition.color;
            currentColor.a = 1 - timer;
            _imageTurnTransition.color = currentColor;
            yield return null;
        }
        
        currentColor.a = 0;
        _imageTurnTransition.color = currentColor;
        _panel.SetActive(false);
    }

    private void OnHumanStartTurn()
    {
        StartCoroutine(DisplayTurnTransition("Your Turn"));
    }

    private void OnCpuStartTurn()
    {
        StartCoroutine(DisplayTurnTransition("Enemy Turn"));
    }

    private IEnumerator DisplayTurnTransition(string textToDisplay)
    {
        _panel.SetActive(true);
        
        _textTurnTransition.SetText("");

        float timer = 0;

        Color currentColor = Color.black;

        while (timer < _timeFadeTurnTransition)
        {
            timer += Time.deltaTime;
            currentColor = _imageTurnTransition.color;
            currentColor.a = timer;
            _imageTurnTransition.color = currentColor;
            yield return null;
        }

        currentColor.a = 1;
        _imageTurnTransition.color = currentColor;
        
        _textTurnTransition.SetText(textToDisplay);

        yield return new WaitForSeconds(_timeOnScreenTurnTransition);
        
        _textTurnTransition.SetText("");
        
        timer = 0;
        while (timer < _timeFadeTurnTransition)
        {
            timer += Time.deltaTime;
            currentColor = _imageTurnTransition.color;
            currentColor.a = 1 - timer;
            _imageTurnTransition.color = currentColor;
            yield return null;
        }
        
        currentColor.a = 0;
        _imageTurnTransition.color = currentColor;
        _panel.SetActive(false);
    }
}
