using System.Collections.Generic;

public class AllEnemiesCards : TargetCategory, ITargetable
{
    public override void TakeDamage(int damage)
    {
        foreach (var card in GetAllEnemies())
        {
            card.CardTakeDamage(damage);
        }
    }

    public override void Heal(int healAmount)
    {
        var allEnemies = GetAllEnemies();
        foreach (var enemy in allEnemies)
        {
            enemy.Heal(healAmount);
        }
    }
    
    public List<CardController> GetAllEnemies()
    {
        return GameManager.Instance.EnemyPlayer.CardsOnBoard;
    }
}