namespace Mesch.RealTree;

/// <summary>
/// Base context for all tree operations, providing common information available to all actions and events.
/// </summary>
public abstract class OperationContext
{
    /// <summary>
    /// Gets the cancellation token that can be used to cancel the operation.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets the root tree instance where the operation is taking place.
    /// </summary>
    public IRealTree Tree { get; }

    /// <summary>
    /// Gets the UTC timestamp when the operation was initiated.
    /// </summary>
    public DateTime OperationTime { get; }

    /// <summary>
    /// Initializes a new instance of the operation context.
    /// </summary>
    /// <param name="tree">The tree where the operation occurs.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    protected OperationContext(IRealTree tree, CancellationToken cancellationToken)
    {
        Tree = tree;
        CancellationToken = cancellationToken;
        OperationTime = DateTime.UtcNow;
    }
}