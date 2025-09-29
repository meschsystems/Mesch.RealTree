using Xunit.Abstractions;

namespace Mesch.RealTree.Tests;

public class RealTreeEventsTests
{
    private readonly ITestOutputHelper _output;
    private readonly IRealTreeFactory _factory;
    private readonly IRealTreeOperations _operations;

    public RealTreeEventsTests(ITestOutputHelper output)
    {
        _output = output;
        _factory = new RealTreeFactory();
        _operations = new RealTreeOperations(_factory);
    }

    [Fact]
    public async Task ContainerAddedEvent_ExecutesAfterAdd()
    {
        var tree = _factory.CreateTree();
        var executed = false;
        IRealTreeContainer capturedContainer = null;

        tree.RegisterContainerAddedEvent(async ctx =>
        {
            executed = true;
            capturedContainer = ctx.Container;
            await Task.CompletedTask;
        });

        var container = await _operations.AddContainerAsync(tree, null, "Test");
        await Task.Delay(50); // Give event time to execute

        Assert.True(executed);
        Assert.Same(container, capturedContainer);
    }

    [Fact]
    public async Task ItemAddedEvent_ExecutesAfterAdd()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var executed = false;
        IRealTreeItem capturedItem = null;

        container.RegisterItemAddedEvent(async ctx =>
        {
            executed = true;
            capturedItem = ctx.Item;
            await Task.CompletedTask;
        });

        var item = await _operations.AddItemAsync(container, null, "Test");
        await Task.Delay(50);

        Assert.True(executed);
        Assert.Same(item, capturedItem);
    }

    [Fact]
    public async Task ContainerRemovedEvent_ExecutesAfterRemove()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Test");
        var tcs = new TaskCompletionSource<bool>();
        var capturedNode = default(IRealTreeNode);

        // Register on parent (tree) not on the container being removed
        tree.RegisterContainerRemovedEvent(async ctx =>
        {
            capturedNode = ctx.Node;
            tcs.TrySetResult(true);
            await Task.CompletedTask;
        });

        await _operations.RemoveAsync(container);

        // Wait for event with timeout
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(200));

        Assert.Same(tcs.Task, completed);
        Assert.Same(container, capturedNode);
    }

    [Fact]
    public async Task ItemRemovedEvent_ExecutesAfterRemove()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var item = await _operations.AddItemAsync(container, null, "Item");
        var tcs = new TaskCompletionSource<bool>();
        var capturedNode = default(IRealTreeNode);

        // Register on parent (container) not on the item being removed
        container.RegisterItemRemovedEvent(async ctx =>
        {
            capturedNode = ctx.Node;
            tcs.TrySetResult(true);
            await Task.CompletedTask;
        });

        await _operations.RemoveAsync(item);

        // Wait for event with timeout
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(200));

        Assert.Same(tcs.Task, completed);
        Assert.Same(item, capturedNode);
    }

    [Fact]
    public async Task NodeUpdatedEvent_ExecutesAfterUpdate()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "OldName");
        var executed = false;

        container.RegisterNodeUpdatedEvent(async ctx =>
        {
            executed = true;
            Assert.Equal("OldName", ctx.OldName);
            Assert.Equal("NewName", ctx.NewName);
            await Task.CompletedTask;
        });

        await _operations.UpdateAsync(container, newName: "NewName");
        await Task.Delay(50);

        Assert.True(executed);
    }

    [Fact]
    public async Task NodeMovedEvent_ExecutesAfterMove()
    {
        var tree = _factory.CreateTree();
        var container1 = await _operations.AddContainerAsync(tree, null, "C1");
        var container2 = await _operations.AddContainerAsync(tree, null, "C2");
        var item = await _operations.AddItemAsync(container1, null, "Item");
        var executed = false;

        item.RegisterNodeMovedEvent(async ctx =>
        {
            executed = true;
            Assert.Same(container1, ctx.OldParent);
            Assert.Same(container2, ctx.NewParent);
            await Task.CompletedTask;
        });

        await _operations.MoveAsync(item, container2);
        await Task.Delay(50);

        Assert.True(executed);
    }

    [Fact]
    public async Task BulkContainersAddedEvent_ExecutesAfterBulkAdd()
    {
        var tree = _factory.CreateTree();
        var executed = false;

        tree.RegisterBulkContainersAddedEvent(async ctx =>
        {
            executed = true;
            Assert.Equal(3, ctx.Containers.Count);
            await Task.CompletedTask;
        });

        var containers = new[]
        {
            _factory.CreateContainer(name: "C1"),
            _factory.CreateContainer(name: "C2"),
            _factory.CreateContainer(name: "C3")
        };

        await _operations.BulkAddContainersAsync(tree, containers);
        await Task.Delay(50);

        Assert.True(executed);
    }

    [Fact]
    public async Task BulkItemsAddedEvent_ExecutesAfterBulkAdd()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var executed = false;

        container.RegisterBulkItemsAddedEvent(async ctx =>
        {
            executed = true;
            Assert.Equal(2, ctx.Items.Count);
            await Task.CompletedTask;
        });

        var items = new[]
        {
            _factory.CreateItem(name: "I1"),
            _factory.CreateItem(name: "I2")
        };

        await _operations.BulkAddItemsAsync(container, items);
        await Task.Delay(50);

        Assert.True(executed);
    }

    [Fact]
    public async Task BulkNodesRemovedEvent_ExecutesAfterBulkRemove()
    {
        var tree = _factory.CreateTree();
        var c1 = await _operations.AddContainerAsync(tree, null, "C1");
        var c2 = await _operations.AddContainerAsync(tree, null, "C2");
        var executed = false;

        tree.RegisterBulkNodesRemovedEvent(async ctx =>
        {
            executed = true;
            Assert.Equal(2, ctx.Nodes.Count);
            await Task.CompletedTask;
        });

        await _operations.BulkRemoveAsync(new[] { c1, c2 });
        await Task.Delay(50);

        Assert.True(executed);
    }

    [Fact]
    public async Task MultipleEvents_ExecuteInParallel()
    {
        var tree = _factory.CreateTree();
        var executionTimes = new List<DateTime>();

        tree.RegisterContainerAddedEvent(async ctx =>
        {
            await Task.Delay(10);
            lock (executionTimes)
            {
                executionTimes.Add(DateTime.UtcNow);
            }
        });

        tree.RegisterContainerAddedEvent(async ctx =>
        {
            await Task.Delay(10);
            lock (executionTimes)
            {
                executionTimes.Add(DateTime.UtcNow);
            }
        });

        tree.RegisterContainerAddedEvent(async ctx =>
        {
            await Task.Delay(10);
            lock (executionTimes)
            {
                executionTimes.Add(DateTime.UtcNow);
            }
        });

        await _operations.AddContainerAsync(tree, null, "Test");
        await Task.Delay(100);

        Assert.Equal(3, executionTimes.Count);
        // Events should execute roughly concurrently
        var timeSpan = executionTimes.Max() - executionTimes.Min();
        Assert.True(timeSpan.TotalMilliseconds < 50);
    }

    [Fact]
    public async Task Event_ExceptionDoesNotAffectOperation()
    {
        var tree = _factory.CreateTree();

        tree.RegisterContainerAddedEvent(async ctx =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Event failed");
        });

        // Operation should complete despite event exception
        var container = await _operations.AddContainerAsync(tree, null, "Test");
        await Task.Delay(50);

        Assert.Single(tree.Containers);
        Assert.Contains(container, tree.Containers);
    }

    [Fact]
    public async Task DeregisterEvent_RemovesEventHandler()
    {
        var tree = _factory.CreateTree();
        var executed = false;

        ContainerAddedEventDelegate eventHandler = async ctx =>
        {
            executed = true;
            await Task.CompletedTask;
        };

        tree.RegisterContainerAddedEvent(eventHandler);
        var removed = tree.DeregisterContainerAddedEvent(eventHandler);

        await _operations.AddContainerAsync(tree, null, "Test");
        await Task.Delay(50);

        Assert.True(removed);
        Assert.False(executed);
    }

    [Fact]
    public void DeregisterEvent_ReturnsFalseForNonExistentHandler()
    {
        var tree = _factory.CreateTree();

        ContainerAddedEventDelegate eventHandler = async ctx => await Task.CompletedTask;
        var removed = tree.DeregisterContainerAddedEvent(eventHandler);

        Assert.False(removed);
    }

    [Fact]
    public async Task Events_PropagateFromTreeLevel()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "Container");
        var executed = false;

        tree.RegisterItemAddedEvent(async ctx =>
        {
            executed = true;
            await Task.CompletedTask;
        });

        await _operations.AddItemAsync(container, null, "Item");
        await Task.Delay(50);

        Assert.True(executed);
    }
}
