using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : Player
{
    private bool _isPlaying;
    
    private void Start()
    {
        GameManager.Instance.onHumanTurnStarted += StartTurn;
        GameManager.Instance.onHumanTurnFinished += EndTurn;
        _isPlaying = false;
    }

    private void Update()
    {
        if (!_isPlaying) return;
    }

    private void StartTurn()
    {
        _isPlaying = true;
        
        FillHand();
    }
    
    private void EndTurn()
    {
        _isPlaying = false;
    }
}
