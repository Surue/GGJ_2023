using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUIController : MonoBehaviour
{
    [SerializeField] private Button _nextTurnButton;
    [SerializeField] private TextMeshProUGUI _manaText;
    [SerializeField] private TextMeshProUGUI _healthText;

    private HumanPlayer _humanPlayer;
    
    private void Start()
    {
        _humanPlayer = FindObjectOfType<HumanPlayer>();

        _nextTurnButton.onClick.AddListener(_humanPlayer.NextTurn);
    }

    void Update()
    {
        
    }
}
