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
    public async Task ListContainerAsync_ReturnsDirectChildren()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var child1 = await _operations.AddContainerAsync(container, null, "Child1");
        var child2 = await _operations.AddContainerAsync(container, null, "Child2");
        var item1 = await _operations.AddItemAsync(container, null, "Item1");

        var result = await _operations.ListContainerAsync(container);

        Assert.Equal(3, result.Count);
        Assert.Contains(child1, result);
        Assert.Contains(child2, result);
        Assert.Contains(item1, result);
    }

    [Fact]
    public async Task ListContainerAsync_IncludeContainersOnly()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var child1 = await _operations.AddContainerAsync(container, null, "Child1");
        var item1 = await _operations.AddItemAsync(container, null, "Item1");

        var result = await _operations.ListContainerAsync(container, includeContainers: true, includeItems: false);

        Assert.Single(result);
        Assert.Contains(child1, result);
        Assert.DoesNotContain(item1, result);
    }

    [Fact]
    public async Task ListContainerAsync_IncludeItemsOnly()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var child1 = await _operations.AddContainerAsync(container, null, "Child1");
        var item1 = await _operations.AddItemAsync(container, null, "Item1");

        var result = await _operations.ListContainerAsync(container, includeContainers: false, includeItems: true);

        Assert.Single(result);
        Assert.DoesNotContain(child1, result);
        Assert.Contains(item1, result);
    }

    [Fact]
    public async Task ListContainerAsync_Recursive()
    {
        var tree = _factory.CreateTree();
        var root = await _operations.AddContainerAsync(tree, null, "Root");
        var child1 = await _operations.AddContainerAsync(root, null, "Child1");
        var grandchild = await _operations.AddContainerAsync(child1, null, "Grandchild");
        var item1 = await _operations.AddItemAsync(root, null, "Item1");
        var item2 = await _operations.AddItemAsync(child1, null, "Item2");

        var result = await _operations.ListContainerAsync(root, recursive: true);

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

        container.RegisterListContainerAction(async (ctx, next) =>
        {
            actionExecuted = true;
            Assert.Same(container, ctx.Container);
            Assert.True(ctx.IncludeContainers);
            Assert.True(ctx.IncludeItems);
            Assert.False(ctx.Recursive);
            await next();
        });

        await _operations.ListContainerAsync(container);

        Assert.True(actionExecuted);
    }

    [Fact]
    public async Task ListContainerAction_CanModifyListingBehavior()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        await _operations.AddContainerAsync(container, null, "Child1");
        await _operations.AddItemAsync(container, null, "Item1");

        // Middleware that forces items-only listing
        container.RegisterListContainerAction(async (ctx, next) =>
        {
            ctx.IncludeContainers = false;
            ctx.IncludeItems = true;
            await next();
        });

        var result = await _operations.ListContainerAsync(container, includeContainers: true, includeItems: true);

        // Should only have item, not container, because middleware modified it
        Assert.Single(result);
        Assert.All(result, node => Assert.IsAssignableFrom<IRealTreeItem>(node));
    }

    [Fact]
    public async Task ListContainerAction_CanCancel()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        await _operations.AddContainerAsync(container, null, "Child1");

        container.RegisterListContainerAction(async (ctx, next) =>
        {
            // Don't call next() - cancels the operation
            await Task.CompletedTask;
        });

        var result = await _operations.ListContainerAsync(container);

        // Should be empty because middleware didn't call next()
        Assert.Empty(result);
    }

    [Fact]
    public async Task ContainerListedEvent_ExecutesAfterList()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var child = await _operations.AddContainerAsync(container, null, "Child1");
        var tcs = new TaskCompletionSource<bool>();
        IReadOnlyList<IRealTreeNode>? capturedResult = null;

        container.RegisterContainerListedEvent(async (ctx, result, ct) =>
        {
            capturedResult = result;
            Assert.Same(container, ctx.Container);
            tcs.TrySetResult(true);
            await Task.CompletedTask;
        });

        var listResult = await _operations.ListContainerAsync(container);

        // Wait for event with timeout
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(200));
        Assert.Same(tcs.Task, completed);
        Assert.NotNull(capturedResult);
        Assert.Single(capturedResult);
        Assert.Contains(child, capturedResult);
    }

    [Fact]
    public async Task ListContainerAction_HierarchicalExecution()
    {
        var tree = _factory.CreateTree();
        var parent = await _operations.AddContainerAsync(tree, null, "Parent");
        var child = await _operations.AddContainerAsync(parent, null, "Child");

        var executionOrder = new List<string>();

        tree.RegisterListContainerAction(async (ctx, next) =>
        {
            executionOrder.Add("tree-before");
            await next();
            executionOrder.Add("tree-after");
        });

        parent.RegisterListContainerAction(async (ctx, next) =>
        {
            executionOrder.Add("parent-before");
            await next();
            executionOrder.Add("parent-after");
        });

        child.RegisterListContainerAction(async (ctx, next) =>
        {
            executionOrder.Add("child-before");
            await next();
            executionOrder.Add("child-after");
        });

        await _operations.ListContainerAsync(child);

        // Actions execute from tree down to child (bottom-up collection, but pipeline reverses order)
        Assert.Equal(new[] { "tree-before", "parent-before", "child-before", "child-after", "parent-after", "tree-after" }, executionOrder);
    }

    [Fact]
    public async Task ListContainerAsync_WithoutTriggeringActions()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        await _operations.AddContainerAsync(container, null, "Child1");
        var actionExecuted = false;

        container.RegisterListContainerAction(async (ctx, next) =>
        {
            actionExecuted = true;
            await next();
        });

        var result = await _operations.ListContainerAsync(container, triggerActions: false);

        Assert.False(actionExecuted);
        Assert.Single(result);
    }

    [Fact]
    public async Task ListContainerAsync_WithoutTriggeringEvents()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        await _operations.AddContainerAsync(container, null, "Child1");
        var eventExecuted = false;

        container.RegisterContainerListedEvent(async (ctx, result, ct) =>
        {
            eventExecuted = true;
            await Task.CompletedTask;
        });

        var result = await _operations.ListContainerAsync(container, triggerEvents: false);

        await Task.Delay(50); // Give event time to fire (if it was going to)
        Assert.False(eventExecuted);
        Assert.Single(result);
    }
}
