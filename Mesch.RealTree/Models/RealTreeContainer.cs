namespace Mesch.RealTree;

/// <summary>
/// Concrete container implementation.
/// </summary>
public class RealTreeContainer : RealTreeNodeBase, IRealTreeContainer
{
    private readonly List<IRealTreeContainer> _containers = new List<IRealTreeContainer>();
    private readonly List<IRealTreeItem> _items = new List<IRealTreeItem>();

    #region Action delegates (middleware)
    private readonly List<AddContainerDelegate> _addContainerActions = new List<AddContainerDelegate>();
    private readonly List<RemoveContainerDelegate> _removeContainerActions = new List<RemoveContainerDelegate>();
    private readonly List<AddItemDelegate> _addItemActions = new List<AddItemDelegate>();
    private readonly List<RemoveItemDelegate> _removeItemActions = new List<RemoveItemDelegate>();
    private readonly List<UpdateNodeDelegate> _updateActions = new List<UpdateNodeDelegate>();
    private readonly List<MoveNodeDelegate> _moveActions = new List<MoveNodeDelegate>();
    private readonly List<BulkAddContainerDelegate> _bulkAddContainerActions = new List<BulkAddContainerDelegate>();
    private readonly List<BulkAddItemDelegate> _bulkAddItemActions = new List<BulkAddItemDelegate>();
    private readonly List<BulkRemoveContainerDelegate> _bulkRemoveActions = new List<BulkRemoveContainerDelegate>();
    private readonly List<ListContainerDelegate> _listContainerActions = new List<ListContainerDelegate>();


    #endregion

    #region Event delegates (notifications)
    private readonly List<ContainerAddedEventDelegate> _containerAddedEvents = new List<ContainerAddedEventDelegate>();
    private readonly List<ContainerRemovedEventDelegate> _containerRemovedEvents = new List<ContainerRemovedEventDelegate>();
    private readonly List<ItemAddedEventDelegate> _itemAddedEvents = new List<ItemAddedEventDelegate>();
    private readonly List<ItemRemovedEventDelegate> _itemRemovedEvents = new List<ItemRemovedEventDelegate>();
    private readonly List<NodeUpdatedEventDelegate> _nodeUpdatedEvents = new List<NodeUpdatedEventDelegate>();
    private readonly List<NodeMovedEventDelegate> _nodeMovedEvents = new List<NodeMovedEventDelegate>();
    private readonly List<BulkContainersAddedEventDelegate> _bulkContainersAddedEvents = new List<BulkContainersAddedEventDelegate>();
    private readonly List<BulkItemsAddedEventDelegate> _bulkItemsAddedEvents = new List<BulkItemsAddedEventDelegate>();
    private readonly List<BulkNodesRemovedEventDelegate> _bulkNodesRemovedEvents = new List<BulkNodesRemovedEventDelegate>();
    private readonly List<ContainerListedEventDelegate> _containerListedEvents = new List<ContainerListedEventDelegate>();

    #endregion

    public virtual IReadOnlyList<IRealTreeContainer> Containers => _containers.AsReadOnly();
    public virtual IReadOnlyList<IRealTreeItem> Items => _items.AsReadOnly();
    public virtual IReadOnlyList<IRealTreeNode> Children => _containers.Cast<IRealTreeNode>().Concat(_items.Cast<IRealTreeNode>()).ToList().AsReadOnly();

    public override IRealTree Tree => (Parent as RealTreeContainer)?.Tree ?? (this as IRealTree) ?? throw new InvalidOperationException("Container is not attached to a tree");

    public RealTreeContainer(Guid? id = null, string? name = null) : base(id, name) { }

    // Action registration methods (middleware)
    public void RegisterAddContainerAction(AddContainerDelegate handler) => _addContainerActions.Add(handler);
    public void RegisterRemoveContainerAction(RemoveContainerDelegate handler) => _removeContainerActions.Add(handler);
    public void RegisterAddItemAction(AddItemDelegate handler) => _addItemActions.Add(handler);
    public void RegisterRemoveItemAction(RemoveItemDelegate handler) => _removeItemActions.Add(handler);
    public void RegisterUpdateAction(UpdateNodeDelegate handler) => _updateActions.Add(handler);
    public void RegisterMoveAction(MoveNodeDelegate handler) => _moveActions.Add(handler);
    public void RegisterBulkAddContainerAction(BulkAddContainerDelegate handler) => _bulkAddContainerActions.Add(handler);
    public void RegisterBulkAddItemAction(BulkAddItemDelegate handler) => _bulkAddItemActions.Add(handler);
    public void RegisterBulkRemoveAction(BulkRemoveContainerDelegate handler) => _bulkRemoveActions.Add(handler);
    public void RegisterListContainerAction(ListContainerDelegate handler) => _listContainerActions.Add(handler);

    #region Action unregistration methods
    public bool UnregisterAddContainerAction(AddContainerDelegate handler) => _addContainerActions.Remove(handler);
    public bool UnregisterRemoveContainerAction(RemoveContainerDelegate handler) => _removeContainerActions.Remove(handler);
    public bool UnregisterAddItemAction(AddItemDelegate handler) => _addItemActions.Remove(handler);
    public bool UnregisterRemoveItemAction(RemoveItemDelegate handler) => _removeItemActions.Remove(handler);
    public bool UnregisterUpdateAction(UpdateNodeDelegate handler) => _updateActions.Remove(handler);
    public bool UnregisterMoveAction(MoveNodeDelegate handler) => _moveActions.Remove(handler);
    public bool UnregisterBulkAddContainerAction(BulkAddContainerDelegate handler) => _bulkAddContainerActions.Remove(handler);
    public bool UnregisterBulkAddItemAction(BulkAddItemDelegate handler) => _bulkAddItemActions.Remove(handler);
    public bool UnregisterBulkRemoveAction(BulkRemoveContainerDelegate handler) => _bulkRemoveActions.Remove(handler);
    public bool UnregisterListContainerAction(ListContainerDelegate handler) => _listContainerActions.Remove(handler);

    #endregion

    #region Event registration methods (notifications)
    public void RegisterContainerAddedEvent(ContainerAddedEventDelegate handler) => _containerAddedEvents.Add(handler);
    public void RegisterContainerRemovedEvent(ContainerRemovedEventDelegate handler) => _containerRemovedEvents.Add(handler);
    public void RegisterItemAddedEvent(ItemAddedEventDelegate handler) => _itemAddedEvents.Add(handler);
    public void RegisterItemRemovedEvent(ItemRemovedEventDelegate handler) => _itemRemovedEvents.Add(handler);
    public void RegisterNodeUpdatedEvent(NodeUpdatedEventDelegate handler) => _nodeUpdatedEvents.Add(handler);
    public void RegisterNodeMovedEvent(NodeMovedEventDelegate handler) => _nodeMovedEvents.Add(handler);
    public void RegisterBulkContainersAddedEvent(BulkContainersAddedEventDelegate handler) => _bulkContainersAddedEvents.Add(handler);
    public void RegisterBulkItemsAddedEvent(BulkItemsAddedEventDelegate handler) => _bulkItemsAddedEvents.Add(handler);
    public void RegisterBulkNodesRemovedEvent(BulkNodesRemovedEventDelegate handler) => _bulkNodesRemovedEvents.Add(handler);
    public void RegisterContainerListedEvent(ContainerListedEventDelegate handler) => _containerListedEvents.Add(handler);

    #endregion

    #region Event unregistration methods
    public bool UnregisterContainerAddedEvent(ContainerAddedEventDelegate handler) => _containerAddedEvents.Remove(handler);
    public bool UnregisterContainerRemovedEvent(ContainerRemovedEventDelegate handler) => _containerRemovedEvents.Remove(handler);
    public bool UnregisterItemAddedEvent(ItemAddedEventDelegate handler) => _itemAddedEvents.Remove(handler);
    public bool UnregisterItemRemovedEvent(ItemRemovedEventDelegate handler) => _itemRemovedEvents.Remove(handler);
    public bool UnregisterNodeUpdatedEvent(NodeUpdatedEventDelegate handler) => _nodeUpdatedEvents.Remove(handler);
    public bool UnregisterNodeMovedEvent(NodeMovedEventDelegate handler) => _nodeMovedEvents.Remove(handler);
    public bool UnregisterBulkContainersAddedEvent(BulkContainersAddedEventDelegate handler) => _bulkContainersAddedEvents.Remove(handler);
    public bool UnregisterBulkItemsAddedEvent(BulkItemsAddedEventDelegate handler) => _bulkItemsAddedEvents.Remove(handler);
    public bool UnregisterBulkNodesRemovedEvent(BulkNodesRemovedEventDelegate handler) => _bulkNodesRemovedEvents.Remove(handler);
    public bool UnregisterContainerListedEvent(ContainerListedEventDelegate handler) => _containerListedEvents.Remove(handler);

    #endregion

    /// <summary>
    /// Gets the collection of registered add container action handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<AddContainerDelegate> AddContainerActions => _addContainerActions.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered remove container action handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<RemoveContainerDelegate> RemoveContainerActions => _removeContainerActions.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered add item action handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<AddItemDelegate> AddItemActions => _addItemActions.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered remove item action handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<RemoveItemDelegate> RemoveItemActions => _removeItemActions.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered update action handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<UpdateNodeDelegate> UpdateActions => _updateActions.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered move action handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<MoveNodeDelegate> MoveActions => _moveActions.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered bulk add container action handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<BulkAddContainerDelegate> BulkAddContainerActions => _bulkAddContainerActions.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered bulk add item action handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<BulkAddItemDelegate> BulkAddItemActions => _bulkAddItemActions.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered bulk remove action handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<BulkRemoveContainerDelegate> BulkRemoveActions => _bulkRemoveActions.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered list container action handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<ListContainerDelegate> ListContainerActions => _listContainerActions.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered container added event handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<ContainerAddedEventDelegate> ContainerAddedEvents => _containerAddedEvents.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered container removed event handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<ContainerRemovedEventDelegate> ContainerRemovedEvents => _containerRemovedEvents.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered item added event handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<ItemAddedEventDelegate> ItemAddedEvents => _itemAddedEvents.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered item removed event handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<ItemRemovedEventDelegate> ItemRemovedEvents => _itemRemovedEvents.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered node updated event handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<NodeUpdatedEventDelegate> NodeUpdatedEvents => _nodeUpdatedEvents.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered node moved event handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<NodeMovedEventDelegate> NodeMovedEvents => _nodeMovedEvents.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered bulk containers added event handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<BulkContainersAddedEventDelegate> BulkContainersAddedEvents => _bulkContainersAddedEvents.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered bulk items added event handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<BulkItemsAddedEventDelegate> BulkItemsAddedEvents => _bulkItemsAddedEvents.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered bulk nodes removed event handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<BulkNodesRemovedEventDelegate> BulkNodesRemovedEvents => _bulkNodesRemovedEvents.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered container listed event handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<ContainerListedEventDelegate> ContainerListedEvents => _containerListedEvents.AsReadOnly();

    public void AddContainer(IRealTreeContainer container)
    {
        _containers.Add(container);
        ((RealTreeNodeBase)container).SetParent(this);
    }

    public void RemoveContainer(IRealTreeContainer container)
    {
        _containers.Remove(container);
        ((RealTreeNodeBase)container).SetParent(null);
    }

    public void AddItem(IRealTreeItem item)
    {
        _items.Add(item);
        ((RealTreeNodeBase)item).SetParent(this);
    }

    public void RemoveItem(IRealTreeItem item)
    {
        _items.Remove(item);
        ((RealTreeNodeBase)item).SetParent(null);
    }
}