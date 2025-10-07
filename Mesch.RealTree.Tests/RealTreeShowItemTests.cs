namespace Mesch.RealTree.Tests;

public class RealTreeShowItemTests
{
    private readonly IRealTreeFactory _factory;
    private readonly IRealTreeOperations _operations;

    public RealTreeShowItemTests()
    {
        _factory = new RealTreeFactory();
        _operations = new RealTreeOperations(_factory);
    }

    [Fact]
    public async Task ShowItemAsync_ExecutesSuccessfully()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var item = await _operations.AddItemAsync(container, null, "TestItem");

        // Should complete without throwing
        await _operations.ShowItemAsync(item);
    }

    [Fact]
    public async Task ShowItemAction_ExecutesBeforeShow()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var item = await _operations.AddItemAsync(container, null, "TestItem");
        var actionExecuted = false;

        item.RegisterShowItemAction(async (ctx, next) =>
        {
            actionExecuted = true;
            Assert.Same(item, ctx.Item);
            Assert.NotNull(ctx.ShowMetadata);
            await next();
        });

        await _operations.ShowItemAsync(item);

        Assert.True(actionExecuted);
    }

    [Fact]
    public async Task ShowItemAction_CanModifyMetadata()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var item = await _operations.AddItemAsync(container, null, "TestItem");
        var metadataSet = false;

        // First registered action (executes second due to pipeline reversal)
        item.RegisterShowItemAction(async (ctx, next) =>
        {
            if (ctx.ShowMetadata.ContainsKey("renderMode"))
            {
                metadataSet = true;
                Assert.Equal("detailed", ctx.ShowMetadata["renderMode"]);
            }
            await next();
        });

        // Second registered action (executes first due to pipeline reversal)
        item.RegisterShowItemAction(async (ctx, next) =>
        {
            ctx.ShowMetadata["renderMode"] = "detailed";
            await next();
        });

        await _operations.ShowItemAsync(item);

        Assert.True(metadataSet);
    }

    [Fact]
    public async Task ShowItemAction_CanCancel()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var item = await _operations.AddItemAsync(container, null, "TestItem");
        var firstActionCalled = false;

        // First registered action (never executes because second cancels)
        item.RegisterShowItemAction(async (ctx, next) =>
        {
            firstActionCalled = true;
            await next();
        });

        // Second registered action (executes first, cancels by not calling next())
        item.RegisterShowItemAction(async (ctx, next) =>
        {
            // Don't call next() - cancels the operation
            await Task.CompletedTask;
        });

        await _operations.ShowItemAsync(item);

        // First action should not be called because second didn't call next()
        Assert.False(firstActionCalled);
    }

    [Fact]
    public async Task ShowItemAction_CanThrowException()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var item = await _operations.AddItemAsync(container, null, "TestItem");

        item.RegisterShowItemAction(async (ctx, next) =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Cannot show this item");
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => _operations.ShowItemAsync(item));
    }

    [Fact]
    public async Task ItemShownEvent_ExecutesAfterShow()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var item = await _operations.AddItemAsync(container, null, "TestItem");
        var tcs = new TaskCompletionSource<bool>();
        ShowItemContext? capturedContext = null;

        item.RegisterItemShownEvent(async (ctx, ct) =>
        {
            capturedContext = ctx;
            Assert.Same(item, ctx.Item);
            tcs.TrySetResult(true);
            await Task.CompletedTask;
        });

        await _operations.ShowItemAsync(item);

        // Wait for event with timeout
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(200));
        Assert.Same(tcs.Task, completed);
        Assert.NotNull(capturedContext);
        Assert.Same(item, capturedContext.Item);
    }

    [Fact]
    public async Task ShowItemAction_HierarchicalExecution()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var item = await _operations.AddItemAsync(container, null, "TestItem");

        var executionOrder = new List<string>();

        // ShowItemAction is only available on IRealTreeItem, not tree/container
        // This test verifies that actions registered on the item execute properly
        item.RegisterShowItemAction(async (ctx, next) =>
        {
            executionOrder.Add("item-action-1");
            await next();
        });

        item.RegisterShowItemAction(async (ctx, next) =>
        {
            executionOrder.Add("item-action-2");
            await next();
        });

        await _operations.ShowItemAsync(item);

        // Actions execute in reverse registration order (pipeline reverses the collection)
        Assert.Equal(new[] { "item-action-2", "item-action-1" }, executionOrder);
    }

    [Fact]
    public async Task ShowItemAsync_WithoutTriggeringActions()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var item = await _operations.AddItemAsync(container, null, "TestItem");
        var actionExecuted = false;

        item.RegisterShowItemAction(async (ctx, next) =>
        {
            actionExecuted = true;
            await next();
        });

        await _operations.ShowItemAsync(item, triggerActions: false);

        Assert.False(actionExecuted);
    }

    [Fact]
    public async Task ShowItemAsync_WithoutTriggeringEvents()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var item = await _operations.AddItemAsync(container, null, "TestItem");
        var eventExecuted = false;

        item.RegisterItemShownEvent(async (ctx, ct) =>
        {
            eventExecuted = true;
            await Task.CompletedTask;
        });

        await _operations.ShowItemAsync(item, triggerEvents: false);

        await Task.Delay(50); // Give event time to fire (if it was going to)
        Assert.False(eventExecuted);
    }

    [Fact]
    public async Task ShowItemEvent_MultipleHandlers()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var item = await _operations.AddItemAsync(container, null, "TestItem");

        var tcs1 = new TaskCompletionSource<bool>();
        var tcs2 = new TaskCompletionSource<bool>();

        item.RegisterItemShownEvent(async (ctx, ct) =>
        {
            tcs1.TrySetResult(true);
            await Task.CompletedTask;
        });

        item.RegisterItemShownEvent(async (ctx, ct) =>
        {
            tcs2.TrySetResult(true);
            await Task.CompletedTask;
        });

        await _operations.ShowItemAsync(item);

        // Both events should execute (in parallel)
        var completed1 = await Task.WhenAny(tcs1.Task, Task.Delay(200));
        var completed2 = await Task.WhenAny(tcs2.Task, Task.Delay(200));

        Assert.Same(tcs1.Task, completed1);
        Assert.Same(tcs2.Task, completed2);
    }

    [Fact]
    public async Task ShowItemAction_MultipleActions()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var item = await _operations.AddItemAsync(container, null, "TestItem");

        var executionOrder = new List<int>();

        item.RegisterShowItemAction(async (ctx, next) =>
        {
            executionOrder.Add(1);
            await next();
        });

        item.RegisterShowItemAction(async (ctx, next) =>
        {
            executionOrder.Add(2);
            await next();
        });

        item.RegisterShowItemAction(async (ctx, next) =>
        {
            executionOrder.Add(3);
            await next();
        });

        await _operations.ShowItemAsync(item);

        // Actions execute in reverse registration order (pipeline reverses collection)
        Assert.Equal(new[] { 3, 2, 1 }, executionOrder);
    }

    [Fact]
    public async Task ShowItemAction_CanDeregister()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var item = await _operations.AddItemAsync(container, null, "TestItem");
        var actionExecuted = false;

        ShowItemDelegate action = async (ctx, next) =>
        {
            actionExecuted = true;
            await next();
        };

        item.RegisterShowItemAction(action);
        var removed = item.DeregisterShowItemAction(action);

        await _operations.ShowItemAsync(item);

        Assert.True(removed);
        Assert.False(actionExecuted);
    }

    [Fact]
    public async Task ItemShownEvent_CanDeregister()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var item = await _operations.AddItemAsync(container, null, "TestItem");
        var eventExecuted = false;

        ItemShownEventDelegate eventHandler = async (ctx, ct) =>
        {
            eventExecuted = true;
            await Task.CompletedTask;
        };

        item.RegisterItemShownEvent(eventHandler);
        var removed = item.DeregisterItemShownEvent(eventHandler);

        await _operations.ShowItemAsync(item);

        await Task.Delay(50); // Give event time to fire (if it was going to)
        Assert.True(removed);
        Assert.False(eventExecuted);
    }
}
