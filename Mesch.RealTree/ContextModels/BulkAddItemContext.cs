namespace Mesch.RealTree;

/// <summary>
/// Context for bulk item addition operations, providing information about multiple items being added to a container.
/// </summary>
public class BulkAddItemContext : OperationContext
{
    /// <summary>
    /// Gets the collection of items that are being added in this bulk operation.
    /// </summary>
    public IReadOnlyList<IRealTreeItem> Items { get; }

    /// <summary>
    /// Gets the parent container that will contain all the items being added.
    /// </summary>
    public IRealTreeContainer Parent { get; }

    /// <summary>
    /// Initializes a new instance of the BulkAddItemContext class.
    /// </summary>
    /// <param name="items">The collection of items being added in the bulk operation.</param>
    /// <param name="parent">The parent container that will contain all the added items.</param>
    /// <param name="tree">The tree where the bulk addition operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public BulkAddItemContext(IReadOnlyList<IRealTreeItem> items, IRealTreeContainer parent, IRealTree tree, CancellationToken cancellationToken)
        : base(tree, cancellationToken)
    {
        Items = items;
        Parent = parent;
    }
}