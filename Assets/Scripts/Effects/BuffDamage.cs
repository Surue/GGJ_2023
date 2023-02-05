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
        Owner.IncreaseDamage(Amount);
    }
    
    public override void Debuff(ITargetable target)
    {
        Owner.RemoveBuff(this);
    }

    public override void Execute()
    {        
        Owner.AddBuff(this, Owner);
    }
}