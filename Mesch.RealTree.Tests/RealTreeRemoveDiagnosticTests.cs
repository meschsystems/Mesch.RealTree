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
    public async Task DiagnoseRemove_WithAction_ToSeeIfCoreOperationCalled()
    {
        var tree = _factory.CreateTree();
        var container = await _operations.AddContainerAsync(tree, null, "TestContainer");

        _output.WriteLine($"Before removal: tree has {tree.Containers.Count} containers");
        _output.WriteLine($"Container parent before: {container.Parent?.Name ?? "null"}");

        var actionCalled = false;
        var coreOperationReached = false;

        // Register an action that logs but calls next()
        _operations.RegisterRemoveContainerAction<RealTreeRoot>(async (ctx, next) =>
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
