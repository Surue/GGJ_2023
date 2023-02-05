public class HealEffect : ActionEffect
{
    public int HealAmount = 2;
    public override string Description()
    {
        return $"Repair helicopter {HealAmount.ToString()}";
    }

    public override void Execute(ITargetable target)
    {
        target.Heal(HealAmount);
    }

    public override void Execute()
    {
        
    }
}