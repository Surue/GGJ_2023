using System;

[Serializable]
public abstract class TargetEffect : Effect
{
}

public interface ITargetable
{
    void TakeDamage(int damage);

    void Heal(int healAmount);

    void AddBuff(BuffEffect buffEffect, ITargetable owner, Action<CardController> act);
}