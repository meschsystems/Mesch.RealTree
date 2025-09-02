namespace Mesch.RealTree;

/// <summary>
/// Represents the root tree structure that serves as the entry point for all tree operations.
/// Extends container functionality with global search capabilities and lifecycle management.
/// </summary>
public interface IRealTree : IRealTreeNode, IDisposable
{
    /// <summary>
    /// Gets the collection of root-level containers in this tree.
    /// </summary>
    IReadOnlyList<IRealTreeContainer> Containers { get; }

    /// <summary>
    /// Finds a node by its full path from the root (e.g., "/folder/subfolder/item").
    /// </summary>
    /// <param name="path">The path to search for, starting with "/" for absolute paths.</param>
    /// <returns>The node at the specified path, or null if not found.</returns>
    IRealTreeNode? FindByPath(string path);

    /// <summary>
    /// Finds a node by its unique identifier, searching the entire tree.
    /// </summary>
    /// <param name="id">The unique identifier to search for.</param>
    /// <returns>The node with the specified ID, or null if not found.</returns>
    IRealTreeNode? FindById(Guid id);

    #region Global action registration at tree level (middleware)

    /// <summary>
    /// Registers a global middleware action for all container additions in the entire tree.
    /// </summary>
    /// <param name="handler">The action handler to register globally.</param>
    void RegisterAddContainerAction(AddContainerDelegate handler);

    /// <summary>
    /// Registers a global middleware action for all container removals in the entire tree.
    /// </summary>
    /// <param name="handler">The action handler to register globally.</param>
    void RegisterRemoveContainerAction(RemoveContainerDelegate handler);

    /// <summary>
    /// Registers a global middleware action for all item additions in the entire tree.
    /// </summary>
    /// <param name="handler">The action handler to register globally.</param>
    void RegisterAddItemAction(AddItemDelegate handler);

    /// <summary>
    /// Registers a global middleware action for all item removals in the entire tree.
    /// </summary>
    /// <param name="handler">The action handler to register globally.</param>
    void RegisterRemoveItemAction(RemoveItemDelegate handler);

    /// <summary>
    /// Registers a global middleware action for all node updates in the entire tree.
    /// </summary>
    /// <param name="handler">The action handler to register globally.</param>
    void RegisterUpdateAction(UpdateNodeDelegate handler);

    /// <summary>
    /// Registers a global middleware action for all node moves in the entire tree.
    /// </summary>
    /// <param name="handler">The action handler to register globally.</param>
    void RegisterMoveAction(MoveNodeDelegate handler);

    /// <summary>
    /// Registers a global middleware action for all bulk container additions in the entire tree.
    /// </summary>
    /// <param name="handler">The action handler to register globally.</param>
    void RegisterBulkAddContainerAction(BulkAddContainerDelegate handler);

    /// <summary>
    /// Registers a global middleware action for all bulk item additions in the entire tree.
    /// </summary>
    /// <param name="handler">The action handler to register globally.</param>
    void RegisterBulkAddItemAction(BulkAddItemDelegate handler);

    /// <summary>
    /// Registers a global middleware action for all bulk removals in the entire tree.
    /// </summary>
    /// <param name="handler">The action handler to register globally.</param>
    void RegisterBulkRemoveAction(BulkRemoveContainerDelegate handler);

    /// <summary>
    /// Registers a global middleware action for all container listing operations in the entire tree.
    /// </summary>
    /// <param name="handler">The action handler to register globally.</param>
    void RegisterListContainerAction(ListContainerDelegate handler);

#endregion

    #region Global action deregistration at tree level

    /// <summary>
    /// Deregisters a previously registered global add container action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterAddContainerAction(AddContainerDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global remove container action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterRemoveContainerAction(RemoveContainerDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global add item action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterAddItemAction(AddItemDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global remove item action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterRemoveItemAction(RemoveItemDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global update action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterUpdateAction(UpdateNodeDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global move action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterMoveAction(MoveNodeDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global bulk add container action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterBulkAddContainerAction(BulkAddContainerDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global bulk add item action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterBulkAddItemAction(BulkAddItemDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global bulk remove action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterBulkRemoveAction(BulkRemoveContainerDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global list container action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterListContainerAction(ListContainerDelegate handler);

    #endregion

    #region Global event registration at tree level (notifications)

    /// <summary>
    /// Registers a global event handler for all container additions in the entire tree.
    /// </summary>
    /// <param name="handler">The event handler to register globally.</param>
    void RegisterContainerAddedEvent(ContainerAddedEventDelegate handler);

    /// <summary>
    /// Registers a global event handler for all container removals in the entire tree.
    /// </summary>
    /// <param name="handler">The event handler to register globally.</param>
    void RegisterContainerRemovedEvent(ContainerRemovedEventDelegate handler);

    /// <summary>
    /// Registers a global event handler for all item additions in the entire tree.
    /// </summary>
    /// <param name="handler">The event handler to register globally.</param>
    void RegisterItemAddedEvent(ItemAddedEventDelegate handler);

    /// <summary>
    /// Registers a global event handler for all item removals in the entire tree.
    /// </summary>
    /// <param name="handler">The event handler to register globally.</param>
    void RegisterItemRemovedEvent(ItemRemovedEventDelegate handler);

    /// <summary>
    /// Registers a global event handler for all node updates in the entire tree.
    /// </summary>
    /// <param name="handler">The event handler to register globally.</param>
    void RegisterNodeUpdatedEvent(NodeUpdatedEventDelegate handler);

    /// <summary>
    /// Registers a global event handler for all node moves in the entire tree.
    /// </summary>
    /// <param name="handler">The event handler to register globally.</param>
    void RegisterNodeMovedEvent(NodeMovedEventDelegate handler);

    /// <summary>
    /// Registers a global event handler for all bulk container additions in the entire tree.
    /// </summary>
    /// <param name="handler">The event handler to register globally.</param>
    void RegisterBulkContainersAddedEvent(BulkContainersAddedEventDelegate handler);

    /// <summary>
    /// Registers a global event handler for all bulk item additions in the entire tree.
    /// </summary>
    /// <param name="handler">The event handler to register globally.</param>
    void RegisterBulkItemsAddedEvent(BulkItemsAddedEventDelegate handler);

    /// <summary>
    /// Registers a global event handler for all bulk removals in the entire tree.
    /// </summary>
    /// <param name="handler">The event handler to register globally.</param>
    void RegisterBulkNodesRemovedEvent(BulkNodesRemovedEventDelegate handler);

    /// <summary>
    /// Registers a global event handler for all container listing operations in the entire tree.
    /// </summary>
    /// <param name="handler">The event handler to register globally.</param>
    void RegisterContainerListedEvent(ContainerListedEventDelegate handler);

    #endregion

    #region Global event deregistration at tree level

    /// <summary>
    /// Deregisters a previously registered global container added event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterContainerAddedEvent(ContainerAddedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global container removed event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterContainerRemovedEvent(ContainerRemovedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global item added event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterItemAddedEvent(ItemAddedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global item removed event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterItemRemovedEvent(ItemRemovedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global node updated event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterNodeUpdatedEvent(NodeUpdatedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global node moved event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterNodeMovedEvent(NodeMovedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global bulk containers added event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterBulkContainersAddedEvent(BulkContainersAddedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global bulk items added event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterBulkItemsAddedEvent(BulkItemsAddedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global bulk nodes removed event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterBulkNodesRemovedEvent(BulkNodesRemovedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered global container listed event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterContainerListedEvent(ContainerListedEventDelegate handler);

    #endregion
}