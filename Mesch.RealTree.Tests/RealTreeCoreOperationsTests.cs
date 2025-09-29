using Xunit.Abstractions;

namespace Mesch.RealTree.Tests;

public class RealTreeCoreOperationsTests
{
    private readonly ITestOutputHelper _output;
    private readonly IRealTreeFactory _factory;
    private readonly IRealTreeOperations _operations;

    public RealTreeCoreOperationsTests(ITestOutputHelper output)
    {
        _output = output;
        _factory = new RealTreeFactory();
        _operations = new RealTreeOperations(_factory);
    }

    [Fact]
    public async Task AddContainerAsync_WithValidParent_AddsContainerSuccessfully()
    {
        var tree = _factory.CreateTree();

        var container = await _operations.AddContainerAsync(tree, null, "TestContainer");

        Assert.NotNull(container);
        Assert.Equal("TestContainer", container.Name);
        Assert.Equal(tree, container.Parent);
        Assert.Single(tree.Containers);
        Assert.Contains(container, tree.Containers);
    }

    [Fact]
    public async Task AddContainerAsync_WithExistingContainer_AddsToTree()
    {
        var tree = _factory.CreateTree();
        var container = _factory.CreateContainer(name: "PreCreated");

        var result = await _operations.AddContainerAsync(tree, container);

        Assert.Same(container, result);
        Assert.Equal(tree, container.Parent);
        Assert.Single(tree.Containers);
    }

    [Fact]
    public async Task AddItemAsync_WithValidParent_AddsItemSuccessfully()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");

        var item = await _operations.AddItemAsync(container, null, "TestItem");

        Assert.NotNull(item);
        Assert.Equal("TestItem", item.Name);
        Assert.Equal(container, item.Parent);
        Assert.Single(container.Items);
        Assert.Contains(item, container.Items);
    }

    [Fact]
    public async Task AddItemAsync_WithExistingItem_AddsToContainer()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var item = _factory.CreateItem(name: "PreCreatedItem");

        var result = await _operations.AddItemAsync(container, item);

        Assert.Same(item, result);
        Assert.Equal(container, item.Parent);
        Assert.Single(container.Items);
    }

    [Fact]
    public async Task RemoveAsync_RemovesContainerFromParent()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "ToRemove");

        await _operations.RemoveAsync(container);

        Assert.Empty(tree.Containers);
        Assert.Null(container.Parent);
    }

    [Fact]
    public async Task RemoveAsync_RemovesItemFromParent()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var item = await _operations.AddItemAsync(container, null, "ToRemove");

        await _operations.RemoveAsync(item);

        Assert.Empty(container.Items);
        Assert.Null(item.Parent);
    }

    [Fact]
    public async Task RemoveAllContainersAsync_RemovesAllContainersFromParent()
    {
        var tree = _factory.CreateTree();
        await _operations.AddContainerAsync(tree, null, "Container1");
        await _operations.AddContainerAsync(tree, null, "Container2");
        await _operations.AddContainerAsync(tree, null, "Container3");

        await _operations.RemoveAllContainersAsync(tree);

        Assert.Empty(tree.Containers);
    }

    [Fact]
    public async Task RemoveAllItemsAsync_RemovesAllItemsFromContainer()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        await _operations.AddItemAsync(container, null, "Item1");
        await _operations.AddItemAsync(container, null, "Item2");
        await _operations.AddItemAsync(container, null, "Item3");

        await _operations.RemoveAllItemsAsync(container);

        Assert.Empty(container.Items);
    }

    [Fact]
    public async Task UpdateAsync_ChangesNodeName()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "OldName");

        await _operations.UpdateAsync(container, newName: "NewName");

        Assert.Equal("NewName", container.Name);
    }

    [Fact]
    public async Task UpdateAsync_ChangesNodeMetadata()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        container.Metadata["key1"] = "value1";

        var newMetadata = new Dictionary<string, object> { { "key2", "value2" } };
        await _operations.UpdateAsync(container, newMetadata: newMetadata);

        Assert.Single(container.Metadata);
        Assert.Equal("value2", container.Metadata["key2"]);
        Assert.False(container.Metadata.ContainsKey("key1"));
    }

    [Fact]
    public async Task MoveAsync_MovesNodeToNewParent()
    {
        var tree = _factory.CreateTree();
        var container1 = await _operations.AddContainerAsync(tree, null, "Container1");
        var container2 = await _operations.AddContainerAsync(tree, null, "Container2");
        var item = await _operations.AddItemAsync(container1, null, "Item");

        await _operations.MoveAsync(item, container2);

        Assert.Equal(container2, item.Parent);
        Assert.Empty(container1.Items);
        Assert.Single(container2.Items);
        Assert.Contains(item, container2.Items);
    }

    [Fact]
    public async Task MoveAsync_ThrowsOnCyclicReference()
    {
        var tree = _factory.CreateTree();
        var container1 = await _operations.AddContainerAsync(tree, null, "Container1");
        var container2 = await _operations.AddContainerAsync(container1, null, "Container2");

        await Assert.ThrowsAsync<CyclicReferenceException>(
            async () => await _operations.MoveAsync(container1, container2));
    }

    [Fact]
    public async Task BulkAddContainersAsync_AddsMultipleContainers()
    {
        var tree = _factory.CreateTree();
        var containers = new List<IRealTreeContainer>
        {
            _factory.CreateContainer(name: "Bulk1"),
            _factory.CreateContainer(name: "Bulk2"),
            _factory.CreateContainer(name: "Bulk3")
        };

        await _operations.BulkAddContainersAsync(tree, containers);

        Assert.Equal(3, tree.Containers.Count);
        Assert.All(containers, c => Assert.Contains(c, tree.Containers));
    }

    [Fact]
    public async Task BulkAddItemsAsync_AddsMultipleItems()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var items = new List<IRealTreeItem>
        {
            _factory.CreateItem(name: "Item1"),
            _factory.CreateItem(name: "Item2"),
            _factory.CreateItem(name: "Item3")
        };

        await _operations.BulkAddItemsAsync(container, items);

        Assert.Equal(3, container.Items.Count);
        Assert.All(items, i => Assert.Contains(i, container.Items));
    }

    [Fact]
    public async Task BulkRemoveAsync_RemovesMultipleNodes()
    {
        var tree = _factory.CreateTree();
        var container1 = await _operations.AddContainerAsync(tree, null, "C1");
        var container2 = await _operations.AddContainerAsync(tree, null, "C2");
        var container3 = await _operations.AddContainerAsync(tree, null, "C3");

        await _operations.BulkRemoveAsync(new[] { container1, container2 });

        Assert.Single(tree.Containers);
        Assert.Contains(container3, tree.Containers);
    }

    [Fact]
    public async Task CopyContainerAsync_CreatesDeepCopy()
    {
        var tree = _factory.CreateTree();
        var source = await _operations.AddContainerAsync(tree, null, "Source");
        var childContainer = await _operations.AddContainerAsync(source, null, "Child");
        var childItem = await _operations.AddItemAsync(source, null, "Item");
        source.Metadata["key"] = "value";

        var copy = await _operations.CopyContainerAsync(source, tree);

        Assert.NotEqual(source.Id, copy.Id);
        Assert.Equal(source.Name, copy.Name);
        Assert.Single(copy.Containers);
        Assert.Single(copy.Items);
        Assert.Equal("value", copy.Metadata["key"]);
    }

    [Fact]
    public async Task CopyItemAsync_CreatesDeepCopy()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var source = await _operations.AddItemAsync(container, null, "Source");
        var childContainer = await _operations.AddContainerAsync(source, null, "Child");
        source.Metadata["key"] = "value";

        var copy = await _operations.CopyItemAsync(source, container);

        Assert.NotEqual(source.Id, copy.Id);
        Assert.Equal(source.Name, copy.Name);
        Assert.Single(copy.Containers);
        Assert.Equal("value", copy.Metadata["key"]);
    }

    [Fact]
    public async Task Operations_WithTriggerActionsFalse_SkipsActions()
    {
        var tree = _factory.CreateTree();
        var actionExecuted = false;
        tree.RegisterAddContainerAction(async (ctx, next) =>
        {
            actionExecuted = true;
            await next();
        });

        await _operations.AddContainerAsync(tree, null, "Test", triggerActions: false);

        Assert.False(actionExecuted);
    }

    [Fact]
    public async Task Operations_WithTriggerEventsFalse_SkipsEvents()
    {
        var tree = _factory.CreateTree();
        var eventExecuted = false;
        tree.RegisterContainerAddedEvent(async ctx =>
        {
            eventExecuted = true;
            await Task.CompletedTask;
        });

        await _operations.AddContainerAsync(tree, null, "Test", triggerEvents: false);
        await Task.Delay(50); // Give events time to fire

        Assert.False(eventExecuted);
    }
}