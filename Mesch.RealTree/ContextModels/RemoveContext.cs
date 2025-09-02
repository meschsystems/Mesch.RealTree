namespace Mesch.RealTree;

/// <summary>
/// Context for node removal operations, providing information about the node being removed and its parent.
/// </summary>
public class RemoveContext : OperationContext
{
    /// <summary>
    /// Gets the node that is being removed from the tree.
    /// </summary>
    public IRealTreeNode Node { get; }

    /// <summary>
    /// Gets the parent node that contained the node being removed, or null if removing the root node.
    /// </summary>
    public IRealTreeNode? Parent { get; }

    /// <summary>
    /// Initializes a new instance of the RemoveContext class.
    /// </summary>
    /// <param name="node">The node being removed.</param>
    /// <param name="parent">The parent node that contains the node being removed, or null for root removal.</param>
    /// <param name="tree">The tree where the removal operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public RemoveContext(IRealTreeNode node, IRealTreeNode? parent, IRealTree tree, CancellationToken cancellationToken)
        : base(tree, cancellationToken)
    {
        Node = node;
        Parent = parent;
    }
}