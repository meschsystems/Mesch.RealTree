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
        AddContainerContext? capturedContext = null;

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
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
        AddItemContext? capturedContext = null;

        _operations.RegisterAddItemAction<RealTreeContainer>(async (ctx, next) =>
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
    public async Task RemoveContainerContext_ProvidesCorrectInformation()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Test");
        RemoveContainerContext? capturedContext = null;

        _operations.RegisterRemoveContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            capturedContext = ctx;
            await next();
        });

        await _operations.RemoveAsync(container);

        Assert.NotNull(capturedContext);
        Assert.Same(container, capturedContext.Container);
        Assert.Same(tree, capturedContext.Parent);
    }

    [Fact]
    public async Task RemoveItemContext_ProvidesCorrectInformation()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var item = await _operations.AddItemAsync(container, null, "Test");
        RemoveItemContext? capturedContext = null;

        _operations.RegisterRemoveItemAction<RealTreeContainer>(async (ctx, next) =>
        {
            capturedContext = ctx;
            await next();
        });

        await _operations.RemoveAsync(item);

        Assert.NotNull(capturedContext);
        Assert.Same(item, capturedContext.Item);
        Assert.Same(container, capturedContext.Parent);
    }

    [Fact]
    public async Task UpdateContext_ProvidesOldAndNewValues()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "OldName");
        container.Metadata["oldKey"] = "oldValue";
        UpdateContext? capturedContext = null;

        _operations.RegisterUpdateAction<RealTreeContainer>(async (ctx, next) =>
        {
            capturedContext = ctx;
            await next();
        });

        var newMetadata = new Dictionary<string, object> { { "newKey", "newValue" } };
        await _operations.UpdateAsync(container, "NewName", newMetadata);

        Assert.NotNull(capturedContext);
        Assert.Equal("OldName", capturedContext.OldName);
        Assert.Equal("NewName", capturedContext.NewName);
        Assert.NotNull(capturedContext.OldMetadata);
        Assert.NotNull(capturedContext.NewMetadata);
        Assert.Contains("oldKey", capturedContext.OldMetadata.Keys);
        Assert.Contains("newKey", capturedContext.NewMetadata.Keys);
    }

    // TODO: Implement Move functionality
    // [Fact]
    // public async Task MoveContext_ProvidesOldAndNewParents()
    // {
    //     var tree = _factory.CreateTree();
    //     var c1 = await _operations.AddContainerAsync(tree, null, "C1");
    //     var c2 = await _operations.AddContainerAsync(tree, null, "C2");
    //     var item = await _operations.AddItemAsync(c1, null, "Item");
    //     MoveContext? capturedContext = null;
    //
    //     _operations.RegisterMoveAction<RealTreeItem>(async (ctx, next) =>
    //     {
    //         capturedContext = ctx;
    //         await next();
    //     });
    //
    //     await _operations.MoveAsync(item, c2);
    //
    //     Assert.NotNull(capturedContext);
    //     Assert.Same(c1, capturedContext.OldParent);
    //     Assert.Same(c2, capturedContext.NewParent);
    // }

    [Fact]
    public async Task BulkAddContainerContext_ProvidesItemsList()
    {
        var tree = _factory.CreateTree();
        BulkAddContainerContext? capturedContext = null;

        _operations.RegisterBulkAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            capturedContext = ctx;
            await next();
        });

        var containersData = new[]
        {
            (id: (Guid?)null, name: "C1", metadata: (IDictionary<string, object>?)null),
            (id: (Guid?)null, name: "C2", metadata: (IDictionary<string, object>?)null)
        };

        await _operations.BulkAddContainersAsync<RealTreeContainer>(tree, containersData);

        Assert.NotNull(capturedContext);
        Assert.Same(tree, capturedContext.Parent);
    }

    [Fact]
    public async Task AddContainerContext_ParentAsContainer_ReturnsContainer()
    {
        var tree = _factory.CreateTree();
        AddContainerContext? capturedContext = null;

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            capturedContext = ctx;
            await next();
        });

        await _operations.AddContainerAsync(tree, null, "Test");

        Assert.NotNull(capturedContext);
        Assert.NotNull(capturedContext.ParentAsContainer);
        Assert.Same(tree, capturedContext.ParentAsContainer);
    }

    [Fact]
    public async Task AddContainerContext_ParentAsItem_ReturnsNullWhenParentIsContainer()
    {
        var tree = _factory.CreateTree();
        AddContainerContext? capturedContext = null;

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            capturedContext = ctx;
            await next();
        });

        await _operations.AddContainerAsync(tree, null, "Test");

        Assert.NotNull(capturedContext);
        Assert.Null(capturedContext.ParentAsItem);
    }

    [Fact]
    public async Task AddContainerContext_ParentAsItem_ReturnsItemWhenParentIsItem()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var item = await _operations.AddItemAsync(container, null, "Item");
        AddContainerContext? capturedContext = null;

        _operations.RegisterAddContainerAction<RealTreeItem>(async (ctx, next) =>
        {
            capturedContext = ctx;
            await next();
        });

        await _operations.AddContainerAsync(item, null, "SubContainer");

        Assert.NotNull(capturedContext);
        Assert.NotNull(capturedContext.ParentAsItem);
        Assert.Same(item, capturedContext.ParentAsItem);
    }

    [Fact]
    public async Task Context_CanAccessCancellationToken()
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
}
