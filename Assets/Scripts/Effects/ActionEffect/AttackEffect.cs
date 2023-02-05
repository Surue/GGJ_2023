using System;
using Sirenix.OdinInspector;

[Serializable]
public class AttackEffect : Effect
{
    public bool OverrideDescription = false;
    [ShowIf("@OverrideDescription")]
    public string OverridedDescription;
    
    public int Damage;
    
    public override string Description()
    {
        return OverrideDescription ? OverridedDescription : null;
    }

    public override void Execute(ITargetable target)
    {
        if (target == null)
            return;

        target.TakeDamage(Damage);
    }

    public override void Execute()
    {
        if (Target != null)
        {
            Execute(Target);
        }
    }
}