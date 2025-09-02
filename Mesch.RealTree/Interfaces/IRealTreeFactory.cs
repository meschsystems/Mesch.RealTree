namespace Mesch.RealTree;

/// <summary>
/// Factory interface for creating tree nodes and root trees.
/// Provides a consistent way to instantiate tree components with optional configuration.
/// </summary>
public interface IRealTreeFactory
{
    /// <summary>
    /// Creates a new root tree instance.
    /// </summary>
    /// <param name="id">Optional unique identifier for the tree root. If null, a new GUID will be generated.</param>
    /// <param name="name">Optional name for the tree root. If null, defaults to "Root".</param>
    /// <returns>A new tree instance ready for use.</returns>
    IRealTree CreateTree(Guid? id = null, string? name = null);

    /// <summary>
    /// Creates a new container instance that can hold other containers and items.
    /// </summary>
    /// <param name="id">Optional unique identifier for the container. If null, a new GUID will be generated.</param>
    /// <param name="name">Optional name for the container. If null, the ID will be used as the name.</param>
    /// <returns>A new container instance ready to be added to a tree.</returns>
    IRealTreeContainer CreateContainer(Guid? id = null, string? name = null);

    /// <summary>
    /// Creates a new item instance that can hold containers but not other items.
    /// </summary>
    /// <param name="id">Optional unique identifier for the item. If null, a new GUID will be generated.</param>
    /// <param name="name">Optional name for the item. If null, the ID will be used as the name.</param>
    /// <returns>A new item instance ready to be added to a tree.</returns>
    IRealTreeItem CreateItem(Guid? id = null, string? name = null);
}