using System;

public class BuffDamage : BuffEffect
{
    public int Amount;
    
    public override string Description()
    {
        return $"+{Amount} damage foreach Assault";
    }

    public override void Execute(ITargetable target)
    {
        target.AddBuff(this, Owner, (x) => {x.IncreaseDamage(Amount);});
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