using MyMusicLabel.Domain.Collections.BinarySearchTree;

namespace Tests.BstTests;

[TestOf(typeof(BinarySearchTree<>))]
public class AdditionTests
{
    [Test]
    public void AdditionToEmptyTreeTest()
    {
        var tree = new BinarySearchTree<int>();
        Assume.That(tree, Has.Count.EqualTo(0));
        
        tree.Add(1);
        
        Assert.That(tree, Has.Count.EqualTo(1));
        Assert.That(tree.First().Value, Is.EqualTo(1));
    }

    [Test]
    public void AddLeftChildToRootTest()
    {
        BinarySearchTree<int> tree = [0];
        
        tree.Add(-1);
        
        Assert.That(tree, Has.Count.EqualTo(2));
        Assert.That(tree.Select(node => node.Value), Is.EquivalentTo([-1, 0]));
    }

    [Test]
    public void AddRightChildToRootTest()
    {
        BinarySearchTree<int> tree = [0];
        
        tree.Add(1);
        
        Assert.That(tree, Has.Count.EqualTo(2));
        Assert.That(tree.Select(node => node.Value), Is.EquivalentTo([0, 1]));
    }

    [Test]
    public void AddToFullTreeTest()
    {
        BinarySearchTree<int> tree = [-1, 0, 2];
        
        tree.Add(1);
        
        Assert.That(tree, Has.Count.EqualTo(4));
        Assert.That(tree.Select(node => node.Value), Is.EquivalentTo([-1, 0, 1, 2]));
    }

    [Test]
    public void AddDublicate()
    {
        BinarySearchTree<int> tree = [-1, 0, 2];
        
        tree.Add(0);
        
        Assert.That(tree, Has.Count.EqualTo(4));
        Assert.That(tree.Select(node => node.Value), Is.EquivalentTo([-1, 0, 0, 2]));
    }
}