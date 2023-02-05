using System.Collections.Generic;

public class AllSelfCards : TargetCategory, ITargetable
{
    public override void TakeDamage(int damage)
    {
        foreach (var card in GetAllCards())
        {
            card.CardTakeDamage(damage);
        }
    }

    public override void Heal(int healAmount)
    {
        var allEnemies = GetAllCards();
        foreach (var enemy in allEnemies)
        {
            enemy.Heal(healAmount);
        }
    }
    
    public List<CardController> GetAllCards()
    {
        return GameManager.Instance.CurrentPlayer.CardsOnBoard;
    }
}