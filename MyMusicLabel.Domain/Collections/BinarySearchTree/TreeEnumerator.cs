using System.Collections;
using System.Runtime.CompilerServices;

// For testing purposes, so that I can place unit tests for BST and Chain in the Tests project
[assembly: InternalsVisibleTo("Tests")]

namespace MyMusicLabel.Domain.Collections.BinarySearchTree;

// Обходит дерево слева направо
internal class TreeEnumerator<T>(TreeNode<T>? root) : IEnumerator<TreeNode<T>> 
where T : IComparable<T>, IEquatable<T>
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