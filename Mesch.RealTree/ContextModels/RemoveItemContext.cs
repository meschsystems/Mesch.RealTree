namespace Mesch.RealTree;

/// <summary>
/// Context for item removal operations, providing strongly-typed information about the item being removed.
/// </summary>
public class RemoveItemContext : OperationContext
{
    /// <summary>
    /// Gets the item that is being removed from the tree.
    /// </summary>
    public IRealTreeItem Item { get; }

    /// <summary>
    /// Gets the parent container that contained the item being removed.
    /// </summary>
    public IRealTreeContainer Parent { get; }

    /// <summary>
    /// Initializes a new instance of the RemoveItemContext class.
    /// </summary>
    /// <param name="item">The item being removed.</param>
    /// <param name="parent">The parent container that contains the item being removed.</param>
    /// <param name="tree">The tree where the removal operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public RemoveItemContext(IRealTreeItem item, IRealTreeContainer parent, IRealTree tree, CancellationToken cancellationToken)
        : base(tree, cancellationToken)
    {
        Item = item;
        Parent = parent;
    }
}
