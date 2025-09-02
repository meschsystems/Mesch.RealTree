namespace Mesch.RealTree;

/// <summary>
/// Concrete item implementation.
/// </summary>
public class RealTreeItem : RealTreeNodeBase, IRealTreeItem
{
    private readonly List<IRealTreeContainer> _containers = new List<IRealTreeContainer>();

    // Action delegates (middleware)
    private readonly List<AddContainerDelegate> _addContainerActions = new List<AddContainerDelegate>();
    private readonly List<RemoveContainerDelegate> _removeContainerActions = new List<RemoveContainerDelegate>();
    private readonly List<UpdateNodeDelegate> _updateActions = new List<UpdateNodeDelegate>();
    private readonly List<MoveNodeDelegate> _moveActions = new List<MoveNodeDelegate>();
    private readonly List<BulkAddContainerDelegate> _bulkAddContainerActions = new List<BulkAddContainerDelegate>();
    private readonly List<BulkRemoveContainerDelegate> _bulkRemoveActions = new List<BulkRemoveContainerDelegate>();

    // Event delegates (notifications)
    private readonly List<ContainerAddedEventDelegate> _containerAddedEvents = new List<ContainerAddedEventDelegate>();
    private readonly List<ContainerRemovedEventDelegate> _containerRemovedEvents = new List<ContainerRemovedEventDelegate>();
    private readonly List<NodeUpdatedEventDelegate> _nodeUpdatedEvents = new List<NodeUpdatedEventDelegate>();
    private readonly List<NodeMovedEventDelegate> _nodeMovedEvents = new List<NodeMovedEventDelegate>();
    private readonly List<BulkContainersAddedEventDelegate> _bulkContainersAddedEvents = new List<BulkContainersAddedEventDelegate>();
    private readonly List<BulkNodesRemovedEventDelegate> _bulkNodesRemovedEvents = new List<BulkNodesRemovedEventDelegate>();

    public IReadOnlyList<IRealTreeContainer> Containers => _containers.AsReadOnly();

    public override IRealTree Tree => Parent?.Tree ?? throw new InvalidOperationException("Item is not attached to a tree");

    public RealTreeItem(Guid? id = null, string? name = null) : base(id, name) { }

    // Action registration methods (middleware)
    public void RegisterAddContainerAction(AddContainerDelegate handler) => _addContainerActions.Add(handler);
    public void RegisterRemoveContainerAction(RemoveContainerDelegate handler) => _removeContainerActions.Add(handler);
    public void RegisterUpdateAction(UpdateNodeDelegate handler) => _updateActions.Add(handler);
    public void RegisterMoveAction(MoveNodeDelegate handler) => _moveActions.Add(handler);
    public void RegisterBulkAddContainerAction(BulkAddContainerDelegate handler) => _bulkAddContainerActions.Add(handler);
    public void RegisterBulkRemoveAction(BulkRemoveContainerDelegate handler) => _bulkRemoveActions.Add(handler);

    // Action unregistration methods
    public bool UnregisterAddContainerAction(AddContainerDelegate handler) => _addContainerActions.Remove(handler);
    public bool UnregisterRemoveContainerAction(RemoveContainerDelegate handler) => _removeContainerActions.Remove(handler);
    public bool UnregisterUpdateAction(UpdateNodeDelegate handler) => _updateActions.Remove(handler);
    public bool UnregisterMoveAction(MoveNodeDelegate handler) => _moveActions.Remove(handler);
    public bool UnregisterBulkAddContainerAction(BulkAddContainerDelegate handler) => _bulkAddContainerActions.Remove(handler);
    public bool UnregisterBulkRemoveAction(BulkRemoveContainerDelegate handler) => _bulkRemoveActions.Remove(handler);

    // Event registration methods (notifications)
    public void RegisterContainerAddedEvent(ContainerAddedEventDelegate handler) => _containerAddedEvents.Add(handler);
    public void RegisterContainerRemovedEvent(ContainerRemovedEventDelegate handler) => _containerRemovedEvents.Add(handler);
    public void RegisterNodeUpdatedEvent(NodeUpdatedEventDelegate handler) => _nodeUpdatedEvents.Add(handler);
    public void RegisterNodeMovedEvent(NodeMovedEventDelegate handler) => _nodeMovedEvents.Add(handler);
    public void RegisterBulkContainersAddedEvent(BulkContainersAddedEventDelegate handler) => _bulkContainersAddedEvents.Add(handler);
    public void RegisterBulkNodesRemovedEvent(BulkNodesRemovedEventDelegate handler) => _bulkNodesRemovedEvents.Add(handler);

    // Event unregistration methods
    public bool UnregisterContainerAddedEvent(ContainerAddedEventDelegate handler) => _containerAddedEvents.Remove(handler);
    public bool UnregisterContainerRemovedEvent(ContainerRemovedEventDelegate handler) => _containerRemovedEvents.Remove(handler);
    public bool UnregisterNodeUpdatedEvent(NodeUpdatedEventDelegate handler) => _nodeUpdatedEvents.Remove(handler);
    public bool UnregisterNodeMovedEvent(NodeMovedEventDelegate handler) => _nodeMovedEvents.Remove(handler);
    public bool UnregisterBulkContainersAddedEvent(BulkContainersAddedEventDelegate handler) => _bulkContainersAddedEvents.Remove(handler);
    public bool UnregisterBulkNodesRemovedEvent(BulkNodesRemovedEventDelegate handler) => _bulkNodesRemovedEvents.Remove(handler);

    // Action collections for operations service access

    /// <summary>
    /// Gets the collection of registered add container action handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<AddContainerDelegate> AddContainerActions => _addContainerActions.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered remove container action handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<RemoveContainerDelegate> RemoveContainerActions => _removeContainerActions.AsReadOnly();

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
    /// Gets the collection of registered bulk remove action handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<BulkRemoveContainerDelegate> BulkRemoveActions => _bulkRemoveActions.AsReadOnly();

    // Event collections for operations service access

    /// <summary>
    /// Gets the collection of registered container added event handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<ContainerAddedEventDelegate> ContainerAddedEvents => _containerAddedEvents.AsReadOnly();

    /// <summary>
    /// Gets the collection of registered container removed event handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<ContainerRemovedEventDelegate> ContainerRemovedEvents => _containerRemovedEvents.AsReadOnly();

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
    /// Gets the collection of registered bulk nodes removed event handlers for internal operations service access.
    /// </summary>
    public IReadOnlyList<BulkNodesRemovedEventDelegate> BulkNodesRemovedEvents => _bulkNodesRemovedEvents.AsReadOnly();


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
}