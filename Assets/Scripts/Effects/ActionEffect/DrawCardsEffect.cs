public class DrawCardsEffect : ActionEffect
{
    public int CardsAmount = 2;
    
    public override string Description()
    {
        return $"Draw {CardsAmount.ToString()} cards";
    }

    public override void Execute(ITargetable target)
    {
        for (int i = 0; i < CardsAmount; i++)
        {
            GameManager.Instance.CurrentPlayer.DrawCard();
        }
    }

    public override void Execute()
    {
        
    }
}