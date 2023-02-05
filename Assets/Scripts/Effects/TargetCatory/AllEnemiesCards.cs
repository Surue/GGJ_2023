using System;
using System.Collections.Generic;

public class AllEnemiesCards : TargetCategory, ITargetable
{
    public override void TakeDamage(int damage, ITargetable owner)
    {
        foreach (var card in GetAllEnemiesCards())
        {
            card.CardTakeDamage(damage);
        }
    }

    public override void Heal(int healAmount, ITargetable owner)
    {
        foreach (var card in GetAllEnemiesCards())
        {
            card.Heal(healAmount, owner);
        }
    }
    
    public override void AddBuff(BuffEffect buffEffect, ITargetable owner, Action<CardController> act)
    {
        foreach (var card in GetAllEnemiesCards())
        {
            if (card == (CardController)owner)
            {
                continue;
            }
            else
            {
                act(card);
                card.AddBuff(buffEffect, owner, act);
            }
        }
    }
    
    public List<CardController> GetAllEnemiesCards()
    {
        return GameManager.Instance.EnemyPlayer.CardsOnBoard;
    }
}