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
    public async Task AddContainerAsync_WithMetadata_SetsMetadataBeforeMiddleware()
    {
        var tree = _factory.CreateTree();
        var metadata = new Dictionary<string, object>
        {
            ["key1"] = "value1",
            ["key2"] = 42
        };

        var container = await _operations.AddContainerAsync(tree, null, "TestContainer", metadata);

        Assert.Equal("value1", container.Metadata["key1"]);
        Assert.Equal(42, container.Metadata["key2"]);
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
    public async Task AddItemAsync_WithMetadata_SetsMetadataBeforeMiddleware()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var metadata = new Dictionary<string, object>
        {
            ["subdomain"] = "test",
            ["tier"] = "enterprise"
        };

        var item = await _operations.AddItemAsync(container, null, "TestItem", metadata);

        Assert.Equal("test", item.Metadata["subdomain"]);
        Assert.Equal("enterprise", item.Metadata["tier"]);
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

        Assert.Equal("value2", container.Metadata["key2"]);
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
    public async Task BulkAddContainersAsync_AddsMultipleContainers()
    {
        var tree = _factory.CreateTree();
        var containersData = new[]
        {
            (id: (Guid?)null, name: "Bulk1", metadata: (IDictionary<string, object>?)null),
            (id: (Guid?)null, name: "Bulk2", metadata: (IDictionary<string, object>?)null),
            (id: (Guid?)null, name: "Bulk3", metadata: (IDictionary<string, object>?)null)
        };

        await _operations.BulkAddContainersAsync<RealTreeContainer>(tree, containersData);

        Assert.Equal(3, tree.Containers.Count);
        Assert.Contains(tree.Containers, c => c.Name == "Bulk1");
        Assert.Contains(tree.Containers, c => c.Name == "Bulk2");
        Assert.Contains(tree.Containers, c => c.Name == "Bulk3");
    }

    [Fact]
    public async Task BulkAddItemsAsync_AddsMultipleItems()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var itemsData = new[]
        {
            (id: (Guid?)null, name: "Item1", metadata: (IDictionary<string, object>?)null),
            (id: (Guid?)null, name: "Item2", metadata: (IDictionary<string, object>?)null),
            (id: (Guid?)null, name: "Item3", metadata: (IDictionary<string, object>?)null)
        };

        await _operations.BulkAddItemsAsync<RealTreeItem>(container, itemsData);

        Assert.Equal(3, container.Items.Count);
        Assert.Contains(container.Items, i => i.Name == "Item1");
        Assert.Contains(container.Items, i => i.Name == "Item2");
        Assert.Contains(container.Items, i => i.Name == "Item3");
    }

    [Fact]
    public async Task BulkAddContainersAsync_WithMetadata_SetsMetadataForEachContainer()
    {
        var tree = _factory.CreateTree();
        var containersData = new[]
        {
            (id: (Guid?)null, name: "C1", metadata: (IDictionary<string, object>?)new Dictionary<string, object> { ["tier"] = "A" }),
            (id: (Guid?)null, name: "C2", metadata: (IDictionary<string, object>?)new Dictionary<string, object> { ["tier"] = "B" })
        };

        await _operations.BulkAddContainersAsync<RealTreeContainer>(tree, containersData);

        var c1 = tree.Containers.First(c => c.Name == "C1");
        var c2 = tree.Containers.First(c => c.Name == "C2");
        Assert.Equal("A", c1.Metadata["tier"]);
        Assert.Equal("B", c2.Metadata["tier"]);
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
    public async Task CopyContainerAsync_CreatesShallowCopyByDefault()
    {
        var tree = _factory.CreateTree();
        var source = await _operations.AddContainerAsync(tree, null, "Source");
        await _operations.AddContainerAsync(source, null, "Child");
        source.Metadata["key"] = "value";

        var copy = await _operations.CopyContainerAsync(source, tree, deep: false);

        Assert.NotEqual(source.Id, copy.Id);
        Assert.Equal(source.Name, copy.Name);
        Assert.Empty(copy.Containers); // Shallow copy doesn't copy children
        Assert.Equal("value", copy.Metadata["key"]);
    }

    [Fact]
    public async Task CopyItemAsync_CreatesShallowCopyByDefault()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var source = await _operations.AddItemAsync(container, null, "Source");
        await _operations.AddContainerAsync(source, null, "Child");
        source.Metadata["key"] = "value";

        var copy = await _operations.CopyItemAsync(source, container, deep: false);

        Assert.NotEqual(source.Id, copy.Id);
        Assert.Equal(source.Name, copy.Name);
        Assert.Empty(copy.Containers); // Shallow copy doesn't copy children
        Assert.Equal("value", copy.Metadata["key"]);
    }

    [Fact]
    public async Task Operations_WithTriggerActionsFalse_SkipsActions()
    {
        var tree = _factory.CreateTree();
        var actionExecuted = false;

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            actionExecuted = true;
            await next();
        });

        await _operations.AddContainerAsync(tree, null, "Test", triggerActions: false);

        Assert.False(actionExecuted);
    }

    [Fact]
    public async Task Operations_WithTriggerActionsTrue_ExecutesActions()
    {
        var tree = _factory.CreateTree();
        var actionExecuted = false;

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            actionExecuted = true;
            await next();
        });

        await _operations.AddContainerAsync(tree, null, "Test", triggerActions: true);

        Assert.True(actionExecuted);
    }
}
