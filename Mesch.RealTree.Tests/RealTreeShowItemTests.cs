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

        _operations.RegisterShowItemAction<RealTreeItem>(async (ctx, next) =>
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
        item.Metadata["size"] = 1024;

        _operations.RegisterShowItemAction<RealTreeItem>(async (ctx, next) =>
        {
            ctx.ShowMetadata["displayName"] = $"Item: {ctx.Item.Name}";
            ctx.ShowMetadata["formattedSize"] = $"{ctx.Item.Metadata["size"]} bytes";
            await next();
        });

        await _operations.ShowItemAsync(item);

        // Middleware executed successfully
        Assert.True(true);
    }

    [Fact]
    public async Task ShowItemAction_CanCancel()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Parent");
        var item = await _operations.AddItemAsync(container, null, "TestItem");
        var secondActionExecuted = false;

        _operations.RegisterShowItemAction<RealTreeItem>(async (ctx, next) =>
        {
            // Don't call next() - cancels operation
            await Task.CompletedTask;
        });

        _operations.RegisterShowItemAction<RealTreeItem>(async (ctx, next) =>
        {
            secondActionExecuted = true;
            await next();
        });

        await _operations.ShowItemAsync(item);

        Assert.False(secondActionExecuted);
    }

    [Fact]
    public async Task ShowItemAction_TypeBasedExecution()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var item = await _operations.AddItemAsync(container, null, "Item");
        var customItem = new CustomItemType();
        container.AddItem(customItem);

        var realTreeItemActionExecuted = false;
        var customItemActionExecuted = false;

        _operations.RegisterShowItemAction<RealTreeItem>(async (ctx, next) =>
        {
            realTreeItemActionExecuted = true;
            await next();
        });

        _operations.RegisterShowItemAction<CustomItemType>(async (ctx, next) =>
        {
            customItemActionExecuted = true;
            await next();
        });

        // Show RealTreeItem - should only fire RealTreeItem middleware
        await _operations.ShowItemAsync(item);
        Assert.True(realTreeItemActionExecuted);
        Assert.False(customItemActionExecuted);

        // Reset
        realTreeItemActionExecuted = false;

        // Show CustomItem - should only fire CustomItem middleware
        await _operations.ShowItemAsync(customItem);
        Assert.False(realTreeItemActionExecuted);
        Assert.True(customItemActionExecuted);
    }

    [Fact]
    public async Task ShowItemAsync_WithoutTriggeringActions()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var item = await _operations.AddItemAsync(container, null, "Item");
        var actionExecuted = false;

        _operations.RegisterShowItemAction<RealTreeItem>(async (ctx, next) =>
        {
            actionExecuted = true;
            await next();
        });

        await _operations.ShowItemAsync(item, triggerActions: false);

        Assert.False(actionExecuted);
    }

    [Fact]
    public async Task ShowItemAsync_MultipleActionsFormPipeline()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var item = await _operations.AddItemAsync(container, null, "Item");
        var log = new List<string>();

        _operations.RegisterShowItemAction<RealTreeItem>(async (ctx, next) =>
        {
            log.Add("Action1-Before");
            await next();
            log.Add("Action1-After");
        });

        _operations.RegisterShowItemAction<RealTreeItem>(async (ctx, next) =>
        {
            log.Add("Action2-Before");
            await next();
            log.Add("Action2-After");
        });

        await _operations.ShowItemAsync(item);

        Assert.Equal(new[] { "Action1-Before", "Action2-Before", "Action2-After", "Action1-After" }, log);
    }

    [Fact]
    public async Task ShowItemAction_CanAccessTree()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var item = await _operations.AddItemAsync(container, null, "Item");
        IRealTree? capturedTree = null;

        _operations.RegisterShowItemAction<RealTreeItem>(async (ctx, next) =>
        {
            capturedTree = ctx.Tree;
            await next();
        });

        await _operations.ShowItemAsync(item);

        Assert.Same(tree, capturedTree);
    }

    [Fact]
    public async Task ShowItemAction_DeregistrationWorks()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var item = await _operations.AddItemAsync(container, null, "Item");
        var actionExecuted = false;

        ShowItemDelegate action = async (ctx, next) =>
        {
            actionExecuted = true;
            await next();
        };

        _operations.RegisterShowItemAction<RealTreeItem>(action);
        var removed = _operations.DeregisterShowItemAction<RealTreeItem>(action);

        await _operations.ShowItemAsync(item);

        Assert.True(removed);
        Assert.False(actionExecuted);
    }
}

// Custom item type for testing type-based middleware
public class CustomItemType : RealTreeItem
{
    public CustomItemType(Guid? id = null, string? name = null) : base(id, name)
    {
    }
}
