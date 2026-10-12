using System.Collections;
using System.Runtime.CompilerServices;

// For testing purposes, so that I can place unit tests for BST and Chain in the Tests project
[assembly: InternalsVisibleTo("Tests")]

namespace MyMusicLabel.Domain.Collections;

internal class BinarySearchTree<TValue> : IEnumerable<TreeNode<TValue>>
where TValue : IComparable
{
    private TreeNode<TValue>? _root;
    public uint Count { get; private set; }
    
    // Я решил реализовать интерфейс IEnumerable без yield просто ради интереса и практики.
    public IEnumerator<TreeNode<TValue>> GetEnumerator() => new TreeEnumerator<TValue>(_root);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(TValue value)
    {
        Count++;
        if (_root is null)
        {
            _root = new TreeNode<TValue>(value, null);
            return;
        }
        
        var parentLeaf = _getParentLeaf(value, _root!);
        TreeNode<TValue> newNode = new (value, parentLeaf);
        if (value.CompareTo(parentLeaf.Value) <= 0) parentLeaf.LeftChild = newNode;
        else parentLeaf.RightChild = newNode;
    }

    private static TreeNode<TValue> _getParentLeaf(TValue value, TreeNode<TValue> root)
    {
        // Не получение итератора, т. к. итератор начинает с самого левого листа, а здесь надо сверху
        var prevNode = root;
        var currentNode = root;
        while (currentNode is not null)
        {
            prevNode = currentNode;
            currentNode = value.CompareTo(currentNode.Value) <= 0 ? currentNode.LeftChild : currentNode.RightChild;
        }

        return prevNode;
    }

    /// <summary>
    /// Throws InvalidOperationException in case the provided value is not stored in the tree.
    /// </summary>
    public void Remove(TValue value)
    {
        var node = this.First(node => node.Value.Equals(value));
        switch (node)
        {
            case { HasLeftChild: false, HasRightChild: false }:
                _deleteChildlessNode(node);
                break;
            case { HasLeftChild: true, HasRightChild: false }:
                _replaceNodeWithItsChild(node, node.LeftChild!);
                break;
            case { HasLeftChild: false, HasRightChild: true }:
                _replaceNodeWithItsChild(node, node.RightChild!);
                break;
            default:
                _replaceNodeWithLeftmostDescendant(node);
                break;
        }
        Count--;
    }

    private void _deleteChildlessNode(TreeNode<TValue> node)
    {
        if (!node.HasParent) // Удаление корня
            _root = null;
        else
        {
            var parent = node.Parent;
            if (parent!.RightChild == node) parent.RightChild = null;
            else parent.LeftChild = null;
        }
    }
    private void _replaceNodeWithItsChild(TreeNode<TValue> node, TreeNode<TValue> child)
    {
        if (!node.HasParent) // Удаление корня
            _root = child;
        else
        {
            var parent = node.Parent;
            if (parent!.RightChild == node) parent.RightChild = child;
            else parent.LeftChild = child;
        }
    }

    /// <summary>
    /// Assumes that node has both children
    /// </summary>
    private void _replaceNodeWithLeftmostDescendant(TreeNode<TValue> node)
    {
        var rightChild = node.RightChild;
        var leftmostDescendant = rightChild;
        while (leftmostDescendant!.HasLeftChild) leftmostDescendant = leftmostDescendant.LeftChild;
        
        leftmostDescendant.Parent!.LeftChild = leftmostDescendant.RightChild;
        leftmostDescendant.RightChild = node.RightChild;
        leftmostDescendant.LeftChild = node.LeftChild;
        
        if (!node.HasParent) // Удаление корня
            _root = leftmostDescendant;
        else
        {
            var parent = node.Parent;
            if (parent!.RightChild == node) parent.RightChild = leftmostDescendant;
            else parent.LeftChild = leftmostDescendant;
        }
    }
}

internal class TreeNode<T>(T val, TreeNode<T>? parent) where T : IComparable
{
    public TreeNode<T>? LeftChild
    {
        get;
        set
        {
            field = value;
            value?.Parent = this;
        }
    }
    public TreeNode<T>? RightChild {
        get;
        set
        {
            field = value;
            value?.Parent = this;
        }
    }

    public TreeNode<T>? Parent { get; private set; } = parent;
    
    public T Value { get; } = val;
    
    public bool HasLeftChild => LeftChild is not null;
    public bool HasRightChild => RightChild is not null;
    public bool HasParent => Parent is not null;
    
    public static implicit operator TreeNode<T>(T value) => new (value, null);
    
    public override string ToString() 
        => $"TreeNode({Value.ToString() ?? "null"})";
}

// Обходит дерево слева направо
internal class TreeEnumerator<T>(TreeNode<T>? root) : IEnumerator<TreeNode<T>> where T : IComparable
{
    private TreeNode<T>? _current;
    private readonly HashSet<TreeNode<T>> _visitedNodes = [];
    public bool MoveNext()
    {
        var previousNode = _current;
        // Microsoft docs: enumerators are positioned before the first element until the first MoveNext() call.
        if (_current is null) _descendToLeftmostNode();
        else
        {
            if (CanVisitRightChild) _current = _current.RightChild!;
            if (CanVisitLeftChild)
                while (_current.HasLeftChild) _current = _current.LeftChild!;
            // Вторая часть условия нужна, если мы в первом условном операторе пришли в правого сына без детей
            else if (_current.HasParent && _visitedNodes.Contains(_current))
            {
                while (_current.Parent?.RightChild == _current) _current = _current.Parent;
                if (_current.HasParent) // Текущий узел - левый сын
                    _current = _current.Parent!;
                else return false; // Вернулись в корень
            } 
        }
        _visitedNodes.Add(_current!);
        return previousNode != _current;
    }
    
    private bool CanVisitRightChild => (_current?.HasRightChild ?? false) && !_visitedNodes.Contains(_current.RightChild!);
    private bool CanVisitLeftChild => (_current?.HasLeftChild ?? false) && !_visitedNodes.Contains(_current.LeftChild!);

    public void Reset()
    {
        _visitedNodes.Clear();
        _current = null;
    }

    TreeNode<T> IEnumerator<TreeNode<T>>.Current => _current!;

    object? IEnumerator.Current => _current;

    public void Dispose() { }

    private void _descendToLeftmostNode()
    {
        if (root is null) return;
        _current = root;
        while (_current.HasLeftChild) _current = _current.LeftChild!;
    }
}