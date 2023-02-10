using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AiDescription", menuName = "ScriptableObjects/AiDescription")]
public class AiDescriptionScriptable : ScriptableObject
{
    [SerializeField] private float _maxTComputationTime = 5.0f;
    [SerializeField] private float _scoreCpuHealthFactor = 1.0f;
    [SerializeField] private float _scoreTurnCountFactor = 40.0f;
    [SerializeField] private float _scoreCardLostCountFactor = -5.0f;
    [SerializeField] private float _scoreCardDestroyedFactor = 5.0f;

    public float MaxTComputationTime => _maxTComputationTime;
    public float ScoreCpuHealthFactor => _scoreCpuHealthFactor;
    public float ScoreTurnCountFactor => _scoreTurnCountFactor;
    public float ScoreCardLostCountFactor => _scoreCardLostCountFactor;
    public float ScoreCardDestroyedFactor => _scoreCardDestroyedFactor;
}
