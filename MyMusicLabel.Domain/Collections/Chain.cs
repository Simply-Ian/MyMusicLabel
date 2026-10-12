using System.Collections;

namespace MyMusicLabel.Domain.Collections;

/// TODO: включить файл (Include)

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
/// За поддержку количества больших элементов мы платим удорожанием вставки/удаления
/// </summary>
/// <typeparam name="T">Тип значения в узле. Должен поддерживать сравнение.
/// TODO: Подумать: может ли он быть значимым типом?
/// </typeparam>
public class Chain<T> : ICollection<T> where T: IComparable<T>
{
    /* private protected согласуется с internal модификатором у типа: разрешает доступ из этого класса или из 
    производных классов в этой сборке.
    System.Tuple реализует IComparable, экземпляры сравниваются лексикографически */
    private protected BinarySearchTree<Tuple<T, uint>> _tree = new();

    // TODO: в энумераторе дерева определить каст в IEnumerator<T>
    public IEnumerator<T> GetEnumerator() => (IEnumerator<T>)_tree.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /* TODO: походу, придется реализовывать Add и Delete как методы расширения, потому что дерево само по себе 
    предоставляет операции Add(хранимый тип) и Remove(хранимый тип) */
    // TODO: увеличивать счетчики у всех узлов слева
    public void Add(T item) => _tree.Add(item);
    
    /// <summary>
    /// Не нужен, определен для совместимости с ICollection
    /// </summary>
    public void Clear()
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Не нужен, определен для совместимости с ICollection. К тому же, если не добавлять еще одну нижележащую коллекцию,
    /// реализовать его несколько неприятно, т. к. дерево хранит кортежи => нужно либо хранить свой класс с
    /// переопределенными операторами равенства, либо enumerate все дерево в список и уже в списке искать значения
    /// </summary>
    public bool Contains(T item) => throw new NotImplementedException();

    public void CopyTo(T[] array, int arrayIndex) 
        => _tree
            .Select(pair => pair.Value.Item1)
            .ToArray()
            .CopyTo(array, arrayIndex);

    /* TODO: походу, придется реализовывать Add и Delete как методы расширения, потому что дерево само по себе
    предоставляет операции Add(хранимый тип) и Remove(хранимый тип) */
    // TODO: уменьшать счетчики у всех узлов слева
    public bool Remove(T item) => _tree.Remove(item);

    public int Count => (int)_tree.Count;
    public bool IsReadOnly => false;
}