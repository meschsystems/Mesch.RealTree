using Xunit.Abstractions;

namespace Mesch.RealTree.Tests;
public class RealTreeContextTests
{
    private readonly ITestOutputHelper _output;
    private readonly IRealTreeFactory _factory;
    private readonly IRealTreeOperations _operations;

    public RealTreeContextTests(ITestOutputHelper output)
    {
        _output = output;
        _factory = new RealTreeFactory();
        _operations = new RealTreeOperations(_factory);
    }

    [Fact]
    public async Task AddContainerContext_ProvidesCorrectInformation()
    {
        var tree = _factory.CreateTree();
        AddContainerContext capturedContext = null;

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            capturedContext = ctx;
            await next();
        });

        var container = await _operations.AddContainerAsync(tree, null, "Test");

        Assert.NotNull(capturedContext);
        Assert.Same(container, capturedContext.Container);
        Assert.Same(tree, capturedContext.Parent);
        Assert.Same(tree, capturedContext.Tree);
    }

    [Fact]
    public async Task AddItemContext_ProvidesCorrectInformation()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        AddItemContext capturedContext = null;

        container.RegisterAddItemAction(async (ctx, next) =>
        {
            capturedContext = ctx;
            await next();
        });

        var item = await _operations.AddItemAsync(container, null, "Test");

        Assert.NotNull(capturedContext);
        Assert.Same(item, capturedContext.Item);
        Assert.Same(container, capturedContext.Parent);
        Assert.Same(tree, capturedContext.Tree);
    }

    [Fact]
    public async Task RemoveContext_ProvidesCorrectInformation()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Test");
        RemoveContext capturedContext = null;

        tree.RegisterRemoveContainerAction(async (ctx, next) =>
        {
            capturedContext = ctx;
            await next();
        });

        await _operations.RemoveAsync(container);

        Assert.NotNull(capturedContext);
        Assert.Same(container, capturedContext.Node);
        Assert.Same(tree, capturedContext.Parent);
    }

    [Fact]
    public async Task UpdateContext_ProvidesOldAndNewValues()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "OldName");
        container.Metadata["oldKey"] = "oldValue";
        UpdateContext capturedContext = null;

        container.RegisterUpdateAction(async (ctx, next) =>
        {
            capturedContext = ctx;
            await next();
        });

        var newMetadata = new Dictionary<string, object> { { "newKey", "newValue" } };
        await _operations.UpdateAsync(container, "NewName", newMetadata);

        Assert.Equal("OldName", capturedContext.OldName);
        Assert.Equal("NewName", capturedContext.NewName);
        Assert.Contains("oldKey", capturedContext.OldMetadata.Keys);
        Assert.Contains("newKey", capturedContext.NewMetadata.Keys);
    }

    [Fact]
    public async Task MoveContext_ProvidesOldAndNewParents()
    {
        var tree = _factory.CreateTree();
        var c1 = await _operations.AddContainerAsync(tree, null, "C1");
        var c2 = await _operations.AddContainerAsync(tree, null, "C2");
        var item = await _operations.AddItemAsync(c1, null, "Item");
        MoveContext capturedContext = null;

        item.RegisterMoveAction(async (ctx, next) =>
        {
            capturedContext = ctx;
            await next();
        });

        await _operations.MoveAsync(item, c2);

        Assert.Same(c1, capturedContext.OldParent);
        Assert.Same(c2, capturedContext.NewParent);
    }

    [Fact]
    public async Task BulkAddContainerContext_ProvidesContainersList()
    {
        var tree = _factory.CreateTree();
        BulkAddContainerContext capturedContext = null;

        tree.RegisterBulkAddContainerAction(async (ctx, next) =>
        {
            capturedContext = ctx;
            await next();
        });

        var containers = new[]
        {
            _factory.CreateContainer(name: "C1"),
            _factory.CreateContainer(name: "C2")
        };

        await _operations.BulkAddContainersAsync(tree, containers);

        Assert.Equal(2, capturedContext.Containers.Count);
        Assert.Same(tree, capturedContext.Parent);
    }

    [Fact]
    public async Task OperationContext_HasTimestamp()
    {
        var tree = _factory.CreateTree();
        var before = DateTime.UtcNow;
        DateTime timestamp = default;

        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            timestamp = ctx.OperationTime;
            await next();
        });

        await _operations.AddContainerAsync(tree, null, "Test");
        var after = DateTime.UtcNow;

        Assert.InRange(timestamp, before, after);
    }
}