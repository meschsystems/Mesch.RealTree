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
    /// Gets the transaction scope for this operation, if any.
    /// Middleware can use this to register commit/rollback callbacks.
    /// </summary>
    public IRealTreeTransaction? Transaction { get; }

    /// <summary>
    /// Initializes a new instance of the operation context.
    /// </summary>
    /// <param name="tree">The tree where the operation occurs.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    protected OperationContext(IRealTree tree, CancellationToken cancellationToken)
        : this(tree, cancellationToken, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the operation context with transaction support.
    /// </summary>
    /// <param name="tree">The tree where the operation occurs.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <param name="transaction">The transaction scope for this operation, if any.</param>
    protected OperationContext(IRealTree tree, CancellationToken cancellationToken, IRealTreeTransaction? transaction)
    {
        Tree = tree;
        CancellationToken = cancellationToken;
        Transaction = transaction;
        OperationTime = DateTime.UtcNow;
    }
}