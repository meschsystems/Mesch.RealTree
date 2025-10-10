namespace Mesch.RealTree;

/// <summary>
/// Represents a container that can hold other containers and items.
/// Containers form the structural backbone of the tree.
/// </summary>
public interface IRealTreeContainer : IRealTreeNode
{
    /// <summary>
    /// Gets the collection of child containers within this container.
    /// </summary>
    IReadOnlyList<IRealTreeContainer> Containers { get; }

    /// <summary>
    /// Gets the collection of child items within this container.
    /// </summary>
    IReadOnlyList<IRealTreeItem> Items { get; }

    /// <summary>
    /// Gets the combined collection of all child nodes (containers and items).
    /// </summary>
    IReadOnlyList<IRealTreeNode> Children { get; }

    /// <summary>
    /// Adds a container as a direct child of this container.
    /// </summary>
    void AddContainer(IRealTreeContainer container);

    /// <summary>
    /// Removes a container from this container's children.
    /// </summary>
    void RemoveContainer(IRealTreeContainer container);

    /// <summary>
    /// Adds an item as a direct child of this container.
    /// </summary>
    void AddItem(IRealTreeItem item);

    /// <summary>
    /// Removes an item from this container's children.
    /// </summary>
    void RemoveItem(IRealTreeItem item);
}
