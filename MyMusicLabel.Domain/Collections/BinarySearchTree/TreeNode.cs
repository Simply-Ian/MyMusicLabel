namespace MyMusicLabel.Domain.Collections.BinarySearchTree;

internal class TreeNode<T>(T val, TreeNode<T>? parent) 
    where T : IComparable<T>, IEquatable<T>
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

    public void DeleteChildrenRecursively()
    {
        if (HasLeftChild)
        {
            LeftChild!.DeleteChildrenRecursively();
            LeftChild = null;
        }
        if (HasRightChild)
        {
            RightChild!.DeleteChildrenRecursively();
            RightChild = null;
        }
    }
}