namespace Mesch.RealTree;

/// <summary>
/// Context for container addition operations, providing information about the container being added and its parent node.
/// </summary>
public class AddContainerContext : OperationContext
{
    /// <summary>
    /// Gets the container that is being added to the tree.
    /// </summary>
    public IRealTreeContainer Container { get; }

    /// <summary>
    /// Gets the parent node that will contain the container being added.
    /// Can be either a container or an item, as both node types can hold containers.
    /// </summary>
    public IRealTreeNode Parent { get; }

    /// <summary>
    /// Gets the parent as a container if it implements IRealTreeContainer, otherwise null.
    /// </summary>
    public IRealTreeContainer? ParentAsContainer => Parent as IRealTreeContainer;

    /// <summary>
    /// Gets the parent as an item if it implements IRealTreeItem, otherwise null.
    /// </summary>
    public IRealTreeItem? ParentAsItem => Parent as IRealTreeItem;

    /// <summary>
    /// Initializes a new instance of the AddContainerContext class.
    /// </summary>
    /// <param name="container">The container being added.</param>
    /// <param name="parent">The parent node that will contain the added container.</param>
    /// <param name="tree">The tree where the addition operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public AddContainerContext(IRealTreeContainer container, IRealTreeNode parent, IRealTree tree, CancellationToken cancellationToken)
        : this(container, parent, tree, cancellationToken, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AddContainerContext class with transaction support.
    /// </summary>
    /// <param name="container">The container being added.</param>
    /// <param name="parent">The parent node that will contain the added container.</param>
    /// <param name="tree">The tree where the addition operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <param name="transaction">The transaction scope for this operation, if any.</param>
    public AddContainerContext(IRealTreeContainer container, IRealTreeNode parent, IRealTree tree, CancellationToken cancellationToken, IRealTreeTransaction? transaction)
        : base(tree, cancellationToken, transaction)
    {
        Container = container;
        Parent = parent;
    }
}