namespace Mesch.RealTree;
/// <summary>
/// Default factory implementation for creating tree nodes with standard behavior.
/// Provides straightforward instantiation of tree components without additional configuration.
/// </summary>
public class RealTreeFactory : IRealTreeFactory
{
    /// <summary>
    /// Creates a new root tree instance with default configuration.
    /// </summary>
    /// <param name="id">Optional unique identifier for the tree root. If null, a new GUID will be generated.</param>
    /// <param name="name">Optional name for the tree root. If null, defaults to "Root".</param>
    /// <returns>A new RealTreeRoot instance ready for use.</returns>
    public IRealTree CreateTree(Guid? id = null, string? name = null)
    {
        return new RealTreeRoot(id, name);
    }

    /// <summary>
    /// Creates a new container instance with default configuration.
    /// </summary>
    /// <param name="id">Optional unique identifier for the container. If null, a new GUID will be generated.</param>
    /// <param name="name">Optional name for the container. If null, the ID will be used as the name.</param>
    /// <returns>A new RealTreeContainer instance ready to be added to a tree.</returns>
    public IRealTreeContainer CreateContainer(Guid? id = null, string? name = null)
    {
        return new RealTreeContainer(id, name);
    }

    /// <summary>
    /// Creates a new item instance with default configuration.
    /// </summary>
    /// <param name="id">Optional unique identifier for the item. If null, a new GUID will be generated.</param>
    /// <param name="name">Optional name for the item. If null, the ID will be used as the name.</param>
    /// <returns>A new RealTreeItem instance ready to be added to a tree.</returns>
    public IRealTreeItem CreateItem(Guid? id = null, string? name = null)
    {
        return new RealTreeItem(id, name);
    }
}