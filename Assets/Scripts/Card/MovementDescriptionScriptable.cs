using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementType
{
    Adjacent,
    FullBoard,
}

[CreateAssetMenu(fileName = "NewMovementDescription", menuName = "ScriptableObjects/Movement Description")]
public class MovementDescriptionScriptable : ScriptableObject
{
    [SerializeField] private int _maxMovementLineCount;
    [SerializeField] private int _maxMovementColumnCount;
    [SerializeField] private int _freeMovementCount;

    public int MaxMovementLineCount => _maxMovementLineCount;
    public int MaxMovementColumnCount => _maxMovementColumnCount;
    public int FreeMovementCount => _freeMovementCount;
}
    
