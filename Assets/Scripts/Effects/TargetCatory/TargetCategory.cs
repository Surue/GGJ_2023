using System;

public abstract class TargetCategory : ITargetable
{
    public virtual void TakeDamage(int damage)
    {
    }

    public virtual void Heal(int healAmount)
    {
    }

    public virtual void AddBuff(BuffEffect buffEffect, ITargetable owner, Action<CardController> act)
    {
    }
}