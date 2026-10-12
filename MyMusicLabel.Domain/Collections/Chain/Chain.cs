using System.Collections;
using MoreLinq;
using MyMusicLabel.Domain.Collections.BinarySearchTree;

namespace MyMusicLabel.Domain.Collections.Chain;

/// <summary>
/// Представляет собой последовательность отсортированных значений. Позволяет для произвольного промежутка найти
/// количество элементов, находящихся в этом промежутке (включая концы).
/// Хранит элементы в BST, для каждого узла вместе со значением хранит количество элементов, больших него. Соответственно,
/// задача подсчета значений в промежутке сводится к поиску
/// 1. минимального узла, большего или равного левому концу
/// 2. максимального узла, не превосходящего правый конец
/// 3. расчету разности сопутствующих значений этих узлов.
/// BST выбрано в качестве нижележащей структуры данных, поскольку оно позволяет за логарифмическое время выполнять шаги
/// 1 и 2.
///
/// За поддержку количества бОльших элементов мы платим удорожанием вставки/удаления
/// </summary>
/// <typeparam name="T">Тип значения в узле. Должен поддерживать сравнение. </typeparam>
public class Chain<T> : ICollection<T> where T: IComparable<T>, IEquatable<T>
{
    /* private protected согласуется с internal модификатором у типа: разрешает доступ из этого класса или из 
    производных классов в этой сборке */
    private protected BinarySearchTree<ChainRing<T>> _tree = new();
    
    // А тут использую yield, потому что мне лень конвертировать энумератор дерева в энумератор цепочки вручную
    public IEnumerator<T> GetEnumerator()
    {
        foreach (var node in _tree) 
            yield return node.Value.StoredItem;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public void Add(T item)
    {
        ChainRing<T> ring = new(item)
        {
            LargerElementsCount = (uint)_tree.Count(node => node.Value.StoredItem.CompareTo(item) >= 0)
        };
        _tree.Add(ring);
        _tree
            .Where(node => node.Value.StoredItem.CompareTo(item) <= 0)
            .ForEach(node => node.Value.LargerElementsCount++);
    }

    public void Clear() => _tree.Clear();
    public bool Contains(T item) => _tree.Any(node => node.Value.StoredItem.Equals(item));
    public void CopyTo(T[] array, int arrayIndex) 
        => _tree
            .Select(node => node.Value.StoredItem)
            .ToArray()
            .CopyTo(array, arrayIndex);
    
    public bool Remove(T item)
    {
        try
        {
            _tree.Remove(item);
            _tree
                .Where(node => node.Value.StoredItem.CompareTo(item) <= 0)
                .ForEach(node => node.Value.LargerElementsCount--);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public uint CountInInterval(T startInclusive, T endInclusive)
    {
        var startNode = _tree
            .FirstOrDefault(
                node => node.Value.StoredItem.CompareTo(startInclusive) >= 0
            )?.Value;
        if (startNode is null) return 0;
        var endNode = _tree
                    .LastOrDefault(
                        node => node.Value.StoredItem.CompareTo(endInclusive) <= 0
                    )?.Value;
        if (endNode is null) return (uint)Count;
        return startNode.LargerElementsCount - endNode.LargerElementsCount + _tree.ItemCount(endNode.StoredItem);
    }

    public int Count => (int)_tree.Count;
    public bool IsReadOnly => false;
}