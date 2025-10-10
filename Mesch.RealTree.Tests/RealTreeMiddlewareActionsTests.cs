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

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
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

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
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

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            ctx.Container.Metadata["processed"] = true;
            await next();
        });

        var container = await _operations.AddContainerAsync(tree, null, "Test");

        Assert.True((bool)container.Metadata["processed"]);
    }

    [Fact]
    public async Task AddContainerAction_ExecutesInRegistrationOrder()
    {
        var tree = _factory.CreateTree();
        var order = new List<int>();

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            order.Add(1);
            await next();
        });

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            order.Add(2);
            await next();
        });

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            order.Add(3);
            await next();
        });

        await _operations.AddContainerAsync(tree, null, "Test");

        // Actions execute in registration order
        Assert.Equal(new[] { 1, 2, 3 }, order);
    }

    [Fact]
    public async Task AddContainerAction_FiresBasedOnParentType()
    {
        var tree = _factory.CreateTree();
        var parent = await _operations.AddContainerAsync(tree, null, "Parent");
        var treeActionFired = false;
        var containerActionFired = false;

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            treeActionFired = true;
            await next();
        });

        _operations.RegisterAddContainerAction<RealTreeContainer>(async (ctx, next) =>
        {
            containerActionFired = true;
            await next();
        });

        await _operations.AddContainerAsync(parent, null, "Child");

        Assert.False(treeActionFired); // Parent is RealTreeContainer, not RealTreeRoot
        Assert.True(containerActionFired);
    }

    [Fact]
    public async Task AddItemAction_ExecutesBeforeAdd()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var executionOrder = new List<string>();

        _operations.RegisterAddItemAction<RealTreeContainer>(async (ctx, next) =>
        {
            executionOrder.Add("before");
            await next();
            executionOrder.Add("after");
        });

        await _operations.AddItemAsync(container, null, "Test");

        Assert.Equal(new[] { "before", "after" }, executionOrder);
    }

    [Fact]
    public async Task AddItemAction_FiresBasedOnParentType()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var executed = false;

        _operations.RegisterAddItemAction<RealTreeContainer>(async (ctx, next) =>
        {
            executed = true;
            await next();
        });

        await _operations.AddItemAsync(container, null, "Item");

        Assert.True(executed);
    }

    [Fact]
    public async Task RemoveContainerAction_ExecutesBeforeRemove()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Test");
        var executed = false;

        _operations.RegisterRemoveContainerAction<RealTreeRoot>(async (ctx, next) =>
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

        _operations.RegisterRemoveItemAction<RealTreeContainer>(async (ctx, next) =>
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
        string? capturedOldName = null;
        string? capturedNewName = null;

        _operations.RegisterUpdateAction<RealTreeContainer>(async (ctx, next) =>
        {
            capturedOldName = ctx.OldName;
            capturedNewName = ctx.NewName;
            await next();
        });

        await _operations.UpdateAsync(container, newName: "NewName");

        Assert.Equal("OldName", capturedOldName);
        Assert.Equal("NewName", capturedNewName);
    }

    // TODO: Implement Move functionality
    // [Fact]
    // public async Task MoveAction_ExecutesBeforeMove()
    // {
    //     var tree = _factory.CreateTree();
    //     var container1 = await _operations.AddContainerAsync(tree, null, "C1");
    //     var container2 = await _operations.AddContainerAsync(tree, null, "C2");
    //     var item = await _operations.AddItemAsync(container1, null, "Item");
    //     IRealTreeNode? capturedOldParent = null;
    //     IRealTreeNode? capturedNewParent = null;
    //
    //     _operations.RegisterMoveAction<RealTreeItem>(async (ctx, next) =>
    //     {
    //         capturedOldParent = ctx.OldParent;
    //         capturedNewParent = ctx.NewParent;
    //         await next();
    //     });
    //
    //     await _operations.MoveAsync(item, container2);
    //
    //     Assert.Same(container1, capturedOldParent);
    //     Assert.Same(container2, capturedNewParent);
    // }

    [Fact]
    public async Task BulkAddContainerAction_ExecutesForBulkOperation()
    {
        var tree = _factory.CreateTree();
        var executionCount = 0;

        _operations.RegisterBulkAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            executionCount++;
            await next();
        });

        var containersData = new[]
        {
            (id: (Guid?)null, name: "C1", metadata: (IDictionary<string, object>?)null),
            (id: (Guid?)null, name: "C2", metadata: (IDictionary<string, object>?)null),
            (id: (Guid?)null, name: "C3", metadata: (IDictionary<string, object>?)null)
        };

        await _operations.BulkAddContainersAsync<RealTreeContainer>(tree, containersData);

        Assert.Equal(1, executionCount);
    }

    [Fact]
    public async Task BulkAddItemAction_ExecutesForBulkOperation()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var executionCount = 0;

        _operations.RegisterBulkAddItemAction<RealTreeContainer>(async (ctx, next) =>
        {
            executionCount++;
            await next();
        });

        var itemsData = new[]
        {
            (id: (Guid?)null, name: "I1", metadata: (IDictionary<string, object>?)null),
            (id: (Guid?)null, name: "I2", metadata: (IDictionary<string, object>?)null)
        };

        await _operations.BulkAddItemsAsync<RealTreeItem>(container, itemsData);

        Assert.Equal(1, executionCount);
    }

    [Fact]
    public async Task BulkRemoveAction_ExecutesForBulkOperation()
    {
        var tree = _factory.CreateTree();
        var c1 = await _operations.AddContainerAsync(tree, null, "C1");
        var c2 = await _operations.AddContainerAsync(tree, null, "C2");
        var executionCount = 0;

        _operations.RegisterBulkRemoveAction<RealTreeRoot>(async (ctx, next) =>
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

        _operations.RegisterAddContainerAction<RealTreeRoot>(action);
        var removed = _operations.DeregisterAddContainerAction<RealTreeRoot>(action);

        await _operations.AddContainerAsync(tree, null, "Test");

        Assert.True(removed);
        Assert.False(executed);
    }

    [Fact]
    public void DeregisterAction_ReturnsFalseForNonExistentAction()
    {
        var tree = _factory.CreateTree();

        AddContainerDelegate action = async (ctx, next) => await next();
        var removed = _operations.DeregisterAddContainerAction<RealTreeRoot>(action);

        Assert.False(removed);
    }

    [Fact]
    public async Task Actions_CanAccessCancellationToken()
    {
        var tree = _factory.CreateTree();
        var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            capturedToken = ctx.CancellationToken;
            await next();
        });

        await _operations.AddContainerAsync(tree, null, "Test", cancellationToken: cts.Token);

        Assert.Equal(cts.Token, capturedToken);
    }

    [Fact]
    public async Task Actions_CanAccessTree()
    {
        var tree = _factory.CreateTree();
        IRealTree? capturedTree = null;

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            capturedTree = ctx.Tree;
            await next();
        });

        await _operations.AddContainerAsync(tree, null, "Test");

        Assert.Same(tree, capturedTree);
    }

    [Fact]
    public async Task Action_ThrowingException_CancelsOperation()
    {
        var tree = _factory.CreateTree();

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            // Throw exception to cancel the operation
            throw new InvalidOperationException("Operation cancelled by middleware");
        });

        // Exception cancels the operation
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _operations.AddContainerAsync(tree, null, "Test"));

        // Operation was cancelled, container should not be added
        Assert.Empty(tree.Containers);
    }

    [Fact]
    public async Task MultipleActions_CanFormPipeline()
    {
        var tree = _factory.CreateTree();
        var log = new List<string>();

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            log.Add("Action1-Before");
            await next();
            log.Add("Action1-After");
        });

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            log.Add("Action2-Before");
            await next();
            log.Add("Action2-After");
        });

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            log.Add("Action3-Before");
            await next();
            log.Add("Action3-After");
        });

        await _operations.AddContainerAsync(tree, null, "Test");

        // Actions execute in registration order
        Assert.Equal(new[]
        {
            "Action1-Before",
            "Action2-Before",
            "Action3-Before",
            "Action3-After",
            "Action2-After",
            "Action1-After"
        }, log);
    }

    [Fact]
    public async Task TypeBasedMiddleware_OnlyFiresForMatchingTypes()
    {
        var tree = _factory.CreateTree();
        var container1Fired = false;
        var container2Fired = false;

        // Create custom container types
        var customContainer = new CustomTestContainer();

        _operations.RegisterAddItemAction<CustomTestContainer>(async (ctx, next) =>
        {
            container1Fired = true;
            await next();
        });

        _operations.RegisterAddItemAction<RealTreeContainer>(async (ctx, next) =>
        {
            container2Fired = true;
            await next();
        });

        // Add custom container to tree first
        tree.AddContainer(customContainer);

        // Add item to custom container - should only fire CustomTestContainer middleware
        await _operations.AddItemAsync(customContainer, null, "Item");

        Assert.True(container1Fired);
        Assert.False(container2Fired);
    }

    [Fact]
    public async Task MetadataFirst_MiddlewareCanAccessMetadata()
    {
        var tree = _factory.CreateTree();
        string? capturedSubdomain = null;

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            // Metadata is already set before middleware executes
            capturedSubdomain = ctx.Container.Metadata.ContainsKey("subdomain")
                ? ctx.Container.Metadata["subdomain"].ToString()
                : null;
            await next();
        });

        var metadata = new Dictionary<string, object> { ["subdomain"] = "test-subdomain" };
        await _operations.AddContainerAsync(tree, null, "Container", metadata);

        Assert.Equal("test-subdomain", capturedSubdomain);
    }
}

// Custom container for testing type-based middleware
public class CustomTestContainer : RealTreeContainer
{
    public CustomTestContainer(Guid? id = null, string? name = null) : base(id, name)
    {
    }
}
