using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CpuPlayer : Player
{
    private bool _isPlaying;
    
    void Start()
    {
        GameManager.Instance.onCpuTurnStarted += StartTurn;
        GameManager.Instance.onCpuTurnStarted += AddManaStartTurn;
        GameManager.Instance.onCpuTurnFinished += EndTurn;

        _isPlaying = false;
    }

    void Update()
    {
        if(!_isPlaying) return;
    }

    private void StartTurn()
    {
        _isPlaying = true;
        
        FillHand();
        
        GameManager.Instance.HasFinishedStartingTurn();
    }
    
    private void EndTurn()
    {
        _isPlaying = false;
    }
}
