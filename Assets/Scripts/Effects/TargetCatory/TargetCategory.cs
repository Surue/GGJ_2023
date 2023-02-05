using System;

public abstract class TargetCategory : ITargetable
{
    public virtual void TakeDamage(int damage, ITargetable owner)
    {
    }

    public virtual void Heal(int healAmount, ITargetable owner)
    {
    }

    public virtual void AddBuff(BuffEffect buffEffect, ITargetable owner, Action<CardController> act)
    {
    }
}