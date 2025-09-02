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
    /// Initializes a new instance of the AddContainerContext class.
    /// </summary>
    /// <param name="container">The container being added.</param>
    /// <param name="parent">The parent node that will contain the added container.</param>
    /// <param name="tree">The tree where the addition operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public AddContainerContext(IRealTreeContainer container, IRealTreeNode parent, IRealTree tree, CancellationToken cancellationToken)
        : base(tree, cancellationToken)
    {
        Container = container;
        Parent = parent;
    }
}