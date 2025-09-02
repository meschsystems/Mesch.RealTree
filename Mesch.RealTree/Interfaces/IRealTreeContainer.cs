namespace Mesch.RealTree;

/// <summary>
/// Represents a container that can hold other containers and items.
/// Containers form the structural backbone of the tree and support middleware actions and events.
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
    /// <param name="container">The container to add.</param>
    void AddContainer(IRealTreeContainer container);

    /// <summary>
    /// Removes a container from this container's children.
    /// </summary>
    /// <param name="container">The container to remove.</param>
    void RemoveContainer(IRealTreeContainer container);

    /// <summary>
    /// Adds an item as a direct child of this container.
    /// </summary>
    /// <param name="item">The item to add.</param>
    void AddItem(IRealTreeItem item);

    /// <summary>
    /// Removes an item from this container's children.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    void RemoveItem(IRealTreeItem item);

    // Action registration methods (middleware - can intercept/modify operations)

    /// <summary>
    /// Registers a middleware action that executes when containers are added to this node or its descendants.
    /// Actions can intercept, modify, or cancel operations.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterAddContainerAction(AddContainerDelegate handler);

    /// <summary>
    /// Registers a middleware action that executes when containers are removed from this node or its descendants.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterRemoveContainerAction(RemoveContainerDelegate handler);

    /// <summary>
    /// Registers a middleware action that executes when items are added to this node or its descendants.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterAddItemAction(AddItemDelegate handler);

    /// <summary>
    /// Registers a middleware action that executes when items are removed from this node or its descendants.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterRemoveItemAction(RemoveItemDelegate handler);

    /// <summary>
    /// Registers a middleware action that executes when nodes are updated in this subtree.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterUpdateAction(UpdateNodeDelegate handler);

    /// <summary>
    /// Registers a middleware action that executes when nodes are moved in this subtree.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterMoveAction(MoveNodeDelegate handler);

    /// <summary>
    /// Registers a middleware action that executes during bulk container addition operations.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterBulkAddContainerAction(BulkAddContainerDelegate handler);

    /// <summary>
    /// Registers a middleware action that executes during bulk item addition operations.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterBulkAddItemAction(BulkAddItemDelegate handler);

    /// <summary>
    /// Registers a middleware action that executes during bulk removal operations.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterBulkRemoveAction(BulkRemoveContainerDelegate handler);

    // Action unregistration methods

    /// <summary>
    /// Unregisters a previously registered add container action.
    /// </summary>
    /// <param name="handler">The action handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterAddContainerAction(AddContainerDelegate handler);

    /// <summary>
    /// Unregisters a previously registered remove container action.
    /// </summary>
    /// <param name="handler">The action handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterRemoveContainerAction(RemoveContainerDelegate handler);

    /// <summary>
    /// Unregisters a previously registered add item action.
    /// </summary>
    /// <param name="handler">The action handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterAddItemAction(AddItemDelegate handler);

    /// <summary>
    /// Unregisters a previously registered remove item action.
    /// </summary>
    /// <param name="handler">The action handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterRemoveItemAction(RemoveItemDelegate handler);

    /// <summary>
    /// Unregisters a previously registered update action.
    /// </summary>
    /// <param name="handler">The action handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterUpdateAction(UpdateNodeDelegate handler);

    /// <summary>
    /// Unregisters a previously registered move action.
    /// </summary>
    /// <param name="handler">The action handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterMoveAction(MoveNodeDelegate handler);

    /// <summary>
    /// Unregisters a previously registered bulk add container action.
    /// </summary>
    /// <param name="handler">The action handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterBulkAddContainerAction(BulkAddContainerDelegate handler);

    /// <summary>
    /// Unregisters a previously registered bulk add item action.
    /// </summary>
    /// <param name="handler">The action handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterBulkAddItemAction(BulkAddItemDelegate handler);

    /// <summary>
    /// Unregisters a previously registered bulk remove action.
    /// </summary>
    /// <param name="handler">The action handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterBulkRemoveAction(BulkRemoveContainerDelegate handler);

    // Event registration methods (notifications - called after operations complete)

    /// <summary>
    /// Registers an event handler that executes after containers are added to this subtree.
    /// Events are fire-and-forget and run in parallel.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterContainerAddedEvent(ContainerAddedEventDelegate handler);

    /// <summary>
    /// Registers an event handler that executes after containers are removed from this subtree.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterContainerRemovedEvent(ContainerRemovedEventDelegate handler);

    /// <summary>
    /// Registers an event handler that executes after items are added to this subtree.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterItemAddedEvent(ItemAddedEventDelegate handler);

    /// <summary>
    /// Registers an event handler that executes after items are removed from this subtree.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterItemRemovedEvent(ItemRemovedEventDelegate handler);

    /// <summary>
    /// Registers an event handler that executes after nodes are updated in this subtree.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterNodeUpdatedEvent(NodeUpdatedEventDelegate handler);

    /// <summary>
    /// Registers an event handler that executes after nodes are moved in this subtree.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterNodeMovedEvent(NodeMovedEventDelegate handler);

    /// <summary>
    /// Registers an event handler that executes after bulk container additions in this subtree.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterBulkContainersAddedEvent(BulkContainersAddedEventDelegate handler);

    /// <summary>
    /// Registers an event handler that executes after bulk item additions in this subtree.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterBulkItemsAddedEvent(BulkItemsAddedEventDelegate handler);

    /// <summary>
    /// Registers an event handler that executes after bulk removals in this subtree.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterBulkNodesRemovedEvent(BulkNodesRemovedEventDelegate handler);

    // Event unregistration methods

    /// <summary>
    /// Unregisters a previously registered container added event handler.
    /// </summary>
    /// <param name="handler">The event handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterContainerAddedEvent(ContainerAddedEventDelegate handler);

    /// <summary>
    /// Unregisters a previously registered container removed event handler.
    /// </summary>
    /// <param name="handler">The event handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterContainerRemovedEvent(ContainerRemovedEventDelegate handler);

    /// <summary>
    /// Unregisters a previously registered item added event handler.
    /// </summary>
    /// <param name="handler">The event handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterItemAddedEvent(ItemAddedEventDelegate handler);

    /// <summary>
    /// Unregisters a previously registered item removed event handler.
    /// </summary>
    /// <param name="handler">The event handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterItemRemovedEvent(ItemRemovedEventDelegate handler);

    /// <summary>
    /// Unregisters a previously registered node updated event handler.
    /// </summary>
    /// <param name="handler">The event handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterNodeUpdatedEvent(NodeUpdatedEventDelegate handler);

    /// <summary>
    /// Unregisters a previously registered node moved event handler.
    /// </summary>
    /// <param name="handler">The event handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterNodeMovedEvent(NodeMovedEventDelegate handler);

    /// <summary>
    /// Unregisters a previously registered bulk containers added event handler.
    /// </summary>
    /// <param name="handler">The event handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterBulkContainersAddedEvent(BulkContainersAddedEventDelegate handler);

    /// <summary>
    /// Unregisters a previously registered bulk items added event handler.
    /// </summary>
    /// <param name="handler">The event handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterBulkItemsAddedEvent(BulkItemsAddedEventDelegate handler);

    /// <summary>
    /// Unregisters a previously registered bulk nodes removed event handler.
    /// </summary>
    /// <param name="handler">The event handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterBulkNodesRemovedEvent(BulkNodesRemovedEventDelegate handler);
}