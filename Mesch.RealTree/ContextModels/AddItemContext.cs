namespace Mesch.RealTree;

/// <summary>
/// Context for item addition operations, providing information about the item being added and its parent container.
/// </summary>
public class AddItemContext : OperationContext
{
    /// <summary>
    /// Gets the item that is being added to the tree.
    /// </summary>
    public IRealTreeItem Item { get; }

    /// <summary>
    /// Gets the parent container that will contain the item being added.
    /// Items can only be added to containers, not to other items.
    /// </summary>
    public IRealTreeContainer Parent { get; }

    /// <summary>
    /// Initializes a new instance of the AddItemContext class.
    /// </summary>
    /// <param name="item">The item being added.</param>
    /// <param name="parent">The parent container that will contain the added item.</param>
    /// <param name="tree">The tree where the addition operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public AddItemContext(IRealTreeItem item, IRealTreeContainer parent, IRealTree tree, CancellationToken cancellationToken)
        : base(tree, cancellationToken)
    {
        Item = item;
        Parent = parent;
    }
}