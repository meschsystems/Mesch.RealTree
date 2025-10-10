using Xunit.Abstractions;

namespace Mesch.RealTree.Tests;

/// <summary>
/// Event tests are no longer applicable in the type-based architecture.
/// Events were removed along with instance-based registration.
/// Middleware actions now serve the purpose of both actions and events.
/// </summary>
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
    public void EventsRemoved_MiddlewareActionsReplaceEvents()
    {
        // In the refactored architecture, events were removed
        // Middleware actions now serve both pre-operation and post-operation needs
        // Use the "after await next()" pattern for post-operation logic

        var tree = _factory.CreateTree();
        var postOperationExecuted = false;

        _operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
        {
            // Pre-operation logic here
            await next();
            // Post-operation logic here (replaces events)
            postOperationExecuted = true;
        });

        _operations.AddContainerAsync(tree, null, "Test").Wait();

        Assert.True(postOperationExecuted);
    }
}
