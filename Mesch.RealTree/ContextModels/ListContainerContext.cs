namespace Mesch.RealTree;

/// <summary>
/// Context for container listing operations, providing information about the container whose contents are being requested.
/// </summary>
public class ListContainerContext : OperationContext
{
    /// <summary>
    /// Gets the container whose contents are being listed.
    /// </summary>
    public IRealTreeContainer Container { get; }

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
    /// <param name="container">The container whose contents are being listed.</param>
    /// <param name="tree">The tree where the listing operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public ListContainerContext(IRealTreeContainer container, IRealTree tree, CancellationToken cancellationToken)
        : base(tree, cancellationToken)
    {
        Container = container;
    }

    /// <summary>
    /// Initializes a new instance of the ListContainerContext class with specific listing options.
    /// </summary>
    /// <param name="container">The container whose contents are being listed.</param>
    /// <param name="includeContainers">Whether to include child containers in the listing.</param>
    /// <param name="includeItems">Whether to include child items in the listing.</param>
    /// <param name="recursive">Whether to perform a recursive listing of all descendants.</param>
    /// <param name="tree">The tree where the listing operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public ListContainerContext(IRealTreeContainer container, bool includeContainers, bool includeItems, bool recursive, IRealTree tree, CancellationToken cancellationToken)
        : base(tree, cancellationToken)
    {
        Container = container;
        IncludeContainers = includeContainers;
        IncludeItems = includeItems;
        Recursive = recursive;
    }
}