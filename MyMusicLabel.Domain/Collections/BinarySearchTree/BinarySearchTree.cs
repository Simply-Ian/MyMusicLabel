using System.Collections;

namespace MyMusicLabel.Domain.Collections.BinarySearchTree;

internal class BinarySearchTree<TValue> : IEnumerable<TreeNode<TValue>>
    where TValue : IComparable<TValue>, IEquatable<TValue>
{
    private TreeNode<TValue>? _root;
    public uint Count { get; private set; }
    
    // Я решил реализовать интерфейс IEnumerable без yield просто ради интереса и практики.
    public IEnumerator<TreeNode<TValue>> GetEnumerator() => new TreeEnumerator<TValue>(_root);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #region Add
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
    #endregion

    #region Remove
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
        // Мб стоит сюда добавить вызов сборщика мусора, чтобы автоматически подобрал удаленный узел
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
        
        // Если правый сын удаляемого узла не имеет левого потомка, перецеплять правого сына к родителю не надо
        if (leftmostDescendant.Parent!.LeftChild == leftmostDescendant)
        {
            leftmostDescendant.Parent!.LeftChild = leftmostDescendant.RightChild;
            leftmostDescendant.RightChild = node.RightChild;
        }
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
    #endregion

    public void Clear()
    {
        // Мб стоит сюда добавить вызов сборщика мусора, чтобы автоматически подобрал удаленные узлы
        _root?.DeleteChildrenRecursively();
        _root = null;
        Count = 0;
    }

    public uint ItemCount(TValue value)
    {
        if (_root is null) return 0;
        var curNode = _root;
        while (curNode is not null && !curNode.Value.Equals(value))
            curNode = value.CompareTo(curNode.Value) > 0 ? curNode.RightChild : curNode.LeftChild;
        if (curNode is null) return 0;

        uint counter = 0;
        while (curNode is not null && curNode!.Value.Equals(value))
        {
            counter++;
            curNode = curNode.LeftChild;
        }
        return counter;
    }
}