namespace MyMusicLabel.Domain.Collections.Chain;

/// <summary>
/// Нужен, так как хранить в дереве кортежи неинтуитивно: непонятно, что значит второй элемент-число/
/// Сравнивается только по хранимому значению, чтобы правильно хранилось в дереве
/// </summary>
internal class ChainRing<TItem>(TItem item) : IComparable<ChainRing<TItem>>, IEquatable<ChainRing<TItem>> 
    where TItem: IComparable<TItem>, IEquatable<TItem>
{
    public TItem StoredItem = item;
    public uint LargerElementsCount;
    
    public int CompareTo(ChainRing<TItem> other) => StoredItem.CompareTo(other.StoredItem);
    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        return obj is ChainRing<TItem> other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ChainRing<>)}");
    }
    public bool Equals(ChainRing<TItem> other) => StoredItem.Equals(other.StoredItem);
    public override bool Equals(object? obj) => obj is ChainRing<TItem> other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(StoredItem);
    public static bool operator ==(ChainRing<TItem> left, ChainRing<TItem> right) => left.Equals(right);
    public static bool operator !=(ChainRing<TItem> left, ChainRing<TItem> right) => !left.Equals(right);
    public static implicit operator ChainRing<TItem>(TItem item) => new(item);
    public override string ToString() => $"Ring({item}, {LargerElementsCount})";
}