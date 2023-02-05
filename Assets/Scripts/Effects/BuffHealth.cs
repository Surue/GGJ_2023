using System;

public class BuffHealth : BuffEffect
{
    public int Amount;
    
    public override string Description()
    {
        return $"All enemies: +{Amount} HP";
    }

    public override void Execute(ITargetable target)
    {
        // var enemies = GameManager.Instance.CurrentCombat.Enemies;
        //
        // foreach (var enemy in enemies)
        // {
        //     if (enemy.CardInstance == (Card) target)
        //     {
        //         enemy.CardInstance.Health += Amount;
        //         
        //         var gameEventsystem = new GameEventSystem.Ref();
        //         gameEventsystem.Events.OnTargetableBuffed.Invoke(new Tuple<BuffEffect, ITargetable>(this, target));
        //
        //     }
        // }
    }
    
    public override void Debuff(ITargetable target)
    {
        // var enemies = GameManager.Instance.CurrentCombat.Enemies;
        //
        // foreach (var enemy in enemies)
        // {
        //     if (enemy.CardInstance == (Card) target)
        //     {
        //         enemy.CardInstance.Health -= Amount;
        //         
        //         var gameEventsystem = new GameEventSystem.Ref();
        //         gameEventsystem.Events.OnTargetableDebuff.Invoke(new Tuple<BuffEffect, ITargetable>(this, target));
        //     }
        // }
    }

    public override void Execute()
    {
        // if (Target is AllEnemies allEnemies)
        // {
        //     var enemies = allEnemies.GetAllEnemies();
        //     foreach (var enemy in enemies)
        //     {
        //         if (enemy == Owner)
        //             continue;
        //         
        //         enemy.AddBuff(this, Owner);
        //     }
        // }
    }
}
