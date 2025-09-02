namespace Mesch.RealTree;

/// <summary>
/// Context for node move operations, providing information about the node being moved and its old/new parent locations.
/// </summary>
public class MoveContext : OperationContext
{
    /// <summary>
    /// Gets the node that is being moved to a new location in the tree.
    /// </summary>
    public IRealTreeNode Node { get; }

    /// <summary>
    /// Gets the original parent node that contained the node before the move, or null if moving from the root level.
    /// </summary>
    public IRealTreeNode? OldParent { get; }

    /// <summary>
    /// Gets the new parent node that will contain the node after the move operation completes.
    /// </summary>
    public IRealTreeNode NewParent { get; }

    /// <summary>
    /// Initializes a new instance of the MoveContext class.
    /// </summary>
    /// <param name="node">The node being moved.</param>
    /// <param name="oldParent">The original parent node, or null if moving from root level.</param>
    /// <param name="newParent">The new parent node that will contain the moved node.</param>
    /// <param name="tree">The tree where the move operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public MoveContext(IRealTreeNode node, IRealTreeNode? oldParent, IRealTreeNode newParent, IRealTree tree, CancellationToken cancellationToken)
        : base(tree, cancellationToken)
    {
        Node = node;
        OldParent = oldParent;
        NewParent = newParent;
    }
}