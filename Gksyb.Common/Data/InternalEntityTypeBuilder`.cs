namespace Chloe.Entity
{
    public class InternalEntityTypeBuilder<TEntity> : EntityTypeBuilder<TEntity>
    {
        public InternalEntityTypeBuilder() : base(true)
        {
        }
    }
}