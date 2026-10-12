using MyMusicLabel.Domain.Collections;

namespace Tests.BstTests;

[TestOf(typeof(BinarySearchTree<>))]
public class RemovalTests
{
    [Test]
    public void RemoveTheOnlyNode()
    {
        BinarySearchTree<int> tree = [0];
        
        tree.Remove(0);
        
        Assert.That(tree, Has.Count.EqualTo(0));
        Assert.That(tree, Is.EquivalentTo(new List<int>()));
    }
    
    [Test]
    public void ThrowsIfRemovingFromEmptyTree()
    {
        BinarySearchTree<int> tree = [];
        Assert.Throws<InvalidOperationException>(() => tree.Remove(0));
    }

    [Test]
    public void ThrowsIfRemovingNotFoundNode()
    {
        BinarySearchTree<int> tree = [3, 2 , 5, 1, 4];
        Assert.Throws<InvalidOperationException>(() => tree.Remove(0));
    }
    
    [Test]
    public void RemoveChildlessNode()
    {
        BinarySearchTree<int> tree = [2, 1, 3];
        
        tree.Remove(1);
        
        Assert.That(tree, Has.Count.EqualTo(2));
        Assert.That(tree.Select(i => i.Value), Is.EquivalentTo(new List<int>([2, 3])));
    }

    [Test]
    public void RemoveNodeWithOneChild()
    {
        BinarySearchTree<int> tree = [2, 1, 3, 4];
        
        tree.Remove(3);
        
        Assert.That(tree, Has.Count.EqualTo(3));
        Assert.That(tree.Select(i => i.Value), Is.EquivalentTo(new List<int>([1, 2, 4])));
    }

    [Test]
    public void RemoveNodeWithTwoChildren()
    {
        BinarySearchTree<int> tree = [5, 2, 1, 4, 3, 8, 7, 12, 11, 9, 10, 6];
        
        tree.Remove(8);
        
        Assert.That(tree, Has.Count.EqualTo(11));
        Assert.That(tree.Select(i => i.Value), Is.EquivalentTo(Enumerable.Range(1, 12).Where(i => i != 8)));
    }
}