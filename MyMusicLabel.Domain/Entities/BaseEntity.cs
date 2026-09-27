namespace MyMusicLabel.Domain.Entities;

public class BaseEntity (Guid id) : IEquatable<BaseEntity>
{
    public Guid Id => id;
    
    public override bool Equals(object? o) => Equals(o as BaseEntity);

    public override int GetHashCode() => Id.GetHashCode();

    public bool Equals(BaseEntity? other)
        => other is not null && Id == other.Id;

    public static bool operator ==(BaseEntity? a, BaseEntity? b) => a is null && b is null 
                                                                    || a is not null && b is not null && a.Equals(b);

    public static bool operator !=(BaseEntity? a, BaseEntity? b) => !(a == b);
}