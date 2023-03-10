using System;
using System.Collections.Generic;

public class AllSelfCards : TargetCategory, ITargetable
{
    public override void TakeDamage(int damage, ITargetable owner)
    {
        foreach (var card in GetAllCards())
        {
            card.CardTakeDamage(damage);
        }
    }

    public override void Heal(int healAmount, ITargetable owner)
    {
        var allEnemies = GetAllCards();
        foreach (var enemy in allEnemies)
        {
            enemy.Heal(healAmount, owner);
        }
    }
    
    public override void AddBuff(BuffEffect buffEffect, ITargetable owner, Action<CardController> act)
    {
        var cards = GetAllCards();
        foreach (var card in cards)
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
    
    public List<CardController> GetAllCards()
    {
        return GameManager.Instance.CurrentPlayer.CardsOnBoard;
    }
}