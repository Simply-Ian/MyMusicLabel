using MyMusicLabel.Domain.Collections.BinarySearchTree;

namespace Tests.BstTests;

[TestOf(typeof(BinarySearchTree<>))]
public class EnumerationTests
{
    [Test]
    public void EnumerationTest()
    {
        BinarySearchTree<int> tree = [5, 2, 1, 4, 3, 8, 7, 9, 6];
        
        var nodes = tree.ToList();
        
        Assert.That(tree, Has.Count.EqualTo(nodes.Count));
        Assert.That(nodes.Select(node => node.Value), Is.EquivalentTo(Enumerable.Range(1, 9)));
    }
    
    [Test]
    public void CountTest()
    {
        BinarySearchTree<int> tree = [5, 2, 1, 4, 3, 8, 7, 9, 6];
        Assert.That(tree, Has.Count.EqualTo(9));
    }
    
    [Test]
    public void EmptyTreeCountTest()
    {
        BinarySearchTree<int> tree = [];
        Assert.That(tree, Has.Count.EqualTo(0));
    }
}