using System;

public class BuffEffect : Effect
{
    public override string Description()
    {
        return "";
    }

    public override void Execute(ITargetable target)
    {
    }

    public override void Execute()
    {
    }

    public virtual void Rollback()
    {
        
    }
    public virtual void Debuff(ITargetable target)
    {
        
    }
}