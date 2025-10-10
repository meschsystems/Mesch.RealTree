namespace Mesch.RealTree;

/// <summary>
/// Context for container removal operations, providing strongly-typed information about the container being removed.
/// </summary>
public class RemoveContainerContext : OperationContext
{
    /// <summary>
    /// Gets the container that is being removed from the tree.
    /// </summary>
    public IRealTreeContainer Container { get; }

    /// <summary>
    /// Gets the parent container that contained the container being removed.
    /// </summary>
    public IRealTreeContainer Parent { get; }

    /// <summary>
    /// Initializes a new instance of the RemoveContainerContext class.
    /// </summary>
    /// <param name="container">The container being removed.</param>
    /// <param name="parent">The parent container that contains the container being removed.</param>
    /// <param name="tree">The tree where the removal operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public RemoveContainerContext(IRealTreeContainer container, IRealTreeContainer parent, IRealTree tree, CancellationToken cancellationToken)
        : this(container, parent, tree, cancellationToken, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the RemoveContainerContext class with transaction support.
    /// </summary>
    /// <param name="container">The container being removed.</param>
    /// <param name="parent">The parent container that contains the container being removed.</param>
    /// <param name="tree">The tree where the removal operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <param name="transaction">The transaction scope for this operation, if any.</param>
    public RemoveContainerContext(IRealTreeContainer container, IRealTreeContainer parent, IRealTree tree, CancellationToken cancellationToken, IRealTreeTransaction? transaction)
        : base(tree, cancellationToken, transaction)
    {
        Container = container;
        Parent = parent;
    }
}
