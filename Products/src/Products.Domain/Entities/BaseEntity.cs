namespace Products.Domain.Entities;

public abstract class BaseEntity <TKey> where TKey : struct
{
    public TKey Id { get; set; }
}

public abstract class BaseEntity : BaseEntity<int>
{
}
