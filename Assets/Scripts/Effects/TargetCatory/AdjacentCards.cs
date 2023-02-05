using System;
using System.Collections.Generic;

public class AdjacentCards : TargetCategory, ITargetable
{
    public override void TakeDamage(int damage, ITargetable owner)
    {
        foreach (var card in GetAdjacentCards(owner))
        {
            card.CardTakeDamage(damage);
        }
    }

    public override void Heal(int healAmount, ITargetable owner)
    {
        foreach (var card in GetAdjacentCards(owner))
        {
            card.Heal(healAmount, owner);
        }
    }
    
    public override void AddBuff(BuffEffect buffEffect, ITargetable owner, Action<CardController> act)
    {
        foreach (var card in GetAdjacentCards(owner))
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
    
    public List<CardController> GetAdjacentCards(ITargetable owner)
    {
        return GameManager.Instance.CurrentPlayer.GetCrossNeighbors((CardController)owner);
    }
}
