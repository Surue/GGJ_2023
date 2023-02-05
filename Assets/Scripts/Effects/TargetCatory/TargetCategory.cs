public abstract class TargetCategory : ITargetable
{
    public virtual void TakeDamage(int damage)
    {
    }

    public virtual void Heal(int healAmount)
    {
    }
}