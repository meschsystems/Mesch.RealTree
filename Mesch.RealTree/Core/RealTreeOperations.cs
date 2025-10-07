
using Microsoft.Extensions.Logging;

namespace Mesch.RealTree;
/// <summary>
/// Implementation of tree operations with middleware actions and event support.
/// This class provides the core functionality for all tree modifications with extensible pipeline support.
/// </summary>
public class RealTreeOperations : IRealTreeOperations
{
    private readonly IRealTreeFactory _factory;
    private readonly ILogger<RealTreeOperations>? _logger;

    /// <summary>
    /// Initializes a new instance of the RealTreeOperations class.
    /// </summary>
    /// <param name="factory">The factory for creating tree nodes.</param>
    /// <param name="logger">Optional logger for recording event execution errors.</param>
    /// <exception cref="ArgumentNullException">Thrown when factory is null.</exception>
    public RealTreeOperations(IRealTreeFactory factory, ILogger<RealTreeOperations>? logger = null)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _logger = logger;
    }

    public async Task<IRealTreeContainer> AddContainerAsync(IRealTreeNode parent, Guid? id, string name, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default)
    {
        var container = _factory.CreateContainer(id, name);
        return await AddContainerAsync(parent, container, triggerActions, triggerEvents, cancellationToken);
    }

    public async Task<IRealTreeContainer> AddContainerAsync(IRealTreeNode parent, IRealTreeContainer container, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default)
    {
        ValidateCyclicReference(container, parent);

        var context = new AddContainerContext(container, parent, parent.Tree, cancellationToken);

        if (triggerActions)
        {
            await ExecuteActionPipeline(context, parent, node => GetAddContainerActions(node), () => PerformAddContainer(parent, container));
        }
        else
        {
            await PerformAddContainer(parent, container);
        }

        if (triggerEvents)
        {
            await ExecuteEvents(context, parent, node => GetContainerAddedEvents(node));
        }

        return container;
    }

    public async Task<IRealTreeItem> AddItemAsync(IRealTreeContainer parent, Guid? id, string name, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default)
    {
        var item = _factory.CreateItem(id, name);
        return await AddItemAsync(parent, item, triggerActions, triggerEvents, cancellationToken);
    }

    public async Task<IRealTreeItem> AddItemAsync(IRealTreeContainer parent, IRealTreeItem item, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default)
    {
        var context = new AddItemContext(item, parent, parent.Tree, cancellationToken);

        if (triggerActions)
        {
            await ExecuteActionPipeline(context, parent, node => GetAddItemActions(node), () => PerformAddItem(parent, item));
        }
        else
        {
            await PerformAddItem(parent, item);
        }

        if (triggerEvents)
        {
            await ExecuteEvents(context, parent, node => GetItemAddedEvents(node));
        }

        return item;
    }

    public async Task RemoveAsync(
        IRealTreeNode node,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
    {
        if (node.Parent == null)
        {
            throw new InvalidOperationException("Cannot remove root node");
        }

        if (node is IRealTreeContainer container)
        {
            var parentContainer = node.Parent as IRealTreeContainer
                ?? throw new InvalidOperationException("Container parent must be a container");
            var context = new RemoveContainerContext(container, parentContainer, node.Tree, cancellationToken);

            if (triggerActions)
            {
                await ExecuteActionPipeline(context, node, n => GetRemoveContainerActions(n), () => { PerformRemove(node); return Task.CompletedTask; });
            }
            else
            {
                PerformRemove(node);
            }

            if (triggerEvents)
            {
                await ExecuteEvents(context, context.Parent, n => GetContainerRemovedEvents(n));
            }
        }
        else if (node is IRealTreeItem item)
        {
            var parentContainer = node.Parent as IRealTreeContainer
                ?? throw new InvalidOperationException("Item parent must be a container");
            var context = new RemoveItemContext(item, parentContainer, node.Tree, cancellationToken);

            if (triggerActions)
            {
                await ExecuteActionPipeline(context, node, n => GetRemoveItemActions(n), () => { PerformRemove(node); return Task.CompletedTask; });
            }
            else
            {
                PerformRemove(node);
            }

            if (triggerEvents)
            {
                await ExecuteEvents(context, context.Parent, n => GetItemRemovedEvents(n));
            }
        }
        else
        {
            throw new InvalidOperationException("Node must be a container or item");
        }
    }

    public async Task RemoveAllContainersAsync(IRealTreeNode parent, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default)
    {
        var containers = parent is IRealTreeContainer container ? container.Containers.ToList() :
                        parent is IRealTreeItem item ? item.Containers.ToList() :
                        new List<IRealTreeContainer>();

        foreach (var containerToRemove in containers)
        {
            await RemoveAsync(containerToRemove, triggerActions, triggerEvents, cancellationToken);
        }
    }

    public async Task RemoveAllItemsAsync(IRealTreeContainer parent, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default)
    {
        var items = parent.Items.ToList();
        foreach (var item in items)
        {
            await RemoveAsync(item, triggerActions, triggerEvents, cancellationToken);
        }
    }

    public async Task UpdateAsync(IRealTreeNode node, string? newName = null, Dictionary<string, object>? newMetadata = null, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default)
    {
        var oldName = node.Name;
        var oldMetadata = new Dictionary<string, object>(node.Metadata);

        var context = new UpdateContext(node, oldName, newName, oldMetadata, newMetadata, node.Tree, cancellationToken);

        if (triggerActions)
        {
            await ExecuteActionPipeline(context, node, n => GetUpdateActions(n), () => { PerformUpdate(node, newName, newMetadata); return Task.CompletedTask; });
        }
        else
        {
            PerformUpdate(node, newName, newMetadata);
        }

        if (triggerEvents)
        {
            await ExecuteEvents(context, node, n => GetNodeUpdatedEvents(n));
        }
    }

    public async Task MoveAsync(IRealTreeNode node, IRealTreeNode newParent, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default)
    {
        ValidateCyclicReference(node, newParent);

        var oldParent = node.Parent;
        var context = new MoveContext(node, oldParent, newParent, node.Tree, cancellationToken);

        if (triggerActions)
        {
            await ExecuteActionPipeline(context, node, n => GetMoveActions(n), () => { PerformMove(node, newParent); return Task.CompletedTask; });
        }
        else
        {
            PerformMove(node, newParent);
        }

        if (triggerEvents)
        {
            await ExecuteEvents(context, node, n => GetNodeMovedEvents(n));
        }
    }

    public async Task<IRealTreeContainer> CopyContainerAsync(IRealTreeContainer source, IRealTreeNode destination, Guid? newId = null, string? newName = null, CancellationToken cancellationToken = default)
    {
        var copy = _factory.CreateContainer(newId, newName ?? source.Name);

        // Copy metadata
        foreach (var kvp in source.Metadata)
        {
            copy.Metadata[kvp.Key] = kvp.Value;
        }

        await AddContainerAsync(destination, copy, true, true, cancellationToken);

        // Recursively copy children
        foreach (var childContainer in source.Containers)
        {
            await CopyContainerAsync(childContainer, copy, cancellationToken: cancellationToken);
        }

        foreach (var childItem in source.Items)
        {
            await CopyItemAsync(childItem, copy, cancellationToken: cancellationToken);
        }

        return copy;
    }

    // Bulk operations implementation
    public async Task BulkAddContainersAsync(IRealTreeNode parent, IEnumerable<IRealTreeContainer> containers, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default)
    {
        var containersList = containers.ToList();
        if (!containersList.Any())
        {
            return;
        }

        // Validate cycle prevention for all containers
        foreach (var container in containersList)
        {
            ValidateCyclicReference(container, parent);
        }

        var context = new BulkAddContainerContext(containersList, parent, parent.Tree, cancellationToken);

        if (triggerActions)
        {
            await ExecuteActionPipeline(context, parent, node => GetBulkAddContainerActions(node), () => PerformBulkAddContainers(parent, containersList));
        }
        else
        {
            await PerformBulkAddContainers(parent, containersList);
        }

        if (triggerEvents)
        {
            await ExecuteEvents(context, parent, node => GetBulkContainersAddedEvents(node));
        }
    }

    public async Task BulkAddItemsAsync(IRealTreeContainer parent, IEnumerable<IRealTreeItem> items, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default)
    {
        var itemsList = items.ToList();
        if (!itemsList.Any())
        {
            return;
        }

        var context = new BulkAddItemContext(itemsList, parent, parent.Tree, cancellationToken);

        if (triggerActions)
        {
            await ExecuteActionPipeline(context, parent, node => GetBulkAddItemActions(node), () => PerformBulkAddItems(parent, itemsList));
        }
        else
        {
            await PerformBulkAddItems(parent, itemsList);
        }

        if (triggerEvents)
        {
            await ExecuteEvents(context, parent, node => GetBulkItemsAddedEvents(node));
        }
    }

    public async Task BulkRemoveAsync(IEnumerable<IRealTreeNode> nodes, bool triggerActions = true, bool triggerEvents = true, CancellationToken cancellationToken = default)
    {
        var nodesList = nodes.ToList();
        if (!nodesList.Any())
        {
            return;
        }

        // Group by parent for context creation
        var nodesByParent = nodesList.Where(n => n.Parent != null).GroupBy(n => n.Parent!);

        foreach (var parentGroup in nodesByParent)
        {
            var context = new BulkRemoveContext(parentGroup.ToList(), parentGroup.Key, parentGroup.First().Tree, cancellationToken);

            if (triggerActions)
            {
                await ExecuteActionPipeline(context, parentGroup.Key, node => GetBulkRemoveActions(node), () => { PerformBulkRemove(parentGroup.ToList()); return Task.CompletedTask; });
            }
            else
            {
                PerformBulkRemove(parentGroup.ToList());
            }

            if (triggerEvents)
            {
                await ExecuteEvents(context, parentGroup.Key, node => GetBulkNodesRemovedEvents(node));
            }
        }
    }

    public async Task<IRealTreeItem> CopyItemAsync(IRealTreeItem source, IRealTreeContainer destination, Guid? newId = null, string? newName = null, CancellationToken cancellationToken = default)
    {
        var copy = _factory.CreateItem(newId, newName ?? source.Name);

        // Copy metadata
        foreach (var kvp in source.Metadata)
        {
            copy.Metadata[kvp.Key] = kvp.Value;
        }

        await AddItemAsync(destination, copy, true, true, cancellationToken);

        // Recursively copy child containers
        foreach (var childContainer in source.Containers)
        {
            await CopyContainerAsync(childContainer, copy, cancellationToken: cancellationToken);
        }

        return copy;
    }

    // Cycle detection and prevention
    private void ValidateCyclicReference(IRealTreeNode nodeToAdd, IRealTreeNode targetParent)
    {
        // If nodeToAdd is an ancestor of targetParent, adding it would create a cycle
        var current = targetParent;
        while (current != null)
        {
            if (current.Id == nodeToAdd.Id)
            {
                throw new CyclicReferenceException();
            }
            current = current.Parent;
        }
    }

    // Helper methods for action pipeline execution (middleware with next() calls)
    private async Task ExecuteActionPipeline<TContext, TDelegate>(TContext context, IRealTreeNode startNode,
        Func<IRealTreeNode, IEnumerable<TDelegate>> getDelegates, Func<Task> coreOperation)
        where TContext : OperationContext
        where TDelegate : Delegate
    {
        var actions = CollectFromHierarchy(startNode, getDelegates);

        if (actions.Count == 0)
        {
            await coreOperation();
            return;
        }

        // Build middleware pipeline
        var pipeline = actions.Aggregate(coreOperation, (next, action) =>
        {
            return () => (Task)action.DynamicInvoke(context, next)!;
        });

        await pipeline();
    }

    // Helper methods for event execution (fire-and-forget notifications with exception logging)
    private async Task ExecuteEvents<TContext, TDelegate>(TContext context, IRealTreeNode startNode,
        Func<IRealTreeNode, IEnumerable<TDelegate>> getDelegates)
        where TContext : OperationContext
        where TDelegate : Delegate
    {
        var events = CollectFromHierarchy(startNode, getDelegates);

        if (events.Count == 0)
        {
            return;
        }

        // Execute all events in parallel (fire-and-forget with error logging)
        var tasks = events.Select(async eventDelegate =>
        {
            try
            {
                await (Task)eventDelegate.DynamicInvoke(context)!;
            }
            catch (Exception ex)
            {
                // Log event execution errors but don't propagate them
                _logger?.LogError(ex, "Event handler failed during {OperationType} operation", context.GetType().Name);
            }
        });

        try
        {
            await Task.WhenAll(tasks);
        }
        catch
        {
            // Events are fire-and-forget, exceptions are already logged above
        }
    }

    private List<T> CollectFromHierarchy<T>(IRealTreeNode startNode, Func<IRealTreeNode, IEnumerable<T>> getDelegates)
    {
        var delegates = new List<T>();
        var current = startNode;

        // Collect delegates from node hierarchy (bottom-up)
        while (current != null)
        {
            delegates.AddRange(getDelegates(current));
            current = current.Parent;
        }

        return delegates;
    }

    // Action delegate getters
    private IEnumerable<AddContainerDelegate> GetAddContainerActions(IRealTreeNode node)
    {
        return node switch
        {
            RealTreeContainer container => container.AddContainerActions,
            RealTreeItem item => item.AddContainerActions,
            _ => Enumerable.Empty<AddContainerDelegate>()
        };
    }

    private IEnumerable<AddItemDelegate> GetAddItemActions(IRealTreeNode node)
    {
        return node is RealTreeContainer container ? container.AddItemActions : Enumerable.Empty<AddItemDelegate>();
    }

    private IEnumerable<RemoveContainerDelegate> GetRemoveContainerActions(IRealTreeNode node)
    {
        return node switch
        {
            RealTreeContainer container => container.RemoveContainerActions,
            RealTreeItem item => item.RemoveContainerActions,
            _ => Enumerable.Empty<RemoveContainerDelegate>()
        };
    }

    private IEnumerable<RemoveItemDelegate> GetRemoveItemActions(IRealTreeNode node)
    {
        return node is RealTreeContainer container ? container.RemoveItemActions : Enumerable.Empty<RemoveItemDelegate>();
    }

    private IEnumerable<UpdateNodeDelegate> GetUpdateActions(IRealTreeNode node)
    {
        return node switch
        {
            RealTreeContainer container => container.UpdateActions,
            RealTreeItem item => item.UpdateActions,
            _ => Enumerable.Empty<UpdateNodeDelegate>()
        };
    }

    private IEnumerable<MoveNodeDelegate> GetMoveActions(IRealTreeNode node)
    {
        return node switch
        {
            RealTreeContainer container => container.MoveActions,
            RealTreeItem item => item.MoveActions,
            _ => Enumerable.Empty<MoveNodeDelegate>()
        };
    }

    // Event delegate getters
    private IEnumerable<ContainerAddedEventDelegate> GetContainerAddedEvents(IRealTreeNode node)
    {
        return node switch
        {
            RealTreeContainer container => container.ContainerAddedEvents,
            RealTreeItem item => item.ContainerAddedEvents,
            _ => Enumerable.Empty<ContainerAddedEventDelegate>()
        };
    }

    private IEnumerable<ItemAddedEventDelegate> GetItemAddedEvents(IRealTreeNode node)
    {
        return node is RealTreeContainer container ? container.ItemAddedEvents : Enumerable.Empty<ItemAddedEventDelegate>();
    }

    private IEnumerable<ContainerRemovedEventDelegate> GetContainerRemovedEvents(IRealTreeNode node)
    {
        return node switch
        {
            RealTreeContainer container => container.ContainerRemovedEvents,
            RealTreeItem item => item.ContainerRemovedEvents,
            _ => Enumerable.Empty<ContainerRemovedEventDelegate>()
        };
    }

    private IEnumerable<ItemRemovedEventDelegate> GetItemRemovedEvents(IRealTreeNode node)
    {
        return node is RealTreeContainer container ? container.ItemRemovedEvents : Enumerable.Empty<ItemRemovedEventDelegate>();
    }

    private IEnumerable<NodeUpdatedEventDelegate> GetNodeUpdatedEvents(IRealTreeNode node)
    {
        return node switch
        {
            RealTreeContainer container => container.NodeUpdatedEvents,
            RealTreeItem item => item.NodeUpdatedEvents,
            _ => Enumerable.Empty<NodeUpdatedEventDelegate>()
        };
    }

    private IEnumerable<NodeMovedEventDelegate> GetNodeMovedEvents(IRealTreeNode node)
    {
        return node switch
        {
            RealTreeContainer container => container.NodeMovedEvents,
            RealTreeItem item => item.NodeMovedEvents,
            _ => Enumerable.Empty<NodeMovedEventDelegate>()
        };
    }

    // Bulk action delegate getters
    private IEnumerable<BulkAddContainerDelegate> GetBulkAddContainerActions(IRealTreeNode node)
    {
        return node switch
        {
            RealTreeContainer container => container.BulkAddContainerActions,
            RealTreeItem item => item.BulkAddContainerActions,
            _ => Enumerable.Empty<BulkAddContainerDelegate>()
        };
    }

    private IEnumerable<BulkAddItemDelegate> GetBulkAddItemActions(IRealTreeNode node)
    {
        return node is RealTreeContainer container ? container.BulkAddItemActions : Enumerable.Empty<BulkAddItemDelegate>();
    }

    private IEnumerable<BulkRemoveContainerDelegate> GetBulkRemoveActions(IRealTreeNode node)
    {
        return node switch
        {
            RealTreeContainer container => container.BulkRemoveActions,
            RealTreeItem item => item.BulkRemoveActions,
            _ => Enumerable.Empty<BulkRemoveContainerDelegate>()
        };
    }

    // Bulk event delegate getters
    private IEnumerable<BulkContainersAddedEventDelegate> GetBulkContainersAddedEvents(IRealTreeNode node)
    {
        return node switch
        {
            RealTreeContainer container => container.BulkContainersAddedEvents,
            RealTreeItem item => item.BulkContainersAddedEvents,
            _ => Enumerable.Empty<BulkContainersAddedEventDelegate>()
        };
    }

    private IEnumerable<BulkItemsAddedEventDelegate> GetBulkItemsAddedEvents(IRealTreeNode node)
    {
        return node is RealTreeContainer container ? container.BulkItemsAddedEvents : Enumerable.Empty<BulkItemsAddedEventDelegate>();
    }

    private IEnumerable<BulkNodesRemovedEventDelegate> GetBulkNodesRemovedEvents(IRealTreeNode node)
    {
        return node switch
        {
            RealTreeContainer container => container.BulkNodesRemovedEvents,
            RealTreeItem item => item.BulkNodesRemovedEvents,
            _ => Enumerable.Empty<BulkNodesRemovedEventDelegate>()
        };
    }

    // Core operation implementations
    private Task PerformAddContainer(IRealTreeNode parent, IRealTreeContainer container)
    {
        switch (parent)
        {
            case IRealTreeContainer parentContainer:
                parentContainer.AddContainer(container);
                break;
            case IRealTreeItem parentItem:
                parentItem.AddContainer(container);
                break;
            default:
                throw new ArgumentException("Parent must be a container or item", nameof(parent));
        }
        return Task.CompletedTask;
    }

    private Task PerformAddItem(IRealTreeContainer parent, IRealTreeItem item)
    {
        parent.AddItem(item);
        return Task.CompletedTask;
    }

    private void PerformRemove(IRealTreeNode node)
    {
        if (node.Parent == null)
        {
            throw new InvalidOperationException("Cannot remove root node");
        }

        switch (node.Parent)
        {
            case IRealTreeContainer parentContainer when node is IRealTreeContainer container:
                parentContainer.RemoveContainer(container);
                break;
            case IRealTreeContainer parentContainer when node is IRealTreeItem item:
                parentContainer.RemoveItem(item);
                break;
            case IRealTreeItem parentItem when node is IRealTreeContainer container:
                parentItem.RemoveContainer(container);
                break;
            default:
                throw new InvalidOperationException("Invalid parent-child relationship");
        }
    }

    private void PerformUpdate(IRealTreeNode node, string? newName, Dictionary<string, object>? newMetadata)
    {
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
    }

    private void PerformMove(IRealTreeNode node, IRealTreeNode newParent)
    {
        // Remove from current parent
        if (node.Parent != null)
        {
            PerformRemove(node);
        }

        // Add to new parent
        switch (newParent)
        {
            case IRealTreeContainer parentContainer when node is IRealTreeContainer container:
                parentContainer.AddContainer(container);
                break;
            case IRealTreeContainer parentContainer when node is IRealTreeItem item:
                parentContainer.AddItem(item);
                break;
            case IRealTreeItem parentItem when node is IRealTreeContainer container:
                parentItem.AddContainer(container);
                break;
            default:
                throw new ArgumentException("Invalid move operation: incompatible parent-child types", nameof(newParent));
        }
    }

    // Core bulk operation implementations
    private Task PerformBulkAddContainers(IRealTreeNode parent, IReadOnlyList<IRealTreeContainer> containers)
    {
        foreach (var container in containers)
        {
            switch (parent)
            {
                case IRealTreeContainer parentContainer:
                    parentContainer.AddContainer(container);
                    break;
                case IRealTreeItem parentItem:
                    parentItem.AddContainer(container);
                    break;
                default:
                    throw new ArgumentException("Parent must be a container or item", nameof(parent));
            }
        }
        return Task.CompletedTask;
    }

    private Task PerformBulkAddItems(IRealTreeContainer parent, IReadOnlyList<IRealTreeItem> items)
    {
        foreach (var item in items)
        {
            parent.AddItem(item);
        }
        return Task.CompletedTask;
    }

    private void PerformBulkRemove(IReadOnlyList<IRealTreeNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.Parent == null)
            {
                continue; // Skip nodes without parents (like root)
            }

            switch (node.Parent)
            {
                case IRealTreeContainer parentContainer when node is IRealTreeContainer container:
                    parentContainer.RemoveContainer(container);
                    break;
                case IRealTreeContainer parentContainer when node is IRealTreeItem item:
                    parentContainer.RemoveItem(item);
                    break;
                case IRealTreeItem parentItem when node is IRealTreeContainer container:
                    parentItem.RemoveContainer(container);
                    break;
            }
        }
    }

    // Query operations implementation
    public async Task<IReadOnlyList<IRealTreeNode>> ListContainerAsync(
        IRealTreeContainer container,
        bool includeContainers = true,
        bool includeItems = true,
        bool recursive = false,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
    {
        var context = new ListContainerContext(container, includeContainers, includeItems, recursive, container.Tree, cancellationToken);
        IReadOnlyList<IRealTreeNode> result;

        if (triggerActions)
        {
            result = await ExecuteListActionPipeline(context, container, node => GetListContainerActions(node), () => Task.FromResult(PerformListContainer(context)));
        }
        else
        {
            result = PerformListContainer(context);
        }

        if (triggerEvents)
        {
            await ExecuteListEvents(context, result, container, node => GetContainerListedEvents(node));
        }

        return result;
    }

    // Helper method for list action pipeline execution
    private async Task<IReadOnlyList<IRealTreeNode>> ExecuteListActionPipeline<TDelegate>(
        ListContainerContext context,
        IRealTreeNode startNode,
        Func<IRealTreeNode, IEnumerable<TDelegate>> getDelegates,
        Func<Task<IReadOnlyList<IRealTreeNode>>> coreOperation)
        where TDelegate : Delegate
    {
        var actions = CollectFromHierarchy(startNode, getDelegates);

        if (actions.Count == 0)
        {
            return await coreOperation();
        }

        // Build middleware pipeline with result capture
        IReadOnlyList<IRealTreeNode>? result = null;
        var pipeline = actions.Aggregate(
            async () => { result = await coreOperation(); },
            (next, action) =>
            {
                return () => (Task)action.DynamicInvoke(context, next)!;
            });

        await pipeline();
        return result ?? new List<IRealTreeNode>().AsReadOnly();
    }

    // Helper method for list event execution
    private async Task ExecuteListEvents<TDelegate>(
        ListContainerContext context,
        IReadOnlyList<IRealTreeNode> result,
        IRealTreeNode startNode,
        Func<IRealTreeNode, IEnumerable<TDelegate>> getDelegates)
        where TDelegate : Delegate
    {
        var events = CollectFromHierarchy(startNode, getDelegates);

        if (events.Count == 0)
        {
            return;
        }

        // Execute all events in parallel (fire-and-forget with error logging)
        var tasks = events.Select(async eventDelegate =>
        {
            try
            {
                await (Task)eventDelegate.DynamicInvoke(context, result, context.CancellationToken)!;
            }
            catch (Exception ex)
            {
                // Log event execution errors but don't propagate them
                _logger?.LogError(ex, "Event handler failed during {OperationType} operation", context.GetType().Name);
            }
        });

        try
        {
            await Task.WhenAll(tasks);
        }
        catch
        {
            // Events are fire-and-forget, exceptions are already logged above
        }
    }

    // Action delegate getter for list container
    private IEnumerable<ListContainerDelegate> GetListContainerActions(IRealTreeNode node)
    {
        return node switch
        {
            RealTreeContainer container => container.ListContainerActions,
            RealTreeItem item => item.ListContainerActions,
            _ => Enumerable.Empty<ListContainerDelegate>()
        };
    }

    // Event delegate getter for container listed
    private IEnumerable<ContainerListedEventDelegate> GetContainerListedEvents(IRealTreeNode node)
    {
        return node switch
        {
            RealTreeContainer container => container.ContainerListedEvents,
            RealTreeItem item => item.ContainerListedEvents,
            _ => Enumerable.Empty<ContainerListedEventDelegate>()
        };
    }

    // Core list operation implementation
    private IReadOnlyList<IRealTreeNode> PerformListContainer(ListContainerContext context)
    {
        var results = new List<IRealTreeNode>();

        if (context.Recursive)
        {
            // Recursive listing
            CollectNodesRecursive(context.Container, results, context.IncludeContainers, context.IncludeItems);
        }
        else
        {
            // Direct children only
            if (context.IncludeContainers)
            {
                results.AddRange(context.Container.Containers);
            }
            if (context.IncludeItems)
            {
                results.AddRange(context.Container.Items);
            }
        }

        return results.AsReadOnly();
    }

    // Helper for recursive collection
    private void CollectNodesRecursive(IRealTreeContainer container, List<IRealTreeNode> results, bool includeContainers, bool includeItems)
    {
        if (includeContainers)
        {
            foreach (var childContainer in container.Containers)
            {
                results.Add(childContainer);
                CollectNodesRecursive(childContainer, results, includeContainers, includeItems);
            }
        }

        if (includeItems)
        {
            foreach (var item in container.Items)
            {
                results.Add(item);
                // Items can have containers too, so recurse
                foreach (var childContainer in item.Containers)
                {
                    results.Add(childContainer);
                    CollectNodesRecursive(childContainer, results, includeContainers, includeItems);
                }
            }
        }
    }

    // Show item operation implementation
    public async Task ShowItemAsync(
        IRealTreeItem item,
        bool triggerActions = true,
        bool triggerEvents = true,
        CancellationToken cancellationToken = default)
    {
        var context = new ShowItemContext(item, item.Tree, cancellationToken);

        if (triggerActions)
        {
            await ExecuteActionPipeline(context, item, node => GetShowItemActions(node), () => Task.CompletedTask);
        }

        if (triggerEvents)
        {
            await ExecuteShowItemEvents(context, item, node => GetItemShownEvents(node));
        }
    }

    // Helper method for show item event execution
    private async Task ExecuteShowItemEvents<TDelegate>(
        ShowItemContext context,
        IRealTreeNode startNode,
        Func<IRealTreeNode, IEnumerable<TDelegate>> getDelegates)
        where TDelegate : Delegate
    {
        var events = CollectFromHierarchy(startNode, getDelegates);

        if (events.Count == 0)
        {
            return;
        }

        // Execute all events in parallel (fire-and-forget with error logging)
        var tasks = events.Select(async eventDelegate =>
        {
            try
            {
                await (Task)eventDelegate.DynamicInvoke(context, context.CancellationToken)!;
            }
            catch (Exception ex)
            {
                // Log event execution errors but don't propagate them
                _logger?.LogError(ex, "Event handler failed during {OperationType} operation", context.GetType().Name);
            }
        });

        try
        {
            await Task.WhenAll(tasks);
        }
        catch
        {
            // Events are fire-and-forget, exceptions are already logged above
        }
    }

    // Action delegate getter for show item
    private IEnumerable<ShowItemDelegate> GetShowItemActions(IRealTreeNode node)
    {
        return node is RealTreeItem item ? item.ShowItemActions : Enumerable.Empty<ShowItemDelegate>();
    }

    // Event delegate getter for item shown
    private IEnumerable<ItemShownEventDelegate> GetItemShownEvents(IRealTreeNode node)
    {
        return node is RealTreeItem item ? item.ItemShownEvents : Enumerable.Empty<ItemShownEventDelegate>();
    }
}