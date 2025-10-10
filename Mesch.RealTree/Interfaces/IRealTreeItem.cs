namespace Mesch.RealTree;

/// <summary>
/// Represents an item that can contain containers but not other items.
/// Items are leaf-like nodes that can hold hierarchical content.
/// </summary>
public interface IRealTreeItem : IRealTreeNode
{
    /// <summary>
    /// Gets the collection of child containers within this item.
    /// </summary>
    IReadOnlyList<IRealTreeContainer> Containers { get; }

    /// <summary>
    /// Adds a container as a direct child of this item.
    /// </summary>
    void AddContainer(IRealTreeContainer container);

    /// <summary>
    /// Removes a container from this item's children.
    /// </summary>
    void RemoveContainer(IRealTreeContainer container);
}
