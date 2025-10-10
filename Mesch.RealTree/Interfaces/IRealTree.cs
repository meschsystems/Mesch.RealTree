namespace Mesch.RealTree;

/// <summary>
/// Represents the root tree structure that serves as the entry point for all tree operations.
/// Extends node functionality with global search capabilities.
/// </summary>
public interface IRealTree : IRealTreeNode, IDisposable
{
    /// <summary>
    /// Gets the collection of root-level containers in this tree.
    /// </summary>
    IReadOnlyList<IRealTreeContainer> Containers { get; }

    /// <summary>
    /// Adds a container at the root level of this tree.
    /// </summary>
    void AddContainer(IRealTreeContainer container);

    /// <summary>
    /// Removes a container from the root level of this tree.
    /// </summary>
    void RemoveContainer(IRealTreeContainer container);

    /// <summary>
    /// Finds a node by its full path from the root (e.g., "/folder/subfolder/item").
    /// </summary>
    IRealTreeNode? FindByPath(string path);

    /// <summary>
    /// Finds a node by its unique identifier, searching the entire tree.
    /// </summary>
    IRealTreeNode? FindById(Guid id);
}
