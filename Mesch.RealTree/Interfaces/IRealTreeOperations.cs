namespace Mesch.RealTree;

/// <summary>
/// Service interface for performing tree operations with type-based middleware and event support.
/// This is the primary interface for all tree modifications, providing strongly-typed operations.
/// </summary>
public interface IRealTreeOperations
{
    // ========================================
    // TYPED CREATION WITH METADATA SUPPORT
    // ========================================

    #region Container Add Operations

    /// <summary>
    /// Creates and adds a new strongly-typed container with optional metadata.
    /// Metadata is set before middleware executes, allowing middleware to read initial metadata values.
    /// </summary>
    /// <typeparam name="T">The specific container type to create. Must have a parameterless constructor.</typeparam>
    /// <param name="parent">The parent node to add the container to.</param>
    /// <param name="id">Optional ID for the container. If null, a new GUID is generated.</param>
    /// <param name="name">The name of the container.</param>
    /// <param name="metadata">Optional metadata to attach to the container before middleware executes.</param>
    /// <param name="triggerActions">Whether to execute middleware actions.</param>
    /// <param name="triggerEvents">Reserved for future use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created and added container of type T.</returns>
    Task<T> AddContainerAsync<T>(
        IRealTreeNode parent,
        Guid? id,
        string name,
        IDictionary<string, object>? metadata = null,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
        where T : IRealTreeContainer, new();

    /// <summary>
    /// Creates and adds a new container with optional metadata using the factory's default container type.
    /// Metadata is set before middleware executes, allowing middleware to read initial metadata values.
    /// </summary>
    /// <param name="parent">The parent node to add the container to.</param>
    /// <param name="id">Optional ID for the container. If null, a new GUID is generated.</param>
    /// <param name="name">The name of the container.</param>
    /// <param name="metadata">Optional metadata to attach to the container before middleware executes.</param>
    /// <param name="triggerActions">Whether to execute middleware actions.</param>
    /// <param name="triggerEvents">Reserved for future use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created and added container.</returns>
    Task<IRealTreeContainer> AddContainerAsync(
        IRealTreeNode parent,
        Guid? id,
        string name,
        IDictionary<string, object>? metadata = null,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default);

    #endregion

    #region Item Add Operations

    /// <summary>
    /// Creates and adds a new strongly-typed item with optional metadata.
    /// Metadata is set before middleware executes, allowing middleware to read initial metadata values.
    /// </summary>
    /// <typeparam name="T">The specific item type to create. Must have a parameterless constructor.</typeparam>
    /// <param name="parent">The parent container to add the item to.</param>
    /// <param name="id">Optional ID for the item. If null, a new GUID is generated.</param>
    /// <param name="name">The name of the item.</param>
    /// <param name="metadata">Optional metadata to attach to the item before middleware executes.</param>
    /// <param name="triggerActions">Whether to execute middleware actions.</param>
    /// <param name="triggerEvents">Reserved for future use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created and added item of type T.</returns>
    Task<T> AddItemAsync<T>(
        IRealTreeContainer parent,
        Guid? id,
        string name,
        IDictionary<string, object>? metadata = null,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
        where T : IRealTreeItem, new();

    /// <summary>
    /// Creates and adds a new item with optional metadata using the factory's default item type.
    /// Metadata is set before middleware executes, allowing middleware to read initial metadata values.
    /// </summary>
    /// <param name="parent">The parent container to add the item to.</param>
    /// <param name="id">Optional ID for the item. If null, a new GUID is generated.</param>
    /// <param name="name">The name of the item.</param>
    /// <param name="metadata">Optional metadata to attach to the item before middleware executes.</param>
    /// <param name="triggerActions">Whether to execute middleware actions.</param>
    /// <param name="triggerEvents">Reserved for future use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created and added item.</returns>
    Task<IRealTreeItem> AddItemAsync(
        IRealTreeContainer parent,
        Guid? id,
        string name,
        IDictionary<string, object>? metadata = null,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default);

    #endregion

    // ========================================
    // REMOVE OPERATIONS
    // ========================================

    /// <summary>
    /// Removes a node from the tree. Cannot remove the root node.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    /// <param name="triggerActions">Whether to execute middleware actions.</param>
    /// <param name="triggerEvents">Reserved for future use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="InvalidOperationException">Thrown when attempting to remove the root node.</exception>
    Task RemoveAsync(IRealTreeNode node, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all child containers from a parent node.
    /// </summary>
    /// <param name="parent">The parent node whose containers should be removed.</param>
    /// <param name="triggerActions">Whether to execute middleware actions for each removal.</param>
    /// <param name="triggerEvents">Reserved for future use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveAllContainersAsync(IRealTreeNode parent, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all child items from a container.
    /// </summary>
    /// <param name="parent">The container whose items should be removed.</param>
    /// <param name="triggerActions">Whether to execute middleware actions for each removal.</param>
    /// <param name="triggerEvents">Reserved for future use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveAllItemsAsync(IRealTreeContainer parent, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    // ========================================
    // UPDATE AND MOVE OPERATIONS
    // ========================================

    /// <summary>
    /// Updates a node's name and/or metadata. Metadata is replaced, not merged.
    /// </summary>
    /// <param name="node">The node to update.</param>
    /// <param name="newName">Optional new name for the node.</param>
    /// <param name="newMetadata">Optional new metadata to replace existing metadata.</param>
    /// <param name="triggerActions">Whether to execute middleware actions.</param>
    /// <param name="triggerEvents">Reserved for future use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(IRealTreeNode node, string? newName = null, Dictionary<string, object>? newMetadata = null, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a node to a new parent. Cannot move the root node or create cyclic references.
    /// </summary>
    /// <param name="node">The node to move.</param>
    /// <param name="newParent">The new parent node.</param>
    /// <param name="triggerActions">Whether to execute middleware actions.</param>
    /// <param name="triggerEvents">Reserved for future use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="InvalidOperationException">Thrown when attempting to move the root node.</exception>
    /// <exception cref="CyclicReferenceException">Thrown when the move would create a cycle (moving to self or descendant).</exception>
    Task MoveAsync(IRealTreeNode node, IRealTreeNode newParent, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    // ========================================
    // BULK OPERATIONS WITH METADATA
    // ========================================

    /// <summary>
    /// Adds multiple containers in a single operation. Empty collections are skipped (no middleware executed).
    /// Bulk middleware executes once, then individual add operations execute without triggering individual middleware.
    /// </summary>
    /// <typeparam name="T">The type of containers to create.</typeparam>
    /// <param name="parent">The parent node to add containers to.</param>
    /// <param name="items">Collection of tuples containing (id, name, metadata) for each container.</param>
    /// <param name="triggerActions">Whether to execute bulk middleware. Individual middleware is always skipped.</param>
    /// <param name="triggerEvents">Reserved for future use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task BulkAddContainersAsync<T>(
        IRealTreeNode parent,
        IEnumerable<(Guid? id, string name, IDictionary<string, object>? metadata)> items,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
        where T : IRealTreeContainer, new();

    /// <summary>
    /// Adds multiple items in a single operation. Empty collections are skipped (no middleware executed).
    /// Bulk middleware executes once, then individual add operations execute without triggering individual middleware.
    /// </summary>
    /// <typeparam name="T">The type of items to create.</typeparam>
    /// <param name="parent">The container to add items to.</param>
    /// <param name="items">Collection of tuples containing (id, name, metadata) for each item.</param>
    /// <param name="triggerActions">Whether to execute bulk middleware. Individual middleware is always skipped.</param>
    /// <param name="triggerEvents">Reserved for future use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task BulkAddItemsAsync<T>(
        IRealTreeContainer parent,
        IEnumerable<(Guid? id, string name, IDictionary<string, object>? metadata)> items,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
        where T : IRealTreeItem, new();

    /// <summary>
    /// Removes multiple nodes in a single operation.
    /// Bulk middleware executes once, then individual remove operations execute without triggering individual middleware.
    /// </summary>
    /// <param name="nodes">The nodes to remove.</param>
    /// <param name="triggerActions">Whether to execute bulk middleware. Individual middleware is always skipped.</param>
    /// <param name="triggerEvents">Reserved for future use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task BulkRemoveAsync(IEnumerable<IRealTreeNode> nodes, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    // ========================================
    // COPY OPERATIONS
    // ========================================

    /// <summary>
    /// Creates a copy of a container with optional new ID and name. Defaults to deep copy (includes all descendants).
    /// Metadata is copied from the source. Middleware executes for each node created.
    /// </summary>
    /// <param name="source">The container to copy.</param>
    /// <param name="destination">The parent where the copy will be placed.</param>
    /// <param name="newId">Optional ID for the copy. If null, a new GUID is generated.</param>
    /// <param name="newName">Optional name for the copy. If null, uses source name.</param>
    /// <param name="deep">Whether to copy all descendants. Default is true.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created container copy.</returns>
    Task<IRealTreeContainer> CopyContainerAsync(IRealTreeContainer source, IRealTreeNode destination, Guid? newId = null, string? newName = null, bool deep = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a copy of an item with optional new ID and name. Defaults to deep copy (includes child containers).
    /// Metadata is copied from the source. Middleware executes for each node created.
    /// </summary>
    /// <param name="source">The item to copy.</param>
    /// <param name="destination">The container where the copy will be placed.</param>
    /// <param name="newId">Optional ID for the copy. If null, a new GUID is generated.</param>
    /// <param name="newName">Optional name for the copy. If null, uses source name.</param>
    /// <param name="deep">Whether to copy child containers. Default is true.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created item copy.</returns>
    Task<IRealTreeItem> CopyItemAsync(IRealTreeItem source, IRealTreeContainer destination, Guid? newId = null, string? newName = null, bool deep = true, CancellationToken cancellationToken = default);

    // ========================================
    // QUERY OPERATIONS
    // ========================================

    /// <summary>
    /// Lists the children of a node with flexible filtering options.
    /// Both containers and items can contain child containers, so this accepts any node.
    /// When recursive is true, traverses into child containers even if includeContainers is false.
    /// Middleware can enrich context metadata with display hints or perform permission checks.
    /// </summary>
    /// <param name="node">The node to list (container or item).</param>
    /// <param name="includeContainers">Whether to include containers in the results.</param>
    /// <param name="includeItems">Whether to include items in the results.</param>
    /// <param name="recursive">Whether to recursively list descendants. Always traverses into children regardless of include flags.</param>
    /// <param name="triggerActions">Whether to execute middleware actions.</param>
    /// <param name="triggerEvents">Reserved for future use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of nodes matching the filter criteria.</returns>
    Task<IReadOnlyList<IRealTreeNode>> ListAsync(IRealTreeNode node, bool includeContainers = true, bool includeItems = true, bool recursive = false, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Shows/displays an item, allowing middleware to enrich context metadata with display information.
    /// This is a query operation - middleware should populate context metadata with display data.
    /// </summary>
    /// <param name="item">The item to show.</param>
    /// <param name="triggerActions">Whether to execute middleware actions.</param>
    /// <param name="triggerEvents">Reserved for future use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ShowItemAsync(IRealTreeItem item, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default);

    // ========================================
    // TYPE-BASED DELEGATE REGISTRATION
    // ========================================

    #region Action Registration

    /// <summary>
    /// Registers middleware for add container operations on a specific parent type.
    /// Executes only when adding containers to parents of type T.
    /// </summary>
    /// <typeparam name="T">The parent node type to register middleware for.</typeparam>
    /// <param name="handler">The middleware delegate to execute.</param>
    void RegisterAddContainerAction<T>(AddContainerDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Registers middleware for add item operations on a specific parent type.
    /// Executes only when adding items to parents of type T.
    /// </summary>
    /// <typeparam name="T">The parent container type to register middleware for.</typeparam>
    /// <param name="handler">The middleware delegate to execute.</param>
    void RegisterAddItemAction<T>(AddItemDelegate handler) where T : IRealTreeContainer;

    /// <summary>
    /// Registers middleware for remove container operations on a specific parent type.
    /// Executes only when removing containers from parents of type T.
    /// </summary>
    /// <typeparam name="T">The parent node type to register middleware for.</typeparam>
    /// <param name="handler">The middleware delegate to execute.</param>
    void RegisterRemoveContainerAction<T>(RemoveContainerDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Registers middleware for remove item operations on a specific parent type.
    /// Executes only when removing items from parents of type T.
    /// </summary>
    /// <typeparam name="T">The parent container type to register middleware for.</typeparam>
    /// <param name="handler">The middleware delegate to execute.</param>
    void RegisterRemoveItemAction<T>(RemoveItemDelegate handler) where T : IRealTreeContainer;

    /// <summary>
    /// Registers middleware for update operations on a specific node type.
    /// Executes only when updating nodes of type T.
    /// </summary>
    /// <typeparam name="T">The node type to register middleware for.</typeparam>
    /// <param name="handler">The middleware delegate to execute.</param>
    void RegisterUpdateAction<T>(UpdateNodeDelegate handler) where T : IRealTreeNode;


    /// <summary>
    /// Registers middleware for bulk add container operations on a specific parent type.
    /// Executes only when bulk adding to parents of type T.
    /// </summary>
    /// <typeparam name="T">The parent node type to register middleware for.</typeparam>
    /// <param name="handler">The middleware delegate to execute.</param>
    void RegisterBulkAddContainerAction<T>(BulkAddContainerDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Registers middleware for bulk add item operations on a specific parent type.
    /// Executes only when bulk adding to parents of type T.
    /// </summary>
    /// <typeparam name="T">The parent container type to register middleware for.</typeparam>
    /// <param name="handler">The middleware delegate to execute.</param>
    void RegisterBulkAddItemAction<T>(BulkAddItemDelegate handler) where T : IRealTreeContainer;

    /// <summary>
    /// Registers middleware for bulk remove operations on a specific parent type.
    /// Executes only when bulk removing from parents of type T.
    /// </summary>
    /// <typeparam name="T">The parent node type to register middleware for.</typeparam>
    /// <param name="handler">The middleware delegate to execute.</param>
    void RegisterBulkRemoveAction<T>(BulkRemoveContainerDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Registers middleware for list operations on a specific node type.
    /// Executes only when listing nodes of type T.
    /// This is a SELF operation - the node being listed owns the decision of how to list its contents.
    /// </summary>
    /// <typeparam name="T">The node type to register middleware for.</typeparam>
    /// <param name="handler">The middleware delegate to execute.</param>
    void RegisterListAction<T>(ListContainerDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Registers middleware for show item operations on a specific item type.
    /// Executes only when showing items of type T.
    /// </summary>
    /// <typeparam name="T">The item type to register middleware for.</typeparam>
    /// <param name="handler">The middleware delegate to execute.</param>
    void RegisterShowItemAction<T>(ShowItemDelegate handler) where T : IRealTreeItem;

    #endregion

    #region Action Deregistration

    /// <summary>
    /// Removes a previously registered add container middleware handler for a specific parent type.
    /// </summary>
    /// <typeparam name="T">The parent node type the handler was registered for.</typeparam>
    /// <param name="handler">The middleware delegate to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterAddContainerAction<T>(AddContainerDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Removes a previously registered add item middleware handler for a specific parent type.
    /// </summary>
    /// <typeparam name="T">The parent container type the handler was registered for.</typeparam>
    /// <param name="handler">The middleware delegate to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterAddItemAction<T>(AddItemDelegate handler) where T : IRealTreeContainer;

    /// <summary>
    /// Removes a previously registered remove container middleware handler for a specific parent type.
    /// </summary>
    /// <typeparam name="T">The parent node type the handler was registered for.</typeparam>
    /// <param name="handler">The middleware delegate to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterRemoveContainerAction<T>(RemoveContainerDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Removes a previously registered remove item middleware handler for a specific parent type.
    /// </summary>
    /// <typeparam name="T">The parent container type the handler was registered for.</typeparam>
    /// <param name="handler">The middleware delegate to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterRemoveItemAction<T>(RemoveItemDelegate handler) where T : IRealTreeContainer;

    /// <summary>
    /// Removes a previously registered update middleware handler for a specific node type.
    /// </summary>
    /// <typeparam name="T">The node type the handler was registered for.</typeparam>
    /// <param name="handler">The middleware delegate to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterUpdateAction<T>(UpdateNodeDelegate handler) where T : IRealTreeNode;


    /// <summary>
    /// Removes a previously registered bulk add container middleware handler for a specific parent type.
    /// </summary>
    /// <typeparam name="T">The parent node type the handler was registered for.</typeparam>
    /// <param name="handler">The middleware delegate to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterBulkAddContainerAction<T>(BulkAddContainerDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Removes a previously registered bulk add item middleware handler for a specific parent type.
    /// </summary>
    /// <typeparam name="T">The parent container type the handler was registered for.</typeparam>
    /// <param name="handler">The middleware delegate to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterBulkAddItemAction<T>(BulkAddItemDelegate handler) where T : IRealTreeContainer;

    /// <summary>
    /// Removes a previously registered bulk remove middleware handler for a specific parent type.
    /// </summary>
    /// <typeparam name="T">The parent node type the handler was registered for.</typeparam>
    /// <param name="handler">The middleware delegate to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterBulkRemoveAction<T>(BulkRemoveContainerDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Removes a previously registered list middleware handler for a specific node type.
    /// </summary>
    /// <typeparam name="T">The node type the handler was registered for.</typeparam>
    /// <param name="handler">The middleware delegate to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterListAction<T>(ListContainerDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Removes a previously registered show item middleware handler for a specific item type.
    /// </summary>
    /// <typeparam name="T">The item type the handler was registered for.</typeparam>
    /// <param name="handler">The middleware delegate to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterShowItemAction<T>(ShowItemDelegate handler) where T : IRealTreeItem;

    #endregion

    #region Event Registration

    // ========================================
    // BOUNDARY EVENTS (Parent-Fired)
    // ========================================

    /// <summary>
    /// Registers an event handler that executes after a container is successfully added.
    /// OWNERSHIP: Fired by the parent after accepting a new container into its collection.
    /// </summary>
    /// <typeparam name="T">The parent node type (container, item, or tree).</typeparam>
    /// <param name="handler">The event handler to register.</param>
    void RegisterContainerAddedEvent<T>(ContainerAddedEventDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Registers an event handler that executes after a container is successfully removed.
    /// OWNERSHIP: Fired by the parent after releasing a container from its collection.
    /// </summary>
    /// <typeparam name="T">The parent node type.</typeparam>
    /// <param name="handler">The event handler to register.</param>
    void RegisterContainerRemovedEvent<T>(ContainerRemovedEventDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Registers an event handler that executes after an item is successfully added.
    /// OWNERSHIP: Fired by the parent container after accepting a new item into its collection.
    /// </summary>
    /// <typeparam name="T">The parent container type.</typeparam>
    /// <param name="handler">The event handler to register.</param>
    void RegisterItemAddedEvent<T>(ItemAddedEventDelegate handler) where T : IRealTreeContainer;

    /// <summary>
    /// Registers an event handler that executes after an item is successfully removed.
    /// OWNERSHIP: Fired by the parent container after releasing an item from its collection.
    /// </summary>
    /// <typeparam name="T">The parent container type.</typeparam>
    /// <param name="handler">The event handler to register.</param>
    void RegisterItemRemovedEvent<T>(ItemRemovedEventDelegate handler) where T : IRealTreeContainer;

    /// <summary>
    /// Registers an event handler that executes after containers are successfully added in bulk.
    /// OWNERSHIP: Fired by the parent after accepting multiple containers.
    /// </summary>
    /// <typeparam name="T">The parent node type.</typeparam>
    /// <param name="handler">The event handler to register.</param>
    void RegisterBulkContainersAddedEvent<T>(BulkContainersAddedEventDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Registers an event handler that executes after items are successfully added in bulk.
    /// OWNERSHIP: Fired by the parent container after accepting multiple items.
    /// </summary>
    /// <typeparam name="T">The parent container type.</typeparam>
    /// <param name="handler">The event handler to register.</param>
    void RegisterBulkItemsAddedEvent<T>(BulkItemsAddedEventDelegate handler) where T : IRealTreeContainer;

    /// <summary>
    /// Registers an event handler that executes after nodes are successfully removed in bulk.
    /// OWNERSHIP: Fired by affected parents after releasing nodes.
    /// </summary>
    /// <typeparam name="T">The parent node type.</typeparam>
    /// <param name="handler">The event handler to register.</param>
    void RegisterBulkNodesRemovedEvent<T>(BulkNodesRemovedEventDelegate handler) where T : IRealTreeNode;

    // ========================================
    // SELF EVENTS (Node-Fired)
    // ========================================

    /// <summary>
    /// Registers an event handler that executes after a node is successfully updated.
    /// OWNERSHIP: Fired by the node after updating itself.
    /// </summary>
    /// <typeparam name="T">The node type being updated.</typeparam>
    /// <param name="handler">The event handler to register.</param>
    void RegisterNodeUpdatedEvent<T>(NodeUpdatedEventDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Registers an event handler that executes after a node is successfully moved.
    /// OWNERSHIP: Fired by the node after moving itself to a new parent.
    /// </summary>
    /// <typeparam name="T">The node type being moved.</typeparam>
    /// <param name="handler">The event handler to register.</param>
    void RegisterNodeMovedEvent<T>(NodeMovedEventDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Registers an event handler that executes after a node's contents are listed.
    /// OWNERSHIP: Fired by the node after listing its own contents.
    /// </summary>
    /// <typeparam name="T">The node type being listed.</typeparam>
    /// <param name="handler">The event handler to register.</param>
    void RegisterContainerListedEvent<T>(ContainerListedEventDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Registers an event handler that executes after an item is shown.
    /// OWNERSHIP: Fired by the item after showing itself.
    /// </summary>
    /// <typeparam name="T">The item type being shown.</typeparam>
    /// <param name="handler">The event handler to register.</param>
    void RegisterItemShownEvent<T>(ItemShownEventDelegate handler) where T : IRealTreeItem;

    // ========================================
    // EVENT DEREGISTRATION
    // ========================================

    /// <summary>
    /// Removes a previously registered container added event handler.
    /// </summary>
    /// <typeparam name="T">The parent node type.</typeparam>
    /// <param name="handler">The event handler to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterContainerAddedEvent<T>(ContainerAddedEventDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Removes a previously registered container removed event handler.
    /// </summary>
    /// <typeparam name="T">The parent node type.</typeparam>
    /// <param name="handler">The event handler to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterContainerRemovedEvent<T>(ContainerRemovedEventDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Removes a previously registered item added event handler.
    /// </summary>
    /// <typeparam name="T">The parent container type.</typeparam>
    /// <param name="handler">The event handler to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterItemAddedEvent<T>(ItemAddedEventDelegate handler) where T : IRealTreeContainer;

    /// <summary>
    /// Removes a previously registered item removed event handler.
    /// </summary>
    /// <typeparam name="T">The parent container type.</typeparam>
    /// <param name="handler">The event handler to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterItemRemovedEvent<T>(ItemRemovedEventDelegate handler) where T : IRealTreeContainer;

    /// <summary>
    /// Removes a previously registered node updated event handler.
    /// </summary>
    /// <typeparam name="T">The node type.</typeparam>
    /// <param name="handler">The event handler to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterNodeUpdatedEvent<T>(NodeUpdatedEventDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Removes a previously registered node moved event handler.
    /// </summary>
    /// <typeparam name="T">The node type.</typeparam>
    /// <param name="handler">The event handler to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterNodeMovedEvent<T>(NodeMovedEventDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Removes a previously registered bulk containers added event handler.
    /// </summary>
    /// <typeparam name="T">The parent node type.</typeparam>
    /// <param name="handler">The event handler to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterBulkContainersAddedEvent<T>(BulkContainersAddedEventDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Removes a previously registered bulk items added event handler.
    /// </summary>
    /// <typeparam name="T">The parent container type.</typeparam>
    /// <param name="handler">The event handler to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterBulkItemsAddedEvent<T>(BulkItemsAddedEventDelegate handler) where T : IRealTreeContainer;

    /// <summary>
    /// Removes a previously registered bulk nodes removed event handler.
    /// </summary>
    /// <typeparam name="T">The parent node type.</typeparam>
    /// <param name="handler">The event handler to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterBulkNodesRemovedEvent<T>(BulkNodesRemovedEventDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Removes a previously registered container listed event handler.
    /// </summary>
    /// <typeparam name="T">The node type.</typeparam>
    /// <param name="handler">The event handler to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterContainerListedEvent<T>(ContainerListedEventDelegate handler) where T : IRealTreeNode;

    /// <summary>
    /// Removes a previously registered item shown event handler.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="handler">The event handler to remove.</param>
    /// <returns>True if the handler was found and removed; otherwise false.</returns>
    bool DeregisterItemShownEvent<T>(ItemShownEventDelegate handler) where T : IRealTreeItem;

    #endregion
}
