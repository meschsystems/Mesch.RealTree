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

    #region Action registration methods (middleware)

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

    /// <summary>
    /// Registers a middleware action that executes when this item's containers are being listed.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterListContainerAction(ListContainerDelegate handler);

    /// <summary>
    /// Registers a middleware action that executes when this item is being shown/displayed.
    /// </summary>
    /// <param name="handler">The action handler to register.</param>
    void RegisterShowItemAction(ShowItemDelegate handler);

    #endregion

    #region Action deregistration methods

    /// <summary>
    /// Deregisters a previously registered add container action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterAddContainerAction(AddContainerDelegate handler);

    /// <summary>
    /// Deregisters a previously registered remove container action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterRemoveContainerAction(RemoveContainerDelegate handler);

    /// <summary>
    /// Deregisters a previously registered update action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterUpdateAction(UpdateNodeDelegate handler);

    /// <summary>
    /// Deregisters a previously registered move action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterMoveAction(MoveNodeDelegate handler);

    /// <summary>
    /// Deregisters a previously registered bulk add container action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterBulkAddContainerAction(BulkAddContainerDelegate handler);

    /// <summary>
    /// Deregisters a previously registered bulk remove action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterBulkRemoveAction(BulkRemoveContainerDelegate handler);

    /// <summary>
    /// Deregisters a previously registered list container action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterListContainerAction(ListContainerDelegate handler);

    /// <summary>
    /// Deregisters a previously registered show item action.
    /// </summary>
    /// <param name="handler">The action handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterShowItemAction(ShowItemDelegate handler);

    #endregion

    #region Event registration methods (notifications)

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

    /// <summary>
    /// Registers an event handler that executes after this item's containers have been listed.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterContainerListedEvent(ContainerListedEventDelegate handler);

    /// <summary>
    /// Registers an event handler that executes after this item has been shown/displayed.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterItemShownEvent(ItemShownEventDelegate handler);

    #endregion

    #region Event deregistration methods

    /// <summary>
    /// Deregisters a previously registered container added event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterContainerAddedEvent(ContainerAddedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered container removed event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterContainerRemovedEvent(ContainerRemovedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered node updated event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterNodeUpdatedEvent(NodeUpdatedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered node moved event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterNodeMovedEvent(NodeMovedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered bulk containers added event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterBulkContainersAddedEvent(BulkContainersAddedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered bulk nodes removed event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterBulkNodesRemovedEvent(BulkNodesRemovedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered container listed event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterContainerListedEvent(ContainerListedEventDelegate handler);

    /// <summary>
    /// Deregisters a previously registered item shown event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    /// <returns>True if the handler was found and removed; otherwise, false.</returns>
    bool DeregisterItemShownEvent(ItemShownEventDelegate handler);

    #endregion
}