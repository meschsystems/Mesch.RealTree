using Xunit.Abstractions;

namespace Mesch.RealTree.Tests;

public class RealTreeRemoveDiagnosticTests
{
    private readonly ITestOutputHelper _output;
    private readonly IRealTreeFactory _factory;
    private readonly IRealTreeOperations _operations;

    public RealTreeRemoveDiagnosticTests(ITestOutputHelper output)
    {
        _output = output;
        _factory = new RealTreeFactory();
        _operations = new RealTreeOperations(_factory);
    }

    [Fact]
    public async Task DiagnoseRemove_CheckActualCollection()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "TestContainer");

        var actionExecuted = false;

        // Register action
        tree.RegisterRemoveContainerAction(async (ctx, next) =>
        {
            actionExecuted = true;
            _output.WriteLine("Action executing!");
            await next();
        });

        // Manually replicate CollectFromHierarchy
        var delegates = new List<RemoveContainerDelegate>();
        var current = container as IRealTreeNode;

        while (current != null)
        {
            var actionsFromNode = current switch
            {
                RealTreeContainer cont => cont.RemoveContainerActions,
                RealTreeItem item => item.RemoveContainerActions,
                _ => System.Linq.Enumerable.Empty<RemoveContainerDelegate>()
            };

            _output.WriteLine($"Node {current.Name}: adding {actionsFromNode.Count()} actions");
            delegates.AddRange(actionsFromNode);

            current = current.Parent;
        }

        _output.WriteLine($"Total delegates collected: {delegates.Count}");

        // Now test if manually building pipeline works
        if (delegates.Count > 0)
        {
            _output.WriteLine("Building pipeline manually...");
            Func<Task> coreOp = () =>
            {
                _output.WriteLine("Core operation executed!");
                return Task.CompletedTask;
            };

            var pipeline = delegates.Aggregate(coreOp, (next, action) =>
            {
                return () => (Task)action.DynamicInvoke(new RemoveContainerContext(container, (IRealTreeContainer)tree, tree, default), next)!;
            });

            _output.WriteLine("Executing pipeline...");
            await pipeline();
        }

        _output.WriteLine($"Action was executed: {actionExecuted}");
    }

    [Fact]
    public async Task DiagnoseRemove_WithAction_ToSeeIfCoreOperationCalled()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "TestContainer");

        _output.WriteLine($"Before removal: tree has {tree.Containers.Count} containers");
        _output.WriteLine($"Container parent before: {container.Parent?.Name ?? "null"}");

        var actionCalled = false;
        var coreOperationReached = false;

        // Register an action that logs but calls next()
        tree.RegisterRemoveContainerAction(async (ctx, next) =>
        {
            actionCalled = true;
            _output.WriteLine($"Action called for {ctx.Container.Name}");
            _output.WriteLine($"About to call next()...");
            await next();
            coreOperationReached = true;
            _output.WriteLine($"After next() returned");
        });

        await _operations.RemoveAsync(container);

        _output.WriteLine($"After removal: tree has {tree.Containers.Count} containers");
        _output.WriteLine($"Container parent after: {container.Parent?.Name ?? "null"}");
        _output.WriteLine($"Action was called: {actionCalled}");
        _output.WriteLine($"Core operation reached: {coreOperationReached}");

        Assert.True(actionCalled, "Action should have been called");
        Assert.True(coreOperationReached, "Core operation should have been reached");
        Assert.Empty(tree.Containers);
    }

    [Fact]
    public async Task DiagnoseRemove_WithoutActions_DirectRemoval()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "TestContainer");

        _output.WriteLine($"Before removal: tree has {tree.Containers.Count} containers");
        _output.WriteLine($"Container parent before: {container.Parent?.Name ?? "null"}");

        // Call with triggerActions = false to bypass action pipeline
        await _operations.RemoveAsync(container, triggerActions: false, triggerEvents: false);

        _output.WriteLine($"After removal: tree has {tree.Containers.Count} containers");
        _output.WriteLine($"Container parent after: {container.Parent?.Name ?? "null"}");

        Assert.Empty(tree.Containers);
    }
}