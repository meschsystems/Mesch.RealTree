namespace Mesch.RealTree;

/// <summary>
/// Context for node update operations, providing information about the node being updated and the old/new values.
/// </summary>
public class UpdateContext : OperationContext
{
    /// <summary>
    /// Gets the node that is being updated.
    /// </summary>
    public IRealTreeNode Node { get; }

    /// <summary>
    /// Gets the original name of the node before the update, or null if the name is not being changed.
    /// </summary>
    public string? OldName { get; }

    /// <summary>
    /// Gets the new name that will be assigned to the node, or null if the name is not being changed.
    /// </summary>
    public string? NewName { get; }

    /// <summary>
    /// Gets a copy of the original metadata dictionary before the update, or null if metadata is not being changed.
    /// </summary>
    public Dictionary<string, object>? OldMetadata { get; }

    /// <summary>
    /// Gets the new metadata dictionary that will replace the existing metadata, or null if metadata is not being changed.
    /// </summary>
    public Dictionary<string, object>? NewMetadata { get; }

    /// <summary>
    /// Initializes a new instance of the UpdateContext class.
    /// </summary>
    /// <param name="node">The node being updated.</param>
    /// <param name="oldName">The original name of the node, or null if name is not changing.</param>
    /// <param name="newName">The new name for the node, or null if name is not changing.</param>
    /// <param name="oldMetadata">A copy of the original metadata, or null if metadata is not changing.</param>
    /// <param name="newMetadata">The new metadata dictionary, or null if metadata is not changing.</param>
    /// <param name="tree">The tree where the update operation is taking place.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public UpdateContext(IRealTreeNode node, string? oldName, string? newName,
        Dictionary<string, object>? oldMetadata, Dictionary<string, object>? newMetadata,
        IRealTree tree, CancellationToken cancellationToken)
        : base(tree, cancellationToken)
    {
        Node = node;
        OldName = oldName;
        NewName = newName;
        OldMetadata = oldMetadata;
        NewMetadata = newMetadata;
    }
}