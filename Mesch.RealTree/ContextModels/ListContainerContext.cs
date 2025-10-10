namespace Mesch.RealTree;

/// <summary>
/// Context for listing operations, providing information about the node whose contents are being requested.
/// Both containers and items can be listed (both can contain child containers).
/// </summary>
public class ListContainerContext : OperationContext
{
    /// <summary>
    /// Gets the node whose contents are being listed (can be a container or item).
    /// </summary>
    public IRealTreeNode Node { get; }

    /// <summary>
    /// Gets the node as a container if it is one, otherwise null.
    /// Convenience property for accessing container-specific functionality.
    /// </summary>
    public IRealTreeContainer? Container => Node as IRealTreeContainer;

    /// <summary>
    /// Gets the node as an item if it is one, otherwise null.
    /// Convenience property for accessing item-specific functionality.
    /// </summary>
    public IRealTreeItem? Item => Node as IRealTreeItem;

    /// <summary>
    /// Gets or sets whether to include child containers in the listing.
    /// Can be modified by middleware actions to alter the listing behavior.
    /// </summary>
    public bool IncludeContainers { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include child items in the listing.
    /// Can be modified by middleware actions to alter the listing behavior.
    /// </summary>
    public bool IncludeItems { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to perform a recursive listing of all descendants.
    /// Can be modified by middleware actions to alter the listing behavior.
    /// </summary>
    public bool Recursive { get; set; } = false;

    /// <summary>
    /// Gets the metadata dictionary for storing arbitrary key-value pairs during the listing operation.
    /// Can be used by middleware actions to pass additional information.
    /// </summary>
    public Dictionary<string, object> ListingMetadata { get; } = new Dictionary<string, object>();

    /// <summary>
    /// Initializes a new instance of the ListContainerContext class.
    /// </summary>
    /// <param name="node">The node whose contents are being listed (container or item).</param>
    /// <param name="tree">The tree where the listing operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public ListContainerContext(IRealTreeNode node, IRealTree tree, CancellationToken cancellationToken)
        : base(tree, cancellationToken)
    {
        Node = node;
    }

    /// <summary>
    /// Initializes a new instance of the ListContainerContext class with specific listing options.
    /// </summary>
    /// <param name="node">The node whose contents are being listed (container or item).</param>
    /// <param name="includeContainers">Whether to include child containers in the listing.</param>
    /// <param name="includeItems">Whether to include child items in the listing.</param>
    /// <param name="recursive">Whether to perform a recursive listing of all descendants.</param>
    /// <param name="tree">The tree where the listing operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public ListContainerContext(IRealTreeNode node, bool includeContainers, bool includeItems, bool recursive, IRealTree tree, CancellationToken cancellationToken)
        : base(tree, cancellationToken)
    {
        Node = node;
        IncludeContainers = includeContainers;
        IncludeItems = includeItems;
        Recursive = recursive;
    }
}