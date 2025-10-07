namespace Mesch.RealTree;

/// <summary>
/// Service interface for performing tree operations with middleware actions and event support.
/// This is the primary interface for all tree modifications, providing type-safe operations with extensibility.
/// </summary>
public interface IRealTreeOperations
{
    // Container operations

    /// <summary>
    /// Creates and adds a new container to the specified parent node.
    /// </summary>
    /// <param name="parent">The parent node that will contain the new container.</param>
    /// <param name="id">Optional unique identifier for the container. If null, a new GUID will be generated.</param>
    /// <param name="name">The name of the new container.</param>
    /// <param name="triggerActions">Whether to execute registered middleware actions.</param>
    /// <param name="triggerEvents">Whether to fire registered event handlers.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The newly created and added container.</returns>
    Task<IRealTreeContainer> AddContainerAsync(IRealTreeNode parent, Guid? id, string name, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an existing container to the specified parent node.
    /// </summary>
    /// <param name="parent">The parent node that will contain the container.</param>
    /// <param name="container">The container to add.</param>
    /// <param name="triggerActions">Whether to execute registered middleware actions.</param>
    /// <param name="triggerEvents">Whether to fire registered event handlers.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The added container.</returns>
    Task<IRealTreeContainer> AddContainerAsync(IRealTreeNode parent, IRealTreeContainer container, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    // Item operations

    /// <summary>
    /// Creates and adds a new item to the specified parent container.
    /// </summary>
    /// <param name="parent">The parent container that will contain the new item.</param>
    /// <param name="id">Optional unique identifier for the item. If null, a new GUID will be generated.</param>
    /// <param name="name">The name of the new item.</param>
    /// <param name="triggerActions">Whether to execute registered middleware actions.</param>
    /// <param name="triggerEvents">Whether to fire registered event handlers.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The newly created and added item.</returns>
    Task<IRealTreeItem> AddItemAsync(IRealTreeContainer parent, Guid? id, string name, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an existing item to the specified parent container.
    /// </summary>
    /// <param name="parent">The parent container that will contain the item.</param>
    /// <param name="item">The item to add.</param>
    /// <param name="triggerActions">Whether to execute registered middleware actions.</param>
    /// <param name="triggerEvents">Whether to fire registered event handlers.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The added item.</returns>
    Task<IRealTreeItem> AddItemAsync(IRealTreeContainer parent, IRealTreeItem item, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    // Remove operations

    /// <summary>
    /// Removes a node from its parent, along with all of its descendants.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    /// <param name="triggerActions">Whether to execute registered middleware actions.</param>
    /// <param name="triggerEvents">Whether to fire registered event handlers.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task RemoveAsync(IRealTreeNode node, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all containers from the specified parent node.
    /// </summary>
    /// <param name="parent">The parent node whose containers will be removed.</param>
    /// <param name="triggerActions">Whether to execute registered middleware actions.</param>
    /// <param name="triggerEvents">Whether to fire registered event handlers.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task RemoveAllContainersAsync(IRealTreeNode parent, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all items from the specified parent container.
    /// </summary>
    /// <param name="parent">The parent container whose items will be removed.</param>
    /// <param name="triggerActions">Whether to execute registered middleware actions.</param>
    /// <param name="triggerEvents">Whether to fire registered event handlers.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task RemoveAllItemsAsync(IRealTreeContainer parent, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    // Update operations

    /// <summary>
    /// Updates a node's name and/or metadata.
    /// </summary>
    /// <param name="node">The node to update.</param>
    /// <param name="newName">The new name for the node, or null to keep the current name.</param>
    /// <param name="newMetadata">The new metadata dictionary, or null to keep current metadata.</param>
    /// <param name="triggerActions">Whether to execute registered middleware actions.</param>
    /// <param name="triggerEvents">Whether to fire registered event handlers.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task UpdateAsync(IRealTreeNode node, string? newName = null, Dictionary<string, object>? newMetadata = null, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    // Move operations

    /// <summary>
    /// Moves a node from its current parent to a new parent.
    /// Validates that the move would not create a cyclic reference.
    /// </summary>
    /// <param name="node">The node to move.</param>
    /// <param name="newParent">The new parent node.</param>
    /// <param name="triggerActions">Whether to execute registered middleware actions.</param>
    /// <param name="triggerEvents">Whether to fire registered event handlers.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <exception cref="CyclicReferenceException">Thrown if the move would create a cycle.</exception>
    Task MoveAsync(IRealTreeNode node, IRealTreeNode newParent, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    // Bulk operations

    /// <summary>
    /// Adds multiple containers to a parent node in a single operation.
    /// More efficient than individual additions for large numbers of containers.
    /// </summary>
    /// <param name="parent">The parent node that will contain the containers.</param>
    /// <param name="containers">The containers to add.</param>
    /// <param name="triggerActions">Whether to execute registered middleware actions.</param>
    /// <param name="triggerEvents">Whether to fire registered event handlers.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task BulkAddContainersAsync(IRealTreeNode parent, IEnumerable<IRealTreeContainer> containers, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple items to a parent container in a single operation.
    /// More efficient than individual additions for large numbers of items.
    /// </summary>
    /// <param name="parent">The parent container that will contain the items.</param>
    /// <param name="items">The items to add.</param>
    /// <param name="triggerActions">Whether to execute registered middleware actions.</param>
    /// <param name="triggerEvents">Whether to fire registered event handlers.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task BulkAddItemsAsync(IRealTreeContainer parent, IEnumerable<IRealTreeItem> items, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes multiple nodes in a single operation.
    /// More efficient than individual removals for large numbers of nodes.
    /// </summary>
    /// <param name="nodes">The nodes to remove.</param>
    /// <param name="triggerActions">Whether to execute registered middleware actions.</param>
    /// <param name="triggerEvents">Whether to fire registered event handlers.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task BulkRemoveAsync(IEnumerable<IRealTreeNode> nodes, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a deep copy of a container and all its descendants, adding it to the specified destination.
    /// </summary>
    /// <param name="source">The container to copy.</param>
    /// <param name="destination">The destination parent node.</param>
    /// <param name="newId">Optional new ID for the copied container. If null, a new GUID will be generated.</param>
    /// <param name="newName">Optional new name for the copied container. If null, the source name will be used.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The newly created copy of the container.</returns>
    Task<IRealTreeContainer> CopyContainerAsync(IRealTreeContainer source, IRealTreeNode destination, Guid? newId = null, string? newName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a deep copy of an item and all its descendants, adding it to the specified destination.
    /// </summary>
    /// <param name="source">The item to copy.</param>
    /// <param name="destination">The destination parent container.</param>
    /// <param name="newId">Optional new ID for the copied item. If null, a new GUID will be generated.</param>
    /// <param name="newName">Optional new name for the copied item. If null, the source name will be used.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The newly created copy of the item.</returns>
    Task<IRealTreeItem> CopyItemAsync(IRealTreeItem source, IRealTreeContainer destination, Guid? newId = null, string? newName = null, CancellationToken cancellationToken = default);

    // Query operations

    /// <summary>
    /// Lists the contents of a container with optional filtering and middleware support.
    /// </summary>
    /// <param name="container">The container whose contents should be listed.</param>
    /// <param name="includeContainers">Whether to include child containers in the results.</param>
    /// <param name="includeItems">Whether to include child items in the results.</param>
    /// <param name="recursive">Whether to recursively list all descendants.</param>
    /// <param name="triggerActions">Whether to execute registered middleware actions.</param>
    /// <param name="triggerEvents">Whether to fire registered event handlers.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A list of nodes matching the specified criteria.</returns>
    Task<IReadOnlyList<IRealTreeNode>> ListContainerAsync(IRealTreeContainer container, bool includeContainers = true, bool includeItems = true, bool recursive = false, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Shows/displays an item with middleware support for custom rendering or validation.
    /// </summary>
    /// <param name="item">The item to show.</param>
    /// <param name="triggerActions">Whether to execute registered middleware actions.</param>
    /// <param name="triggerEvents">Whether to fire registered event handlers.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ShowItemAsync(IRealTreeItem item, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);
}