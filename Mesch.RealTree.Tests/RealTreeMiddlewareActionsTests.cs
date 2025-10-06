using Xunit.Abstractions;

namespace Mesch.RealTree.Tests;

public class RealTreeMiddlewareActionsTests
{
    private readonly ITestOutputHelper _output;
    private readonly IRealTreeFactory _factory;
    private readonly IRealTreeOperations _operations;

    public RealTreeMiddlewareActionsTests(ITestOutputHelper output)
    {
        _output = output;
        _factory = new RealTreeFactory();
        _operations = new RealTreeOperations(_factory);
    }

    [Fact]
    public async Task AddContainerAction_ExecutesBeforeAdd()
    {
        var tree = _factory.CreateTree();
        var executionOrder = new List<string>();

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            executionOrder.Add("before");
            await next();
            executionOrder.Add("after");
        });

        await _operations.AddContainerAsync(tree, null, "Test");

        Assert.Equal(new[] { "before", "after" }, executionOrder);
    }

    [Fact]
    public async Task AddContainerAction_CanCancelOperation()
    {
        var tree = _factory.CreateTree();

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            if (ctx.Container.Name.StartsWith("invalid"))
            {
                throw new InvalidOperationException("Invalid name");
            }
            await next();
        });

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _operations.AddContainerAsync(tree, null, "invalid_name"));

        Assert.Empty(tree.Containers);
    }

    [Fact]
    public async Task AddContainerAction_CanModifyContext()
    {
        var tree = _factory.CreateTree();

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            ctx.Container.Metadata["processed"] = true;
            await next();
        });

        var container = await _operations.AddContainerAsync(tree, null, "Test");

        Assert.True((bool)container.Metadata["processed"]);
    }

    [Fact]
    public async Task AddContainerAction_ExecutesInReverseRegistrationOrder()
    {
        var tree = _factory.CreateTree();
        var order = new List<int>();

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            order.Add(1);
            await next();
        });

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            order.Add(2);
            await next();
        });

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            order.Add(3);
            await next();
        });

        await _operations.AddContainerAsync(tree, null, "Test");

        // Actions execute in reverse order (middleware pattern)
        Assert.Equal(new[] { 3, 2, 1 }, order);
    }

    [Fact]
    public async Task AddContainerAction_PropagatesFromParentToChild()
    {
        var tree = _factory.CreateTree();
        var parent = await _operations.AddContainerAsync(tree, null, "Parent");
        var executed = false;

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            executed = true;
            await next();
        });

        await _operations.AddContainerAsync(parent, null, "Child");

        Assert.True(executed);
    }

    [Fact]
    public async Task AddItemAction_ExecutesBeforeAdd()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var executionOrder = new List<string>();

        container.RegisterAddItemAction(async (ctx, next) =>
        {
            executionOrder.Add("before");
            await next();
            executionOrder.Add("after");
        });

        await _operations.AddItemAsync(container, null, "Test");

        Assert.Equal(new[] { "before", "after" }, executionOrder);
    }

    [Fact]
    public async Task RemoveContainerAction_ExecutesBeforeRemove()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Test");
        var executed = false;

        tree.RegisterRemoveContainerAction(async (ctx, next) =>
        {
            executed = true;
            Assert.Same(container, ctx.Container);
            await next();
        });

        await _operations.RemoveAsync(container);

        Assert.True(executed);
    }

    [Fact]
    public async Task RemoveItemAction_ExecutesBeforeRemove()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var item = await _operations.AddItemAsync(container, null, "Item");
        var executed = false;

        container.RegisterRemoveItemAction(async (ctx, next) =>
        {
            executed = true;
            Assert.Same(item, ctx.Item);
            await next();
        });

        await _operations.RemoveAsync(item);

        Assert.True(executed);
    }

    [Fact]
    public async Task UpdateAction_ExecutesBeforeUpdate()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "OldName");
        string capturedOldName = null;
        string capturedNewName = null;

        container.RegisterUpdateAction(async (ctx, next) =>
        {
            capturedOldName = ctx.OldName;
            capturedNewName = ctx.NewName;
            await next();
        });

        await _operations.UpdateAsync(container, newName: "NewName");

        Assert.Equal("OldName", capturedOldName);
        Assert.Equal("NewName", capturedNewName);
    }

    [Fact]
    public async Task MoveAction_ExecutesBeforeMove()
    {
        var tree = _factory.CreateTree();
        var container1 = await _operations.AddContainerAsync(tree, null, "C1");
        var container2 = await _operations.AddContainerAsync(tree, null, "C2");
        var item = await _operations.AddItemAsync(container1, null, "Item");
        IRealTreeNode capturedOldParent = null;
        IRealTreeNode capturedNewParent = null;

        item.RegisterMoveAction(async (ctx, next) =>
        {
            capturedOldParent = ctx.OldParent;
            capturedNewParent = ctx.NewParent;
            await next();
        });

        await _operations.MoveAsync(item, container2);

        Assert.Same(container1, capturedOldParent);
        Assert.Same(container2, capturedNewParent);
    }

    [Fact]
    public async Task BulkAddContainerAction_ExecutesForBulkOperation()
    {
        var tree = _factory.CreateTree();
        var executionCount = 0;

        tree.RegisterBulkAddContainerAction(async (ctx, next) =>
        {
            executionCount++;
            Assert.Equal(3, ctx.Containers.Count);
            await next();
        });

        var containers = new[]
        {
            _factory.CreateContainer(name: "C1"),
            _factory.CreateContainer(name: "C2"),
            _factory.CreateContainer(name: "C3")
        };

        await _operations.BulkAddContainersAsync(tree, containers);

        Assert.Equal(1, executionCount);
    }

    [Fact]
    public async Task BulkAddItemAction_ExecutesForBulkOperation()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var executionCount = 0;

        container.RegisterBulkAddItemAction(async (ctx, next) =>
        {
            executionCount++;
            Assert.Equal(2, ctx.Items.Count);
            await next();
        });

        var items = new[]
        {
            _factory.CreateItem(name: "I1"),
            _factory.CreateItem(name: "I2")
        };

        await _operations.BulkAddItemsAsync(container, items);

        Assert.Equal(1, executionCount);
    }

    [Fact]
    public async Task BulkRemoveAction_ExecutesForBulkOperation()
    {
        var tree = _factory.CreateTree();
        var c1 = await _operations.AddContainerAsync(tree, null, "C1");
        var c2 = await _operations.AddContainerAsync(tree, null, "C2");
        var executionCount = 0;

        tree.RegisterBulkRemoveAction(async (ctx, next) =>
        {
            executionCount++;
            Assert.Equal(2, ctx.Nodes.Count);
            await next();
        });

        await _operations.BulkRemoveAsync(new[] { c1, c2 });

        Assert.Equal(1, executionCount);
    }

    [Fact]
    public async Task DeregisterAction_RemovesActionFromPipeline()
    {
        var tree = _factory.CreateTree();
        var executed = false;

        AddContainerDelegate action = async (ctx, next) =>
        {
            executed = true;
            await next();
        };

        tree.RegisterAddContainerAction(action);
        var removed = tree.DeregisterAddContainerAction(action);

        await _operations.AddContainerAsync(tree, null, "Test");

        Assert.True(removed);
        Assert.False(executed);
    }

    [Fact]
    public void DeregisterAction_ReturnsFalseForNonExistentAction()
    {
        var tree = _factory.CreateTree();

        AddContainerDelegate action = async (ctx, next) => await next();
        var removed = tree.DeregisterAddContainerAction(action);

        Assert.False(removed);
    }

    [Fact]
    public async Task Actions_CanAccessCancellationToken()
    {
        var tree = _factory.CreateTree();
        var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            capturedToken = ctx.CancellationToken;
            await next();
        });

        await _operations.AddContainerAsync(tree, null, "Test", cancellationToken: cts.Token);

        Assert.Equal(cts.Token, capturedToken);
    }

    [Fact]
    public async Task Actions_CanAccessOperationTime()
    {
        var tree = _factory.CreateTree();
        var beforeTime = DateTime.UtcNow;
        DateTime capturedTime = default;

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            capturedTime = ctx.OperationTime;
            await next();
        });

        await _operations.AddContainerAsync(tree, null, "Test");
        var afterTime = DateTime.UtcNow;

        Assert.InRange(capturedTime, beforeTime, afterTime);
    }

    [Fact]
    public async Task Actions_CanAccessTree()
    {
        var tree = _factory.CreateTree();
        IRealTree capturedTree = null;

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            capturedTree = ctx.Tree;
            await next();
        });

        await _operations.AddContainerAsync(tree, null, "Test");

        Assert.Same(tree, capturedTree);
    }

    [Fact]
    public async Task Action_WithoutCallingNext_BlocksOperation()
    {
        var tree = _factory.CreateTree();

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            // Don't call next - block the operation
            await Task.CompletedTask;
        });

        await _operations.AddContainerAsync(tree, null, "Test");

        // Operation was blocked, container should not be added
        Assert.Empty(tree.Containers);
    }

    [Fact]
    public async Task MultipleActions_CanFormPipeline()
    {
        var tree = _factory.CreateTree();
        var log = new List<string>();

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            log.Add("Action1-Before");
            await next();
            log.Add("Action1-After");
        });

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            log.Add("Action2-Before");
            await next();
            log.Add("Action2-After");
        });

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            log.Add("Action3-Before");
            await next();
            log.Add("Action3-After");
        });

        await _operations.AddContainerAsync(tree, null, "Test");

        // Actions execute in reverse registration order (last registered is outermost)
        Assert.Equal(new[]
        {
            "Action3-Before",
            "Action2-Before",
            "Action1-Before",
            "Action1-After",
            "Action2-After",
            "Action3-After"
        }, log);
    }
}