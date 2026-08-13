namespace Products.Domain.Entities;

public abstract class BaseEntity <TKey> where TKey : struct
{
    public TKey Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public abstract class BaseEntity : BaseEntity<int>
{
}

public abstract class AuditableEntity<TKey> : BaseEntity<TKey> where TKey : struct
{
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public abstract class AuditableEntity : BaseEntity<int>
{
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

