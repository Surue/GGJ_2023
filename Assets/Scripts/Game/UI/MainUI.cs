using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [Header("Human")]
    [SerializeField] private Button _nextTurn;
    [SerializeField] private TextMeshProUGUI _manaTextPlayer;
    [SerializeField] private TextMeshProUGUI _healthTextPlayer;
    [Header("CPU")]
    [SerializeField] private TextMeshProUGUI _manaTextCpu;
    [SerializeField] private TextMeshProUGUI _healthTextCpu;

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
    }

    private void UpdateHealthHuman(int newHealth, int maxHealth)
    {
        _healthTextPlayer.SetText($"HP {newHealth}/{maxHealth}");  
    }

    private void UpdateManaHuman(int newMana, int maxMana)
    {
        _manaTextPlayer.SetText($"Mana {newMana}/{maxMana}");  
    }

    private void UpdateHealthCpu(int newHealth, int maxHealth)
    {
        _healthTextCpu.SetText($"HP {newHealth}/{maxHealth}");
    }

    private void UpdateManaCpu(int newMana, int maxMana)
    {
        _manaTextCpu.SetText($"Mana {newMana}/{maxMana}");
    }
}
