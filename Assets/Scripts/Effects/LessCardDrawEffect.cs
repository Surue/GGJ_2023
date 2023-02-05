public class LessCardDrawEffect : Effect
{
    public int Amount = 1;
    
    public override string Description()
    {
        return $"Survive:\nDraw {Amount.ToString()} less card next turn";
    }

    public override void Execute(ITargetable target)
    {
        GameManager.Instance.CurrentPlayer.ReduceMaxDrawnCard(Amount);
    }

    public override void Execute()
    {
        
    }
}