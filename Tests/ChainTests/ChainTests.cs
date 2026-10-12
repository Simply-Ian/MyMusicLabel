using MoreLinq;
using MyMusicLabel.Domain.Collections.Chain;

namespace Tests.ChainTests;

[TestOf(typeof(Chain<>))]
public class ChainTests
{
    [Test]
    public void EnumerationTest()
    {
        Chain<int> chain = [1, 2, 3, 4, 5, 6];
        
        Assert.That(chain, Has.Count.EqualTo(6));
        Assert.That(chain, Is.EquivalentTo(Enumerable.Range(1, 6)));
    }

    [Test]
    public void ClearTest()
    {
        Chain<int> chain = [1, 2, 3, 4, 5, 6];
        
        chain.Clear();
        
        Assert.That(chain, Has.Count.EqualTo(0));
        Assert.That(chain, Is.EquivalentTo(new List<int>()));
    }

    [Test]
    public void UnorderedInitialization()
    {
        Chain<int> chain = [6, 4, 1, 3, 2, 5];
        
        Assert.That(chain, Is.EquivalentTo(Enumerable.Range(1, 6)));
    }

    [Test]
    public void CheckIfContains()
    {
        Chain<int> chain = [1, 2, 3, 4, 5, 6];

        for (int i = 1; i <= 6; i++)
            Assert.That(chain, Does.Contain(i));
        Assert.That(chain, Does.Not.Contain(7));
    }

    [TestCase(1, 7, ExpectedResult = 6u)]
    [TestCase(1, 1, ExpectedResult = 1u)]
    [TestCase(4, 7, ExpectedResult = 3u)]
    [TestCase(17, 21, ExpectedResult = 0u)]
    public uint GetItemsCountInInterval(int start, int stop)
    {
        Chain<int> chain = [5, 4, 7, 1, 2, 3];
        
        return chain.CountInInterval(start, stop);
    }

    [TestCaseSource(nameof(_additionCaseSource))]
    public void AdditionTest(int item)
    {
        Chain<int> srcChain = [5, 4, 7, 2, 1, 3];
        Chain<int> updatedChain = [5, 4, 7, 2, 1, 3];
        
        updatedChain.Add(item);
        
        Assert.That(updatedChain, Has.Count.EqualTo(srcChain.Count + 1));
        Assert.That(updatedChain, Contains.Item(item));
        srcChain
            .Where(i => i <= item)
            .ForEach(i =>
            {
                uint largerElementsCount = updatedChain.CountInInterval(i, int.MaxValue);
                uint oldLargerElementsCount = srcChain.CountInInterval(i, int.MaxValue);
                Assert.That(largerElementsCount, Is.EqualTo(oldLargerElementsCount + 1));
            });
    }
    
    private static IEnumerable<int> _additionCaseSource = Enumerable.Range(0, 8);
}