namespace Mesch.RealTree.Tests;

public class RealTreeListContainerTests
{
    private readonly IRealTreeFactory _factory;
    private readonly IRealTreeOperations _operations;

    public RealTreeListContainerTests()
    {
        _factory = new RealTreeFactory();
        _operations = new RealTreeOperations(_factory);
    }

    [Fact]
    public async Task ListAsync_ReturnsDirectChildren()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var child1 = await _operations.AddContainerAsync(container, null, "Child1");
        var child2 = await _operations.AddContainerAsync(container, null, "Child2");
        var item1 = await _operations.AddItemAsync(container, null, "Item1");

        var result = await _operations.ListAsync(container);

        Assert.Equal(3, result.Count);
        Assert.Contains(child1, result);
        Assert.Contains(child2, result);
        Assert.Contains(item1, result);
    }

    [Fact]
    public async Task ListAsync_IncludeContainersOnly()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var child1 = await _operations.AddContainerAsync(container, null, "Child1");
        var item1 = await _operations.AddItemAsync(container, null, "Item1");

        var result = await _operations.ListAsync(container, includeContainers: true, includeItems: false);

        Assert.Single(result);
        Assert.Contains(child1, result);
        Assert.DoesNotContain(item1, result);
    }

    [Fact]
    public async Task ListAsync_IncludeItemsOnly()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var child1 = await _operations.AddContainerAsync(container, null, "Child1");
        var item1 = await _operations.AddItemAsync(container, null, "Item1");

        var result = await _operations.ListAsync(container, includeContainers: false, includeItems: true);

        Assert.Single(result);
        Assert.DoesNotContain(child1, result);
        Assert.Contains(item1, result);
    }

    [Fact]
    public async Task ListAsync_Recursive()
    {
        var tree = _factory.CreateTree();
        var root = await _operations.AddContainerAsync(tree, null, "Root");
        var child1 = await _operations.AddContainerAsync(root, null, "Child1");
        var grandchild = await _operations.AddContainerAsync(child1, null, "Grandchild");
        var item1 = await _operations.AddItemAsync(root, null, "Item1");
        var item2 = await _operations.AddItemAsync(child1, null, "Item2");

        var result = await _operations.ListAsync(root, recursive: true);

        Assert.Equal(4, result.Count);
        Assert.Contains(child1, result);
        Assert.Contains(grandchild, result);
        Assert.Contains(item1, result);
        Assert.Contains(item2, result);
    }

    [Fact]
    public async Task ListContainerAction_ExecutesBeforeList()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        await _operations.AddContainerAsync(container, null, "Child1");
        var actionExecuted = false;

        _operations.RegisterListAction<RealTreeContainer>(async (ctx, next) =>
        {
            actionExecuted = true;
            Assert.Same(container, ctx.Container);
            await next();
        });

        await _operations.ListAsync(container);

        Assert.True(actionExecuted);
    }

    [Fact]
    public async Task ListContainerAction_ThrowingException_CancelsQuery()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        await _operations.AddContainerAsync(container, null, "Child1");

        _operations.RegisterListAction<RealTreeContainer>(async (ctx, next) =>
        {
            // Throw exception to cancel the query
            throw new InvalidOperationException("Query cancelled by middleware");
        });

        // Exception cancels the query
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _operations.ListAsync(container));
    }

    [Fact]
    public async Task ListContainerAction_TypeBasedExecution()
    {
        var tree = _factory.CreateTree();
        var parent = await _operations.AddContainerAsync(tree, null, "Parent");
        var rootActionExecuted = false;
        var containerActionExecuted = false;

        _operations.RegisterListAction<RealTreeRoot>(async (ctx, next) =>
        {
            rootActionExecuted = true;
            await next();
        });

        _operations.RegisterListAction<RealTreeContainer>(async (ctx, next) =>
        {
            containerActionExecuted = true;
            await next();
        });

        // List the parent container - should only fire RealTreeContainer middleware
        await _operations.ListAsync(parent);

        Assert.False(rootActionExecuted);
        Assert.True(containerActionExecuted);
    }

    [Fact]
    public async Task ListAsync_WithoutTriggeringActions()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        await _operations.AddContainerAsync(container, null, "Child1");
        var actionExecuted = false;

        _operations.RegisterListAction<RealTreeContainer>(async (ctx, next) =>
        {
            actionExecuted = true;
            await next();
        });

        var result = await _operations.ListAsync(container, triggerActions: false);

        Assert.False(actionExecuted);
        Assert.Single(result);
    }

    [Fact]
    public async Task ListAsync_RecursiveWithItems()
    {
        var tree = _factory.CreateTree();
        var root = await _operations.AddContainerAsync(tree, null, "Root");
        var item = await _operations.AddItemAsync(root, null, "Item");
        var containerInItem = await _operations.AddContainerAsync(item, null, "ContainerInItem");

        var result = await _operations.ListAsync(root, recursive: true);

        Assert.Equal(2, result.Count);
        Assert.Contains(item, result);
        Assert.Contains(containerInItem, result);
    }

    [Fact]
    public async Task ListAsync_EmptyContainer()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Empty");

        var result = await _operations.ListAsync(container);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ListAsync_RecursiveExcludeContainers()
    {
        var tree = _factory.CreateTree();
        var root = await _operations.AddContainerAsync(tree, null, "Root");
        var child = await _operations.AddContainerAsync(root, null, "Child");
        var item1 = await _operations.AddItemAsync(root, null, "Item1");
        var item2 = await _operations.AddItemAsync(child, null, "Item2");

        var result = await _operations.ListAsync(root, includeContainers: false, includeItems: true, recursive: true);

        Assert.Equal(2, result.Count);
        Assert.Contains(item1, result);
        Assert.Contains(item2, result);
        Assert.DoesNotContain(child, result);
    }
}
