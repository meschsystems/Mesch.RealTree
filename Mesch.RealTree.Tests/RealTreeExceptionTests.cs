using Xunit.Abstractions;

namespace Mesch.RealTree.Tests;
public class RealTreeExceptionTests
{
    private readonly ITestOutputHelper _output;
    private readonly IRealTreeFactory _factory;
    private readonly IRealTreeOperations _operations;

    public RealTreeExceptionTests(ITestOutputHelper output)
    {
        _output = output;
        _factory = new RealTreeFactory();
        _operations = new RealTreeOperations(_factory);
    }

    [Fact]
    public async Task CyclicReferenceException_ThrownWhenAddingAncestor()
    {
        var tree = _factory.CreateTree();
        var parent = await _operations.AddContainerAsync(tree, null, "Parent");
        var child = await _operations.AddContainerAsync(parent, null, "Child");

        // Try to add parent as a child of child - this creates a cycle
        await Assert.ThrowsAsync<CyclicReferenceException>(
            async () => child.AddContainer(parent));
    }

    [Fact]
    public async Task CyclicReferenceException_ThrownWhenMovingToDescendant()
    {
        var tree = _factory.CreateTree();
        var parent = await _operations.AddContainerAsync(tree, null, "Parent");
        var child = await _operations.AddContainerAsync(parent, null, "Child");
        var grandchild = await _operations.AddContainerAsync(child, null, "Grandchild");

        await Assert.ThrowsAsync<CyclicReferenceException>(
            async () => await _operations.MoveAsync(parent, grandchild));
    }

    [Fact]
    public async Task CyclicReferenceException_ThrownWhenMovingToSelf()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");

        await Assert.ThrowsAsync<CyclicReferenceException>(
            async () => await _operations.MoveAsync(container, container));
    }

    [Fact]
    public async Task InvalidOperationException_ThrownWhenRemovingRootWithoutParent()
    {
        var tree = _factory.CreateTree();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _operations.RemoveAsync(tree));
    }

    [Fact]
    public void ArgumentNullException_ThrownWhenFactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new RealTreeOperations(null));
    }

    [Fact]
    public void ArgumentException_ThrownWhenSettingEmptyName()
    {
        var container = _factory.CreateContainer(name: "Test");

        Assert.Throws<ArgumentException>(() => container.Name = "");
    }

    [Fact]
    public void InvalidOperationException_ThrownWhenAccessingTreeOnDetachedNode()
    {
        var container = _factory.CreateContainer();

        Assert.Throws<InvalidOperationException>(() => container.Tree);
    }

    [Fact]
    public void InvalidOperationException_ThrownWhenAccessingTreeOnDetachedItem()
    {
        var item = _factory.CreateItem();

        Assert.Throws<InvalidOperationException>(() => item.Tree);
    }

    [Fact]
    public async Task TreeValidationException_IsBaseForValidationErrors()
    {
        var tree = _factory.CreateTree();
        var parent = await _operations.AddContainerAsync(tree, null, "Parent");
        var child = await _operations.AddContainerAsync(parent, null, "Child");

        try
        {
            // Try to add parent to its own child - creates cycle
            child.AddContainer(parent);
            Assert.Fail("Expected exception");
        }
        catch (Exception ex)
        {
            Assert.IsAssignableFrom<TreeValidationException>(ex);
        }
    }
}