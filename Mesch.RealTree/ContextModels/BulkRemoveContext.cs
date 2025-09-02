namespace Mesch.RealTree;

/// <summary>
/// Context for bulk removal operations, providing information about multiple nodes being removed from a common parent.
/// </summary>
public class BulkRemoveContext : OperationContext
{
    /// <summary>
    /// Gets the collection of nodes that are being removed in this bulk operation.
    /// Can include both containers and items being removed from the same parent.
    /// </summary>
    public IReadOnlyList<IRealTreeNode> Nodes { get; }

    /// <summary>
    /// Gets the parent node that contains all the nodes being removed.
    /// All nodes in the bulk removal operation must share the same parent.
    /// </summary>
    public IRealTreeNode Parent { get; }

    /// <summary>
    /// Initializes a new instance of the BulkRemoveContext class.
    /// </summary>
    /// <param name="nodes">The collection of nodes being removed in the bulk operation.</param>
    /// <param name="parent">The parent node that contains all the nodes being removed.</param>
    /// <param name="tree">The tree where the bulk removal operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public BulkRemoveContext(IReadOnlyList<IRealTreeNode> nodes, IRealTreeNode parent, IRealTree tree, CancellationToken cancellationToken)
        : base(tree, cancellationToken)
    {
        Nodes = nodes;
        Parent = parent;
    }
}