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
        target.AddBuff(this, Owner, (x) => {x.Heal(Amount);});
    }
    
    public override void Debuff(ITargetable target)
    {
        Owner.RemoveBuff(this);
    }

    public override void Execute()
    {
        Execute(Target);
    }
}
