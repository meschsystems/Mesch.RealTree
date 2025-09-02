namespace Mesch.RealTree;

/// <summary>
/// Base interface for all tree nodes (containers and items).
/// </summary>
public interface IRealTreeNode
{
    /// <summary>
    /// Gets the unique identifier for this node.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets or sets the name of the node. Cannot be null or whitespace.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets the metadata dictionary for storing arbitrary key-value pairs.
    /// </summary>
    Dictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets the parent node, or null if this is the root node.
    /// </summary>
    IRealTreeNode? Parent { get; }

    /// <summary>
    /// Gets the root tree that contains this node.
    /// </summary>
    IRealTree Tree { get; }

    /// <summary>
    /// Gets the full path from the root to this node (e.g., "/root/folder/item").
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Gets the depth of this node in the tree (0 for root, 1 for immediate children, etc.).
    /// </summary>
    int Depth { get; }
}