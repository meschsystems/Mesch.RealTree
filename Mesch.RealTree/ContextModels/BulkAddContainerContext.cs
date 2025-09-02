namespace Mesch.RealTree;

/// <summary>
/// Context for bulk container addition operations, providing information about multiple containers being added to a parent node.
/// </summary>
public class BulkAddContainerContext : OperationContext
{
    /// <summary>
    /// Gets the collection of containers that are being added in this bulk operation.
    /// </summary>
    public IReadOnlyList<IRealTreeContainer> Containers { get; }

    /// <summary>
    /// Gets the parent node that will contain all the containers being added.
    /// Can be either a container or an item, as both node types can hold containers.
    /// </summary>
    public IRealTreeNode Parent { get; }

    /// <summary>
    /// Initializes a new instance of the BulkAddContainerContext class.
    /// </summary>
    /// <param name="containers">The collection of containers being added in the bulk operation.</param>
    /// <param name="parent">The parent node that will contain all the added containers.</param>
    /// <param name="tree">The tree where the bulk addition operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public BulkAddContainerContext(IReadOnlyList<IRealTreeContainer> containers, IRealTreeNode parent, IRealTree tree, CancellationToken cancellationToken)
        : base(tree, cancellationToken)
    {
        Containers = containers;
        Parent = parent;
    }
}