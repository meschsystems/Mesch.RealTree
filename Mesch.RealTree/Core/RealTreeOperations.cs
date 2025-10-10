using Microsoft.Extensions.Logging;

namespace Mesch.RealTree;

/// <summary>
/// Implementation of tree operations with type-based middleware and event support.
/// </summary>
public class RealTreeOperations : IRealTreeOperations
{
    private readonly IRealTreeFactory _factory;
    private readonly ILogger<RealTreeOperations>? _logger;

    // Type-based action registries
    private readonly Dictionary<Type, List<AddContainerDelegate>> _addContainerActionsByType = new();
    private readonly Dictionary<Type, List<AddItemDelegate>> _addItemActionsByType = new();
    private readonly Dictionary<Type, List<RemoveContainerDelegate>> _removeContainerActionsByType = new();
    private readonly Dictionary<Type, List<RemoveItemDelegate>> _removeItemActionsByType = new();
    private readonly Dictionary<Type, List<UpdateNodeDelegate>> _updateActionsByType = new();
    private readonly Dictionary<Type, List<BulkAddContainerDelegate>> _bulkAddContainerActionsByType = new();
    private readonly Dictionary<Type, List<BulkAddItemDelegate>> _bulkAddItemActionsByType = new();
    private readonly Dictionary<Type, List<BulkRemoveContainerDelegate>> _bulkRemoveActionsByType = new();
    private readonly Dictionary<Type, List<ListContainerDelegate>> _listContainerActionsByType = new();
    private readonly Dictionary<Type, List<ShowItemDelegate>> _showItemActionsByType = new();

    // Type-based event registries
    private readonly Dictionary<Type, List<ContainerAddedEventDelegate>> _containerAddedEventsByType = new();
    private readonly Dictionary<Type, List<ContainerRemovedEventDelegate>> _containerRemovedEventsByType = new();
    private readonly Dictionary<Type, List<ItemAddedEventDelegate>> _itemAddedEventsByType = new();
    private readonly Dictionary<Type, List<ItemRemovedEventDelegate>> _itemRemovedEventsByType = new();
    private readonly Dictionary<Type, List<NodeUpdatedEventDelegate>> _nodeUpdatedEventsByType = new();
    private readonly Dictionary<Type, List<NodeMovedEventDelegate>> _nodeMovedEventsByType = new();
    private readonly Dictionary<Type, List<BulkContainersAddedEventDelegate>> _bulkContainersAddedEventsByType = new();
    private readonly Dictionary<Type, List<BulkItemsAddedEventDelegate>> _bulkItemsAddedEventsByType = new();
    private readonly Dictionary<Type, List<BulkNodesRemovedEventDelegate>> _bulkNodesRemovedEventsByType = new();
    private readonly Dictionary<Type, List<ContainerListedEventDelegate>> _containerListedEventsByType = new();
    private readonly Dictionary<Type, List<ItemShownEventDelegate>> _itemShownEventsByType = new();

    public RealTreeOperations(IRealTreeFactory factory, ILogger<RealTreeOperations>? logger = null)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _logger = logger;
    }

    // ========================================
    // TYPE-BASED REGISTRATION
    // ========================================

    public void RegisterAddContainerAction<T>(AddContainerDelegate handler) where T : IRealTreeNode
    {
        var type = typeof(T);
        if (!_addContainerActionsByType.ContainsKey(type))
        {
            _addContainerActionsByType[type] = new List<AddContainerDelegate>();
        }

        _addContainerActionsByType[type].Add(handler);
    }

    public void RegisterAddItemAction<T>(AddItemDelegate handler) where T : IRealTreeContainer
    {
        var type = typeof(T);
        if (!_addItemActionsByType.ContainsKey(type))
        {
            _addItemActionsByType[type] = new List<AddItemDelegate>();
        }

        _addItemActionsByType[type].Add(handler);
    }

    public void RegisterRemoveContainerAction<T>(RemoveContainerDelegate handler) where T : IRealTreeNode
    {
        var type = typeof(T);
        if (!_removeContainerActionsByType.ContainsKey(type))
        {
            _removeContainerActionsByType[type] = new List<RemoveContainerDelegate>();
        }

        _removeContainerActionsByType[type].Add(handler);
    }

    public void RegisterRemoveItemAction<T>(RemoveItemDelegate handler) where T : IRealTreeContainer
    {
        var type = typeof(T);
        if (!_removeItemActionsByType.ContainsKey(type))
        {
            _removeItemActionsByType[type] = new List<RemoveItemDelegate>();
        }

        _removeItemActionsByType[type].Add(handler);
    }

    public void RegisterUpdateAction<T>(UpdateNodeDelegate handler) where T : IRealTreeNode
    {
        var type = typeof(T);
        if (!_updateActionsByType.ContainsKey(type))
        {
            _updateActionsByType[type] = new List<UpdateNodeDelegate>();
        }

        _updateActionsByType[type].Add(handler);
    }


    public void RegisterBulkAddContainerAction<T>(BulkAddContainerDelegate handler) where T : IRealTreeNode
    {
        var type = typeof(T);
        if (!_bulkAddContainerActionsByType.ContainsKey(type))
        {
            _bulkAddContainerActionsByType[type] = new List<BulkAddContainerDelegate>();
        }

        _bulkAddContainerActionsByType[type].Add(handler);
    }

    public void RegisterBulkAddItemAction<T>(BulkAddItemDelegate handler) where T : IRealTreeContainer
    {
        var type = typeof(T);
        if (!_bulkAddItemActionsByType.ContainsKey(type))
        {
            _bulkAddItemActionsByType[type] = new List<BulkAddItemDelegate>();
        }

        _bulkAddItemActionsByType[type].Add(handler);
    }

    public void RegisterBulkRemoveAction<T>(BulkRemoveContainerDelegate handler) where T : IRealTreeNode
    {
        var type = typeof(T);
        if (!_bulkRemoveActionsByType.ContainsKey(type))
        {
            _bulkRemoveActionsByType[type] = new List<BulkRemoveContainerDelegate>();
        }

        _bulkRemoveActionsByType[type].Add(handler);
    }

    public void RegisterListAction<T>(ListContainerDelegate handler) where T : IRealTreeNode
    {
        var type = typeof(T);
        if (!_listContainerActionsByType.ContainsKey(type))
        {
            _listContainerActionsByType[type] = new List<ListContainerDelegate>();
        }

        _listContainerActionsByType[type].Add(handler);
    }

    public void RegisterShowItemAction<T>(ShowItemDelegate handler) where T : IRealTreeItem
    {
        var type = typeof(T);
        if (!_showItemActionsByType.ContainsKey(type))
        {
            _showItemActionsByType[type] = new List<ShowItemDelegate>();
        }

        _showItemActionsByType[type].Add(handler);
    }

    // Deregistration
    public bool DeregisterAddContainerAction<T>(AddContainerDelegate handler) where T : IRealTreeNode =>
        _addContainerActionsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterAddItemAction<T>(AddItemDelegate handler) where T : IRealTreeContainer =>
        _addItemActionsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterRemoveContainerAction<T>(RemoveContainerDelegate handler) where T : IRealTreeNode =>
        _removeContainerActionsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterRemoveItemAction<T>(RemoveItemDelegate handler) where T : IRealTreeContainer =>
        _removeItemActionsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterUpdateAction<T>(UpdateNodeDelegate handler) where T : IRealTreeNode =>
        _updateActionsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);


    public bool DeregisterBulkAddContainerAction<T>(BulkAddContainerDelegate handler) where T : IRealTreeNode =>
        _bulkAddContainerActionsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterBulkAddItemAction<T>(BulkAddItemDelegate handler) where T : IRealTreeContainer =>
        _bulkAddItemActionsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterBulkRemoveAction<T>(BulkRemoveContainerDelegate handler) where T : IRealTreeNode =>
        _bulkRemoveActionsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterListAction<T>(ListContainerDelegate handler) where T : IRealTreeNode =>
        _listContainerActionsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterShowItemAction<T>(ShowItemDelegate handler) where T : IRealTreeItem =>
        _showItemActionsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    // ========================================
    // EVENT REGISTRATION
    // ========================================

    public void RegisterContainerAddedEvent<T>(ContainerAddedEventDelegate handler) where T : IRealTreeNode
    {
        var type = typeof(T);
        if (!_containerAddedEventsByType.ContainsKey(type))
        {
            _containerAddedEventsByType[type] = new List<ContainerAddedEventDelegate>();
        }
        _containerAddedEventsByType[type].Add(handler);
    }

    public void RegisterContainerRemovedEvent<T>(ContainerRemovedEventDelegate handler) where T : IRealTreeNode
    {
        var type = typeof(T);
        if (!_containerRemovedEventsByType.ContainsKey(type))
        {
            _containerRemovedEventsByType[type] = new List<ContainerRemovedEventDelegate>();
        }
        _containerRemovedEventsByType[type].Add(handler);
    }

    public void RegisterItemAddedEvent<T>(ItemAddedEventDelegate handler) where T : IRealTreeContainer
    {
        var type = typeof(T);
        if (!_itemAddedEventsByType.ContainsKey(type))
        {
            _itemAddedEventsByType[type] = new List<ItemAddedEventDelegate>();
        }
        _itemAddedEventsByType[type].Add(handler);
    }

    public void RegisterItemRemovedEvent<T>(ItemRemovedEventDelegate handler) where T : IRealTreeContainer
    {
        var type = typeof(T);
        if (!_itemRemovedEventsByType.ContainsKey(type))
        {
            _itemRemovedEventsByType[type] = new List<ItemRemovedEventDelegate>();
        }
        _itemRemovedEventsByType[type].Add(handler);
    }

    public void RegisterNodeUpdatedEvent<T>(NodeUpdatedEventDelegate handler) where T : IRealTreeNode
    {
        var type = typeof(T);
        if (!_nodeUpdatedEventsByType.ContainsKey(type))
        {
            _nodeUpdatedEventsByType[type] = new List<NodeUpdatedEventDelegate>();
        }
        _nodeUpdatedEventsByType[type].Add(handler);
    }

    public void RegisterNodeMovedEvent<T>(NodeMovedEventDelegate handler) where T : IRealTreeNode
    {
        var type = typeof(T);
        if (!_nodeMovedEventsByType.ContainsKey(type))
        {
            _nodeMovedEventsByType[type] = new List<NodeMovedEventDelegate>();
        }
        _nodeMovedEventsByType[type].Add(handler);
    }

    public void RegisterBulkContainersAddedEvent<T>(BulkContainersAddedEventDelegate handler) where T : IRealTreeNode
    {
        var type = typeof(T);
        if (!_bulkContainersAddedEventsByType.ContainsKey(type))
        {
            _bulkContainersAddedEventsByType[type] = new List<BulkContainersAddedEventDelegate>();
        }
        _bulkContainersAddedEventsByType[type].Add(handler);
    }

    public void RegisterBulkItemsAddedEvent<T>(BulkItemsAddedEventDelegate handler) where T : IRealTreeContainer
    {
        var type = typeof(T);
        if (!_bulkItemsAddedEventsByType.ContainsKey(type))
        {
            _bulkItemsAddedEventsByType[type] = new List<BulkItemsAddedEventDelegate>();
        }
        _bulkItemsAddedEventsByType[type].Add(handler);
    }

    public void RegisterBulkNodesRemovedEvent<T>(BulkNodesRemovedEventDelegate handler) where T : IRealTreeNode
    {
        var type = typeof(T);
        if (!_bulkNodesRemovedEventsByType.ContainsKey(type))
        {
            _bulkNodesRemovedEventsByType[type] = new List<BulkNodesRemovedEventDelegate>();
        }
        _bulkNodesRemovedEventsByType[type].Add(handler);
    }

    public void RegisterContainerListedEvent<T>(ContainerListedEventDelegate handler) where T : IRealTreeNode
    {
        var type = typeof(T);
        if (!_containerListedEventsByType.ContainsKey(type))
        {
            _containerListedEventsByType[type] = new List<ContainerListedEventDelegate>();
        }
        _containerListedEventsByType[type].Add(handler);
    }

    public void RegisterItemShownEvent<T>(ItemShownEventDelegate handler) where T : IRealTreeItem
    {
        var type = typeof(T);
        if (!_itemShownEventsByType.ContainsKey(type))
        {
            _itemShownEventsByType[type] = new List<ItemShownEventDelegate>();
        }
        _itemShownEventsByType[type].Add(handler);
    }

    public bool DeregisterContainerAddedEvent<T>(ContainerAddedEventDelegate handler) where T : IRealTreeNode =>
        _containerAddedEventsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterContainerRemovedEvent<T>(ContainerRemovedEventDelegate handler) where T : IRealTreeNode =>
        _containerRemovedEventsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterItemAddedEvent<T>(ItemAddedEventDelegate handler) where T : IRealTreeContainer =>
        _itemAddedEventsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterItemRemovedEvent<T>(ItemRemovedEventDelegate handler) where T : IRealTreeContainer =>
        _itemRemovedEventsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterNodeUpdatedEvent<T>(NodeUpdatedEventDelegate handler) where T : IRealTreeNode =>
        _nodeUpdatedEventsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterNodeMovedEvent<T>(NodeMovedEventDelegate handler) where T : IRealTreeNode =>
        _nodeMovedEventsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterBulkContainersAddedEvent<T>(BulkContainersAddedEventDelegate handler) where T : IRealTreeNode =>
        _bulkContainersAddedEventsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterBulkItemsAddedEvent<T>(BulkItemsAddedEventDelegate handler) where T : IRealTreeContainer =>
        _bulkItemsAddedEventsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterBulkNodesRemovedEvent<T>(BulkNodesRemovedEventDelegate handler) where T : IRealTreeNode =>
        _bulkNodesRemovedEventsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterContainerListedEvent<T>(ContainerListedEventDelegate handler) where T : IRealTreeNode =>
        _containerListedEventsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    public bool DeregisterItemShownEvent<T>(ItemShownEventDelegate handler) where T : IRealTreeItem =>
        _itemShownEventsByType.TryGetValue(typeof(T), out var list) && list.Remove(handler);

    // ========================================
    // ADD CONTAINER OPERATIONS
    // ========================================

    public async Task<T> AddContainerAsync<T>(
        IRealTreeNode parent,
        Guid? id,
        string name,
        IDictionary<string, object>? metadata = null,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
        where T : IRealTreeContainer, new()
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Create instance using Activator to call constructor with parameters
        var container = (T)Activator.CreateInstance(typeof(T), id, name)!;

        // Set metadata BEFORE middleware
        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                container.Metadata[kvp.Key] = kvp.Value;
            }
        }

        // Execute type-specific middleware for parent's type
        if (triggerActions)
        {
            await ExecuteAddContainerActions(parent, container, cancellationToken);
        }

        // Add to tree - cast parent to appropriate type
        if (parent is IRealTreeContainer containerParent)
        {
            containerParent.AddContainer(container);
        }
        else if (parent is IRealTreeItem itemParent)
        {
            itemParent.AddContainer(container);
        }
        else
        {
            throw new InvalidOperationException($"Parent of type {parent.GetType().Name} cannot contain containers");
        }

        // Fire events after successful addition
        if (triggerEvents)
        {
            var context = new AddContainerContext(container, parent, parent.Tree, cancellationToken);
            await FireContainerAddedEvents(parent.GetType(), context);
        }

        return container;
    }

    public async Task<IRealTreeContainer> AddContainerAsync(
        IRealTreeNode parent,
        Guid? id,
        string name,
        IDictionary<string, object>? metadata = null,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Create instance using factory
        var container = _factory.CreateContainer(id, name);

        // Set metadata BEFORE middleware
        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                container.Metadata[kvp.Key] = kvp.Value;
            }
        }

        // Execute type-specific middleware for parent's type
        if (triggerActions)
        {
            await ExecuteAddContainerActions(parent, container, cancellationToken);
        }

        // Add to tree - cast parent to appropriate type
        if (parent is IRealTreeContainer containerParent)
        {
            containerParent.AddContainer(container);
        }
        else if (parent is IRealTreeItem itemParent)
        {
            itemParent.AddContainer(container);
        }
        else
        {
            throw new InvalidOperationException($"Parent of type {parent.GetType().Name} cannot contain containers");
        }

        // Fire events after successful addition
        if (triggerEvents)
        {
            var context = new AddContainerContext(container, parent, parent.Tree, cancellationToken);
            await FireContainerAddedEvents(parent.GetType(), context);
        }

        return container;
    }

    // ========================================
    // ADD ITEM OPERATIONS
    // ========================================

    public async Task<T> AddItemAsync<T>(
        IRealTreeContainer parent,
        Guid? id,
        string name,
        IDictionary<string, object>? metadata = null,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
        where T : IRealTreeItem, new()
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Create instance using Activator to call constructor with parameters
        var item = (T)Activator.CreateInstance(typeof(T), id, name)!;

        // Set metadata BEFORE middleware
        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                item.Metadata[kvp.Key] = kvp.Value;
            }
        }

        // Execute type-specific middleware for parent's type
        if (triggerActions)
        {
            await ExecuteAddItemActions(parent, item, cancellationToken);
        }

        // Add to tree
        parent.AddItem(item);

        // Fire events after successful addition
        if (triggerEvents)
        {
            var context = new AddItemContext(item, parent, parent.Tree, cancellationToken);
            await FireItemAddedEvents(parent.GetType(), context);
        }

        return item;
    }

    public async Task<IRealTreeItem> AddItemAsync(
        IRealTreeContainer parent,
        Guid? id,
        string name,
        IDictionary<string, object>? metadata = null,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Create instance using factory
        var item = _factory.CreateItem(id, name);

        // Set metadata BEFORE middleware
        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                item.Metadata[kvp.Key] = kvp.Value;
            }
        }

        // Execute type-specific middleware for parent's type
        if (triggerActions)
        {
            await ExecuteAddItemActions(parent, item, cancellationToken);
        }

        // Add to tree
        parent.AddItem(item);

        // Fire events after successful addition
        if (triggerEvents)
        {
            var eventContext = new AddItemContext(item, parent, parent.Tree, cancellationToken);
            await FireItemAddedEvents(parent.GetType(), eventContext);
        }

        return item;
    }

    // ========================================
    // REMOVE OPERATIONS
    // ========================================

    public async Task RemoveAsync(
        IRealTreeNode node,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (node.Parent == null)
        {
            throw new InvalidOperationException("Cannot remove root node");
        }

        // Execute middleware
        if (triggerActions)
        {
            if (node is IRealTreeContainer container)
            {
                await ExecuteRemoveContainerActions(node.Parent, container, cancellationToken);
            }
            else if (node is IRealTreeItem item && node.Parent is IRealTreeContainer parentContainer)
            {
                await ExecuteRemoveItemActions(parentContainer, item, cancellationToken);
            }
        }

        // Remove from tree
        var parent = node.Parent;
        if (node is IRealTreeContainer c)
        {
            if (node.Parent is IRealTreeContainer containerParent)
            {
                containerParent.RemoveContainer(c);
            }
            else if (node.Parent is IRealTreeItem itemParent)
            {
                itemParent.RemoveContainer(c);
            }
        }
        else if (node is IRealTreeItem i && node.Parent is IRealTreeContainer pc)
        {
            pc.RemoveItem(i);
        }

        // Fire events after successful removal
        if (triggerEvents)
        {
            if (node is IRealTreeContainer container && parent is IRealTreeContainer containerParent)
            {
                var context = new RemoveContainerContext(container, containerParent, parent.Tree, cancellationToken);
                await FireContainerRemovedEvents(parent.GetType(), context);
            }
            else if (node is IRealTreeItem item && parent is IRealTreeContainer itemContainerParent)
            {
                var context = new RemoveItemContext(item, itemContainerParent, parent.Tree, cancellationToken);
                await FireItemRemovedEvents(parent.GetType(), context);
            }
        }
    }

    public async Task RemoveAllContainersAsync(
        IRealTreeNode parent,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<IRealTreeContainer> containers;
        if (parent is IRealTreeContainer containerParent)
        {
            containers = containerParent.Containers;
        }
        else if (parent is IRealTreeItem itemParent)
        {
            containers = itemParent.Containers;
        }
        else
        {
            throw new InvalidOperationException($"Parent type {parent.GetType().Name} does not support containers");
        }

        foreach (var container in containers.ToList())
        {
            await RemoveAsync(container, triggerActions, triggerEvents, cancellationToken);
        }
    }

    public async Task RemoveAllItemsAsync(
        IRealTreeContainer parent,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var items = parent.Items.ToList();
        foreach (var item in items)
        {
            await RemoveAsync(item, triggerActions, triggerEvents, cancellationToken);
        }
    }

    // ========================================
    // UPDATE OPERATIONS
    // ========================================

    public async Task UpdateAsync(
        IRealTreeNode node,
        string? newName = null,
        Dictionary<string, object>? newMetadata = null,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Capture old values before update
        var oldName = node.Name;
        var oldMetadata = new Dictionary<string, object>(node.Metadata);

        // Execute middleware
        if (triggerActions)
        {
            await ExecuteUpdateActions(node, newName, newMetadata, cancellationToken);
        }

        // Apply changes
        if (newName != null)
        {
            node.Name = newName;
        }

        if (newMetadata != null)
        {
            node.Metadata.Clear();
            foreach (var kvp in newMetadata)
            {
                node.Metadata[kvp.Key] = kvp.Value;
            }
        }

        // Fire events after successful update
        if (triggerEvents)
        {
            var context = new UpdateContext(node, oldName, newName ?? node.Name, oldMetadata, newMetadata, node.Tree, cancellationToken);
            await FireNodeUpdatedEvents(node.GetType(), context);
        }
    }

    // ========================================
    // MOVE OPERATIONS
    // ========================================

    public async Task MoveAsync(
        IRealTreeNode node,
        IRealTreeNode newParent,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (node.Parent == null)
        {
            throw new InvalidOperationException("Cannot move root node");
        }

        // Validate cyclic references
        if (node == newParent)
        {
            throw new CyclicReferenceException("Cannot move node to itself");
        }

        if (IsAncestorOf(node, newParent))
        {
            throw new CyclicReferenceException("Cannot move node to its own descendant - this would create a cycle");
        }

        var oldParent = node.Parent;

        // Move is now implemented as a transactional remove + add
        using var transaction = new RealTreeTransaction();

        try
        {
            if (node is IRealTreeContainer container)
            {
                // Execute remove middleware
                if (triggerActions)
                {
                    await ExecuteRemoveContainerActions(oldParent, container, cancellationToken, transaction);
                }

                // Record structural remove operation
                transaction.RecordStructuralOperation(tx =>
                {
                    if (oldParent is IRealTreeContainer oldParentContainer)
                    {
                        oldParentContainer.RemoveContainer(container);
                    }
                    else if (oldParent is IRealTreeItem oldParentItem)
                    {
                        oldParentItem.RemoveContainer(container);
                    }
                });

                // Execute add middleware
                if (triggerActions)
                {
                    await ExecuteAddContainerActions(newParent, container, cancellationToken, transaction);
                }

                // Record structural add operation
                transaction.RecordStructuralOperation(tx =>
                {
                    if (newParent is IRealTreeContainer newParentContainer)
                    {
                        newParentContainer.AddContainer(container);
                    }
                    else if (newParent is IRealTreeItem newParentItem)
                    {
                        newParentItem.AddContainer(container);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Cannot move container to parent of type {newParent.GetType().Name}");
                    }
                });
            }
            else if (node is IRealTreeItem item)
            {
                if (!(oldParent is IRealTreeContainer oldParentContainer) || !(newParent is IRealTreeContainer newParentContainer))
                {
                    throw new InvalidOperationException("Cannot move item to/from non-container parent");
                }

                // Execute remove middleware
                if (triggerActions)
                {
                    await ExecuteRemoveItemActions(oldParentContainer, item, cancellationToken);
                }

                // Record structural remove operation
                transaction.RecordStructuralOperation(tx =>
                {
                    oldParentContainer.RemoveItem(item);
                });

                // Execute add middleware
                if (triggerActions)
                {
                    await ExecuteAddItemActions(newParentContainer, item, cancellationToken);
                }

                // Record structural add operation
                transaction.RecordStructuralOperation(tx =>
                {
                    newParentContainer.AddItem(item);
                });
            }

            // Commit the transaction - this executes all structural operations
            await transaction.CommitAsync();

            // Fire events after successful move
            if (triggerEvents)
            {
                var context = new MoveContext(node, oldParent, newParent, node.Tree, cancellationToken);
                await FireNodeMovedEvents(node.GetType(), context);
            }
        }
        catch
        {
            // Transaction will auto-rollback on exception via Dispose
            throw;
        }
    }

    // ========================================
    // BULK OPERATIONS
    // ========================================

    public async Task BulkAddContainersAsync<T>(
        IRealTreeNode parent,
        IEnumerable<(Guid? id, string name, IDictionary<string, object>? metadata)> items,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
        where T : IRealTreeContainer, new()
    {
        cancellationToken.ThrowIfCancellationRequested();

        var itemsList = items.ToList();

        // Skip if empty list
        if (itemsList.Count == 0)
        {
            return;
        }

        // Execute bulk middleware if registered
        if (triggerActions)
        {
            await ExecuteBulkAddContainerActions(parent, itemsList, cancellationToken);
        }

        // Track added containers for event firing
        var addedContainers = new List<IRealTreeContainer>();

        // Add each container
        foreach (var (id, name, metadata) in itemsList)
        {
            var container = await AddContainerAsync<T>(parent, id, name, metadata, triggerActions: false, triggerEvents: false, cancellationToken);
            addedContainers.Add(container);
        }

        // Fire events after successful bulk addition
        if (triggerEvents)
        {
            var context = new BulkAddContainerContext(addedContainers.AsReadOnly(), parent, parent.Tree, cancellationToken);
            await FireBulkContainersAddedEvents(parent.GetType(), context);
        }
    }

    public async Task BulkAddItemsAsync<T>(
        IRealTreeContainer parent,
        IEnumerable<(Guid? id, string name, IDictionary<string, object>? metadata)> items,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
        where T : IRealTreeItem, new()
    {
        cancellationToken.ThrowIfCancellationRequested();

        var itemsList = items.ToList();

        // Skip if empty list
        if (itemsList.Count == 0)
        {
            return;
        }

        // Execute bulk middleware if registered
        if (triggerActions)
        {
            await ExecuteBulkAddItemActions(parent, itemsList, cancellationToken);
        }

        // Track added items for event firing
        var addedItems = new List<IRealTreeItem>();

        // Add each item
        foreach (var (id, name, metadata) in itemsList)
        {
            var item = await AddItemAsync<T>(parent, id, name, metadata, triggerActions: false, triggerEvents: false, cancellationToken);
            addedItems.Add(item);
        }

        // Fire events after successful bulk addition
        if (triggerEvents)
        {
            var context = new BulkAddItemContext(addedItems.AsReadOnly(), parent, parent.Tree, cancellationToken);
            await FireBulkItemsAddedEvents(parent.GetType(), context);
        }
    }

    public async Task BulkRemoveAsync(
        IEnumerable<IRealTreeNode> nodes,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var nodesList = nodes.ToList();
        var nodesToRemove = nodesList.ToList(); // Create a copy for event context

        // Execute bulk middleware if registered
        if (triggerActions && nodesList.Count > 0)
        {
            var parent = nodesList[0].Parent;
            if (parent != null)
            {
                await ExecuteBulkRemoveActions(parent, nodesList, cancellationToken);
            }
        }

        // Get parent before removal (use first node's parent)
        var parentNode = nodesList.Count > 0 ? nodesList[0].Parent : null;

        // Remove each node
        foreach (var node in nodesList)
        {
            await RemoveAsync(node, triggerActions: false, triggerEvents: false, cancellationToken);
        }

        // Fire events after successful bulk removal
        if (triggerEvents && nodesToRemove.Count > 0 && parentNode != null)
        {
            var context = new BulkRemoveContext(nodesToRemove.AsReadOnly(), parentNode, parentNode.Tree, cancellationToken);
            await FireBulkNodesRemovedEvents(parentNode.GetType(), context);
        }
    }

    // ========================================
    // COPY OPERATIONS
    // ========================================

    public async Task<T> CopyContainerAsync<T>(
        IRealTreeContainer source,
        IRealTreeNode targetParent,
        Guid? newId = null,
        string? newName = null,
        bool deep = false,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
        where T : IRealTreeContainer, new()
    {
        cancellationToken.ThrowIfCancellationRequested();

        var metadata = new Dictionary<string, object>(source.Metadata);
        var copy = await AddContainerAsync<T>(
            targetParent,
            newId ?? Guid.NewGuid(),
            newName ?? source.Name,
            metadata,
            triggerActions,
            triggerEvents,
            cancellationToken);

        if (deep)
        {
            foreach (var childContainer in source.Containers)
            {
                await CopyContainerAsync(childContainer, copy, null, null, deep, cancellationToken);
            }

            foreach (var childItem in source.Items)
            {
                await CopyItemAsync(childItem, copy, null, null, deep, cancellationToken);
            }
        }

        return copy;
    }

    public async Task<T> CopyItemAsync<T>(
        IRealTreeItem source,
        IRealTreeContainer targetParent,
        Guid? newId = null,
        string? newName = null,
        bool deep = false,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
        where T : IRealTreeItem, new()
    {
        cancellationToken.ThrowIfCancellationRequested();

        var metadata = new Dictionary<string, object>(source.Metadata);
        var copy = await AddItemAsync<T>(
            targetParent,
            newId ?? Guid.NewGuid(),
            newName ?? source.Name,
            metadata,
            triggerActions,
            triggerEvents,
            cancellationToken);

        if (deep)
        {
            foreach (var childContainer in source.Containers)
            {
                await CopyContainerAsync(childContainer, copy, null, null, deep, cancellationToken);
            }
        }

        return copy;
    }

    public async Task<IRealTreeContainer> CopyContainerAsync(
        IRealTreeContainer source,
        IRealTreeNode destination,
        Guid? newId = null,
        string? newName = null,
        bool deep = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var metadata = new Dictionary<string, object>(source.Metadata);
        var copy = await AddContainerAsync(destination, newId ?? Guid.NewGuid(), newName ?? source.Name, metadata, true, true, cancellationToken);

        // Deep copy children if requested
        if (deep)
        {
            foreach (var childContainer in source.Containers)
            {
                await CopyContainerAsync(childContainer, copy, null, null, deep, cancellationToken);
            }

            foreach (var childItem in source.Items)
            {
                await CopyItemAsync(childItem, copy, null, null, deep, cancellationToken);
            }
        }

        return copy;
    }

    public async Task<IRealTreeItem> CopyItemAsync(
        IRealTreeItem source,
        IRealTreeContainer destination,
        Guid? newId = null,
        string? newName = null,
        bool deep = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var metadata = new Dictionary<string, object>(source.Metadata);
        var copy = await AddItemAsync(destination, newId ?? Guid.NewGuid(), newName ?? source.Name, metadata, true, true, cancellationToken);

        // Deep copy child containers if requested
        if (deep)
        {
            foreach (var childContainer in source.Containers)
            {
                await CopyContainerAsync(childContainer, copy, null, null, deep, cancellationToken);
            }
        }

        return copy;
    }

    // ========================================
    // QUERY OPERATIONS
    // ========================================

    public async Task<IReadOnlyList<IRealTreeNode>> ListAsync(
        IRealTreeNode node,
        bool includeContainers = true,
        bool includeItems = true,
        bool recursive = false,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (triggerActions)
        {
            await ExecuteListActions(node, cancellationToken);
        }

        var results = new List<IRealTreeNode>();

        // Get containers from the node (both containers and items can have containers)
        IReadOnlyList<IRealTreeContainer> containers = node switch
        {
            IRealTreeContainer cont => cont.Containers,
            IRealTreeItem item => item.Containers,
            _ => Array.Empty<IRealTreeContainer>()
        };

        // Get items from the node (only containers have items)
        IReadOnlyList<IRealTreeItem> items = node is IRealTreeContainer nodeAsContainer
            ? nodeAsContainer.Items
            : Array.Empty<IRealTreeItem>();

        if (includeContainers)
        {
            results.AddRange(containers);
        }

        if (includeItems)
        {
            results.AddRange(items);
        }

        // Recursive traversal (happens regardless of includeContainers/includeItems)
        if (recursive)
        {
            // Recurse into child containers
            foreach (var child in containers)
            {
                var childResults = await ListAsync(child, includeContainers, includeItems, recursive, triggerActions, triggerEvents, cancellationToken);
                results.AddRange(childResults);
            }

            // Recurse into containers within items
            foreach (var item in items)
            {
                foreach (var childContainer in item.Containers)
                {
                    if (includeContainers)
                    {
                        results.Add(childContainer);
                    }

                    var childResults = await ListAsync(childContainer, includeContainers, includeItems, recursive, triggerActions, triggerEvents, cancellationToken);
                    results.AddRange(childResults);
                }
            }
        }

        // Fire events after successful listing
        if (triggerEvents)
        {
            var context = new ListContainerContext(node, node.Tree, cancellationToken);
            await FireContainerListedEvents(node.GetType(), context, results.AsReadOnly(), cancellationToken);
        }

        return results.AsReadOnly();
    }

    public async Task ShowItemAsync(
        IRealTreeItem item,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (triggerActions)
        {
            await ExecuteShowItemActions(item, cancellationToken);
        }

        // Fire events after successful show
        if (triggerEvents)
        {
            var context = new ShowItemContext(item, item.Tree, cancellationToken);
            await FireItemShownEvents(item.GetType(), context, cancellationToken);
        }
    }

    // ========================================
    // VALIDATION HELPERS
    // ========================================

    private bool IsAncestorOf(IRealTreeNode node, IRealTreeNode potentialDescendant)
    {
        var current = node.Parent;
        while (current != null)
        {
            if (current == potentialDescendant)
            {
                return true;
            }

            current = current.Parent;
        }
        return false;
    }

    // ========================================
    // MIDDLEWARE EXECUTION HELPERS
    // ========================================

    private async Task ExecuteAddContainerActions(IRealTreeNode parent, IRealTreeContainer container, CancellationToken cancellationToken)
    {
        await ExecuteAddContainerActions(parent, container, cancellationToken, null);
    }

    private async Task ExecuteAddContainerActions(IRealTreeNode parent, IRealTreeContainer container, CancellationToken cancellationToken, IRealTreeTransaction? transaction)
    {
        var parentType = parent.GetType();
        if (!_addContainerActionsByType.TryGetValue(parentType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var context = new AddContainerContext(container, parent, parent.Tree, cancellationToken, transaction);

        var index = 0;
        async Task Next()
        {
            if (index < handlers.Count)
            {
                var handler = handlers[index++];
                await handler(context, Next);
            }
        }

        await Next();
    }

    private async Task ExecuteAddItemActions(IRealTreeContainer parent, IRealTreeItem item, CancellationToken cancellationToken)
    {
        var parentType = parent.GetType();
        if (!_addItemActionsByType.TryGetValue(parentType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var context = new AddItemContext(item, parent, parent.Tree, cancellationToken);

        var index = 0;
        async Task Next()
        {
            if (index < handlers.Count)
            {
                var handler = handlers[index++];
                await handler(context, Next);
            }
        }

        await Next();
    }

    private async Task ExecuteRemoveContainerActions(IRealTreeNode parent, IRealTreeContainer container, CancellationToken cancellationToken)
    {
        await ExecuteRemoveContainerActions(parent, container, cancellationToken, null);
    }

    private async Task ExecuteRemoveContainerActions(IRealTreeNode parent, IRealTreeContainer container, CancellationToken cancellationToken, IRealTreeTransaction? transaction)
    {
        var parentType = parent.GetType();
        if (!_removeContainerActionsByType.TryGetValue(parentType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        // Note: RemoveContainerContext constructor expects IRealTreeContainer for parent, but we're passing IRealTreeNode
        // This is a type mismatch that needs to be resolved. For now, casting to satisfy the constructor.
        var context = new RemoveContainerContext(container, (IRealTreeContainer)parent, parent.Tree, cancellationToken, transaction);

        var index = 0;
        async Task Next()
        {
            if (index < handlers.Count)
            {
                var handler = handlers[index++];
                await handler(context, Next);
            }
        }

        await Next();
    }

    private async Task ExecuteRemoveItemActions(IRealTreeContainer parent, IRealTreeItem item, CancellationToken cancellationToken)
    {
        var parentType = parent.GetType();
        if (!_removeItemActionsByType.TryGetValue(parentType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var context = new RemoveItemContext(item, parent, parent.Tree, cancellationToken);

        var index = 0;
        async Task Next()
        {
            if (index < handlers.Count)
            {
                var handler = handlers[index++];
                await handler(context, Next);
            }
        }

        await Next();
    }

    private async Task ExecuteUpdateActions(IRealTreeNode node, string? newName, IDictionary<string, object>? metadata, CancellationToken cancellationToken)
    {
        var nodeType = node.GetType();
        if (!_updateActionsByType.TryGetValue(nodeType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        // Capture old values before update
        var oldName = node.Name;
        var oldMetadata = metadata != null ? new Dictionary<string, object>(node.Metadata) : null;
        var newMetadata = metadata != null ? new Dictionary<string, object>(metadata) : null;

        var context = new UpdateContext(node, oldName, newName, oldMetadata, newMetadata, node.Tree, cancellationToken);

        var index = 0;
        async Task Next()
        {
            if (index < handlers.Count)
            {
                var handler = handlers[index++];
                await handler(context, Next);
            }
        }

        await Next();
    }


    private async Task ExecuteBulkAddContainerActions(IRealTreeNode parent, List<(Guid? id, string name, IDictionary<string, object>? metadata)> items, CancellationToken cancellationToken)
    {
        var parentType = parent.GetType();
        if (!_bulkAddContainerActionsByType.TryGetValue(parentType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        // Note: BulkAddContainerContext expects IReadOnlyList<IRealTreeContainer>, but we're called before items are created
        // Passing empty list for now - this is a design issue that needs to be resolved
        var context = new BulkAddContainerContext(Array.Empty<IRealTreeContainer>(), parent, parent.Tree, cancellationToken);

        var index = 0;
        async Task Next()
        {
            if (index < handlers.Count)
            {
                var handler = handlers[index++];
                await handler(context, Next);
            }
        }

        await Next();
    }

    private async Task ExecuteBulkAddItemActions(IRealTreeContainer parent, List<(Guid? id, string name, IDictionary<string, object>? metadata)> items, CancellationToken cancellationToken)
    {
        var parentType = parent.GetType();
        if (!_bulkAddItemActionsByType.TryGetValue(parentType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        // Note: BulkAddItemContext expects IReadOnlyList<IRealTreeItem>, but we're called before items are created
        // Passing empty list for now - this is a design issue that needs to be resolved
        var context = new BulkAddItemContext(Array.Empty<IRealTreeItem>(), parent, parent.Tree, cancellationToken);

        var index = 0;
        async Task Next()
        {
            if (index < handlers.Count)
            {
                var handler = handlers[index++];
                await handler(context, Next);
            }
        }

        await Next();
    }

    private async Task ExecuteBulkRemoveActions(IRealTreeNode parent, List<IRealTreeNode> nodes, CancellationToken cancellationToken)
    {
        var parentType = parent.GetType();
        if (!_bulkRemoveActionsByType.TryGetValue(parentType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var context = new BulkRemoveContext(nodes, parent, parent.Tree, cancellationToken);

        var index = 0;
        async Task Next()
        {
            if (index < handlers.Count)
            {
                var handler = handlers[index++];
                await handler(context, Next);
            }
        }

        await Next();
    }

    private async Task ExecuteListActions(IRealTreeNode node, CancellationToken cancellationToken)
    {
        var nodeType = node.GetType();
        if (!_listContainerActionsByType.TryGetValue(nodeType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var context = new ListContainerContext(node, node.Tree, cancellationToken);

        var index = 0;
        async Task Next()
        {
            if (index < handlers.Count)
            {
                var handler = handlers[index++];
                await handler(context, Next);
            }
        }

        await Next();
    }

    private async Task ExecuteShowItemActions(IRealTreeItem item, CancellationToken cancellationToken)
    {
        var itemType = item.GetType();
        if (!_showItemActionsByType.TryGetValue(itemType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var context = new ShowItemContext(item, item.Tree, cancellationToken);

        var index = 0;
        async Task Next()
        {
            if (index < handlers.Count)
            {
                var handler = handlers[index++];
                await handler(context, Next);
            }
        }

        await Next();
    }

    // ========================================
    // EVENT FIRING HELPER METHODS
    // ========================================

    private async Task FireContainerAddedEvents(Type parentType, AddContainerContext context)
    {
        if (!_containerAddedEventsByType.TryGetValue(parentType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var tasks = handlers.Select(handler =>
        {
            try
            {
                return handler(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in ContainerAdded event handler for type {Type}", parentType.Name);
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task FireContainerRemovedEvents(Type parentType, RemoveContainerContext context)
    {
        if (!_containerRemovedEventsByType.TryGetValue(parentType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var tasks = handlers.Select(handler =>
        {
            try
            {
                return handler(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in ContainerRemoved event handler for type {Type}", parentType.Name);
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task FireItemAddedEvents(Type parentType, AddItemContext context)
    {
        if (!_itemAddedEventsByType.TryGetValue(parentType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var tasks = handlers.Select(handler =>
        {
            try
            {
                return handler(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in ItemAdded event handler for type {Type}", parentType.Name);
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task FireItemRemovedEvents(Type parentType, RemoveItemContext context)
    {
        if (!_itemRemovedEventsByType.TryGetValue(parentType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var tasks = handlers.Select(handler =>
        {
            try
            {
                return handler(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in ItemRemoved event handler for type {Type}", parentType.Name);
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task FireNodeUpdatedEvents(Type nodeType, UpdateContext context)
    {
        if (!_nodeUpdatedEventsByType.TryGetValue(nodeType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var tasks = handlers.Select(handler =>
        {
            try
            {
                return handler(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in NodeUpdated event handler for type {Type}", nodeType.Name);
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task FireNodeMovedEvents(Type nodeType, MoveContext context)
    {
        if (!_nodeMovedEventsByType.TryGetValue(nodeType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var tasks = handlers.Select(handler =>
        {
            try
            {
                return handler(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in NodeMoved event handler for type {Type}", nodeType.Name);
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task FireBulkContainersAddedEvents(Type parentType, BulkAddContainerContext context)
    {
        if (!_bulkContainersAddedEventsByType.TryGetValue(parentType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var tasks = handlers.Select(handler =>
        {
            try
            {
                return handler(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in BulkContainersAdded event handler for type {Type}", parentType.Name);
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task FireBulkItemsAddedEvents(Type parentType, BulkAddItemContext context)
    {
        if (!_bulkItemsAddedEventsByType.TryGetValue(parentType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var tasks = handlers.Select(handler =>
        {
            try
            {
                return handler(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in BulkItemsAdded event handler for type {Type}", parentType.Name);
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task FireBulkNodesRemovedEvents(Type parentType, BulkRemoveContext context)
    {
        if (!_bulkNodesRemovedEventsByType.TryGetValue(parentType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var tasks = handlers.Select(handler =>
        {
            try
            {
                return handler(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in BulkNodesRemoved event handler for type {Type}", parentType.Name);
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task FireContainerListedEvents(Type nodeType, ListContainerContext context, IReadOnlyList<IRealTreeNode> result, CancellationToken cancellationToken)
    {
        if (!_containerListedEventsByType.TryGetValue(nodeType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var tasks = handlers.Select(handler =>
        {
            try
            {
                return handler(context, result, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in ContainerListed event handler for type {Type}", nodeType.Name);
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task FireItemShownEvents(Type itemType, ShowItemContext context, CancellationToken cancellationToken)
    {
        if (!_itemShownEventsByType.TryGetValue(itemType, out var handlers) || handlers.Count == 0)
        {
            return;
        }

        var tasks = handlers.Select(handler =>
        {
            try
            {
                return handler(context, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in ItemShown event handler for type {Type}", itemType.Name);
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }
}
