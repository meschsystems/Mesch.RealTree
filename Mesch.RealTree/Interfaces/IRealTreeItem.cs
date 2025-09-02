namespace Mesch.RealTree;

/// <summary>
/// Represents an item that can contain containers but not other items.
/// Items are leaf-like nodes that can hold hierarchical content but cannot contain other items.
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
    /// <param name="container">The container to add.</param>
    void AddContainer(IRealTreeContainer container);

    /// <summary>
    /// Removes a container from this item's children.
    /// </summary>
    /// <param name="container">The container to remove.</param>
    void RemoveContainer(IRealTreeContainer container);

    // Action registration methods (middleware)

    /// <summary>
    /// Registers a middleware action that executes when containers are added to this item or its descendants.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterAddContainerAction(AddContainerDelegate handler);

    /// <summary>
    /// Registers a middleware action that executes when containers are removed from this item or its descendants.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterRemoveContainerAction(RemoveContainerDelegate handler);

    /// <summary>
    /// Registers a middleware action that executes when this item or its descendants are updated.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterUpdateAction(UpdateNodeDelegate handler);

    /// <summary>
    /// Registers a middleware action that executes when this item or its descendants are moved.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterMoveAction(MoveNodeDelegate handler);

    /// <summary>
    /// Registers a middleware action that executes during bulk container addition operations on this item.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterBulkAddContainerAction(BulkAddContainerDelegate handler);

    /// <summary>
    /// Registers a middleware action that executes during bulk removal operations on this item.
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
    /// Unregisters a previously registered bulk remove action.
    /// </summary>
    /// <param name="handler">The action handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterBulkRemoveAction(BulkRemoveContainerDelegate handler);

    // Event registration methods (notifications)

    /// <summary>
    /// Registers an event handler that executes after containers are added to this item or its descendants.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterContainerAddedEvent(ContainerAddedEventDelegate handler);

    /// <summary>
    /// Registers an event handler that executes after containers are removed from this item or its descendants.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterContainerRemovedEvent(ContainerRemovedEventDelegate handler);

    /// <summary>
    /// Registers an event handler that executes after this item or its descendants are updated.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterNodeUpdatedEvent(NodeUpdatedEventDelegate handler);

    /// <summary>
    /// Registers an event handler that executes after this item or its descendants are moved.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterNodeMovedEvent(NodeMovedEventDelegate handler);

    /// <summary>
    /// Registers an event handler that executes after bulk container additions to this item.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterBulkContainersAddedEvent(BulkContainersAddedEventDelegate handler);

    /// <summary>
    /// Registers an event handler that executes after bulk removals from this item.
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
    /// Unregisters a previously registered bulk nodes removed event handler.
    /// </summary>
    /// <param name="handler">The event handler to unregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool UnregisterBulkNodesRemovedEvent(BulkNodesRemovedEventDelegate handler);
}