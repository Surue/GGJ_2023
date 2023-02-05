using System;

public interface ITargetable
{
    void TakeDamage(int damage, ITargetable owner);

    void Heal(int healAmount, ITargetable owner);

    void AddBuff(BuffEffect buffEffect, ITargetable owner, Action<CardController> act);
}