namespace Mesch.RealTree;

/// <summary>
/// Context for item show operations, providing information about the item being displayed.
/// </summary>
public class ShowItemContext : OperationContext
{
    /// <summary>
    /// Gets the item being shown.
    /// </summary>
    public IRealTreeItem Item { get; }

    /// <summary>
    /// Gets the metadata dictionary for storing arbitrary key-value pairs during the show operation.
    /// Can be used by middleware actions to pass additional information or control rendering.
    /// </summary>
    public Dictionary<string, object> ShowMetadata { get; } = new Dictionary<string, object>();

    /// <summary>
    /// Initializes a new instance of the ShowItemContext class.
    /// </summary>
    /// <param name="item">The item being shown.</param>
    /// <param name="tree">The tree where the show operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public ShowItemContext(IRealTreeItem item, IRealTree tree, CancellationToken cancellationToken)
        : base(tree, cancellationToken)
    {
        Item = item;
    }
}
