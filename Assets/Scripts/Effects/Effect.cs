using System;
using UnityEngine;

[Serializable]
public abstract class Effect
{
    public TargetCategory Target;
    
    [HideInInspector] public CardController Owner;
    
    public abstract string Description();
    public abstract void Execute(ITargetable target);
    public abstract void Execute();
}