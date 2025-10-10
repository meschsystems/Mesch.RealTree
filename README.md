# RealTree

## Introduction

RealTree is a .NET library for building hierarchical tree structures with type-based middleware pipelines. The framework provides a centralized operations service through which all tree modifications flow, enabling consistent policy enforcement, external system synchronization, and composable business logic across different node types.

The library addresses a specific architectural need: managing tree-shaped resources where different parts of the hierarchy require different behavioral contracts. Rather than embedding business logic within node classes, RealTree inverts control through a delegation model. Nodes remain simple data structures while behavior is centralized in an operations service that dispatches middleware based on node types.

## Core Concepts

### Node Types

The framework defines two fundamental node types that form the structural basis of any tree:

**Containers** are nodes that can hold both other containers and items. These nodes form the backbone of hierarchical structures, enabling nested organization similar to folders within a filesystem. A container represents any collection or grouping that needs both subdivision and content.

**Items** are terminal nodes that can hold containers but not other items. This asymmetric design prevents deep nesting of leaf-level entities while still allowing internal structure where needed. An item might represent a document with sections, a product with variations, or any entity that has internal organization but does not collect peers.

Both node types extend from `IRealTreeNode`, providing common properties such as identifiers, names, metadata dictionaries, parent references, and path calculation. The root of every tree is itself a specialized container (`IRealTree`) that serves as the entry point for navigation and operations.

### The Delegation Model

Traditional object-oriented tree implementations embed behavior within node classes through methods like `AddChild()` or `Remove()`. RealTree takes a different approach by routing all operations through a central service (`IRealTreeOperations`). This delegation model provides several architectural advantages.

Operations are dispatched based on type rather than instance. When adding a container to the root, middleware registered for the root type executes. When adding to a regular container, different middleware executes. This type-based dispatch creates behavioral contracts: all instances of `SecureFolder` follow the same security rules regardless of where they appear in the tree.

The separation between structure and behavior means new capabilities can be added without modifying node classes. Cross-cutting concerns such as validation, authorization, and auditing are composed through middleware layers rather than inheritance hierarchies. External system synchronization, caching strategies, and business rules remain cleanly separated from the tree's structural implementation.

### Control Semantics

The framework distinguishes between two categories of operations based on control semantics:

**Boundary operations** cross node boundaries and are controlled by the parent node. When adding or removing children, the parent determines what can enter or exit its collection. This mirrors real-world authorization patterns where containers control their contents. The parent's type determines which middleware executes for these operations.

**Self operations** affect a node's own state or query its contents. Update, list, and show operations are controlled by the node itself, with middleware registered against the node's type rather than its parent's type. This ensures each node type maintains sovereignty over its own behavior.

This ownership model creates intuitive middleware registration: parents guard their boundaries while nodes manage themselves.

## Installation and Setup

The library is available as a NuGet package:

```bash
dotnet add package Mesch.RealTree
```

Basic setup requires creating a factory and operations service:

```csharp
var factory = new RealTreeFactory();
var operations = new RealTreeOperations(factory);
var tree = factory.CreateTree(name: "MyTree");
```

The factory determines default node types when using non-generic operation methods. Custom factories can override these defaults to return specialized node implementations.

## Operations

### Operation Flow

All tree modifications and queries flow through the operations service. Each operation follows a consistent pattern: validation, middleware execution, structural modification, and completion. Operations accept common parameters including cancellation tokens, metadata dictionaries, and flags controlling middleware execution.

The `triggerActions` parameter controls whether registered middleware executes. Setting this to false bypasses the middleware pipeline entirely, useful for bulk imports or system-level operations that should skip business logic validation.

### Adding Nodes

Add operations create new nodes and attach them to parent containers. The framework provides both generic methods for strongly-typed nodes and non-generic methods that use factory defaults:

```csharp
// Strongly-typed addition
var container = await operations.AddContainerAsync<CustomContainer>(
    parent: tree,
    id: null,  // Generates new GUID
    name: "Documents",
    metadata: new Dictionary<string, object> { ["owner"] = "admin" }
);

// Factory default
var item = await operations.AddItemAsync(
    parent: container,
    id: null,
    name: "Report.pdf"
);
```

Metadata specified during creation is set on the node before middleware executes, allowing middleware to read and augment initial values. Parent nodes control admission through type-specific middleware registered via `RegisterAddContainerAction<T>` or `RegisterAddItemAction<T>`.

### Removing Nodes

Remove operations detach nodes from their parents and remove them from the tree. Removing a node also removes all its descendants, maintaining structural integrity:

```csharp
await operations.RemoveAsync(node);

// Bulk removal
await operations.RemoveAllContainersAsync(parent);
await operations.RemoveAllItemsAsync(container);
```

The root node cannot be removed. Parent nodes control removal through middleware registered via `RegisterRemoveContainerAction<T>` or `RegisterRemoveItemAction<T>`.

### Updating Nodes

Update operations modify node properties including names and metadata:

```csharp
await operations.UpdateAsync(
    node: container,
    newName: "Archived Documents",
    newMetadata: new Dictionary<string, object> { ["status"] = "archived" }
);
```

Metadata replacement is complete rather than merged. If new metadata is provided, it entirely replaces existing metadata rather than updating individual keys. Nodes control their own updates through middleware registered via `RegisterUpdateAction<T>`.

### Moving Nodes

Move operations relocate nodes to new parents. The implementation removes the node from its current parent and adds it to the new parent, triggering both sets of middleware:

```csharp
await operations.MoveAsync(node, newParent);
```

The framework prevents cyclic references by validating that nodes cannot be moved to themselves or their descendants. Items can only be moved between containers, not to other items.

### Bulk Operations

Bulk operations process multiple nodes efficiently. Rather than triggering middleware for each individual node, bulk operations execute specialized bulk middleware once for the entire batch:

```csharp
var items = new[]
{
    (id: (Guid?)null, name: "Doc1.pdf", metadata: (IDictionary<string, object>?)null),
    (id: (Guid?)null, name: "Doc2.pdf", metadata: (IDictionary<string, object>?)null)
};

await operations.BulkAddItemsAsync<RealTreeItem>(container, items);
```

Empty collections skip all processing including middleware execution. Bulk middleware is registered separately from individual operation middleware through methods like `RegisterBulkAddContainerAction<T>`.

### Copy Operations

Copy operations create duplicates of existing nodes with new identities:

```csharp
var copy = await operations.CopyContainerAsync(
    source: originalContainer,
    destination: targetParent,
    newId: null,      // Generates new GUID
    newName: null,    // Uses source name
    deep: true        // Include all descendants
);
```

Deep copying recursively duplicates the entire subtree, while shallow copying creates only the top-level node. Metadata is copied from source nodes, and middleware executes for each created node.

### Query Operations

Query operations retrieve information from nodes and optionally synchronize with external systems. The list operation works on both containers and items since both can have children:

```csharp
var contents = await operations.ListAsync(
    node: container,
    includeContainers: true,
    includeItems: true,
    recursive: false
);
```

Recursive listing traverses the entire subtree regardless of inclusion flags, though only matching nodes are returned. The show operation exists specifically for items and typically refreshes metadata from external sources:

```csharp
await operations.ShowItemAsync(item);
// Results communicated through item.Metadata
```

Query operations use dedicated metadata dictionaries in their contexts, allowing middleware to communicate hints and results back to the caller.

## Middleware System

### Pipeline Architecture

Middleware forms a pipeline of handlers that execute in registration order. Each handler receives a context object containing operation details and a next delegate to invoke the subsequent handler:

```csharp
operations.RegisterAddContainerAction<RealTreeRoot>(async (context, next) =>
{
    // Pre-processing
    Console.WriteLine($"Before adding {context.Container.Name}");
    
    // Continue pipeline
    await next();
    
    // Post-processing
    Console.WriteLine($"After adding {context.Container.Name}");
});
```

The pipeline pattern enables several execution strategies. Calling `await next()` continues to the next handler or the actual operation. Throwing an exception cancels the operation and propagates the error to the caller. Not calling `next()` short-circuits remaining middleware but still allows the operation to complete.

### Type-Based Registration

Middleware registration follows the control semantics of each operation. Boundary operations register against parent types:

```csharp
// Controls what can be added TO a SecureFolder
operations.RegisterAddContainerAction<SecureFolder>(async (ctx, next) =>
{
    if (!HasPermission(ctx.Container, "write"))
        throw new UnauthorizedAccessException();
    await next();
});
```

Self operations register against node types:

```csharp
// Controls how DocumentItem updates itself
operations.RegisterUpdateAction<DocumentItem>(async (ctx, next) =>
{
    ctx.Node.Metadata["lastModified"] = DateTime.UtcNow;
    await next();
});
```

This type-based dispatch ensures consistent behavior across all instances of a type regardless of their position in the tree.

### Context Objects

Each operation provides a strongly-typed context object containing relevant information. All contexts inherit from `OperationContext` and include the root tree, cancellation token, and operation timestamp. Transactional operations also include a transaction object for registering commit and rollback callbacks.

Add and remove contexts include the node being added or removed and its parent. Update contexts include both old and new values for names and metadata. Query contexts include specialized metadata dictionaries for communication between middleware and callers.

### Deregistration

Middleware can be removed by maintaining references to handler delegates:

```csharp
AddContainerDelegate handler = async (ctx, next) => { await next(); };
operations.RegisterAddContainerAction<RealTreeRoot>(handler);

// Later
bool removed = operations.DeregisterAddContainerAction<RealTreeRoot>(handler);
```

## Transaction Support

The framework provides transaction semantics for operations that require atomicity. Move operations are internally implemented as transactional sequences, ensuring either complete success or complete rollback:

```csharp
operations.RegisterRemoveContainerAction<SecureFolder>(async (ctx, next) =>
{
    if (ctx.Transaction != null)
    {
        ctx.Transaction.OnCommit(() =>
        {
            // Execute only if entire move succeeds
            NotifyExternalSystem(ctx.Container.Id, "moved");
        });
        
        ctx.Transaction.OnRollback(() =>
        {
            // Cleanup if move fails
            CancelPendingOperations(ctx.Container.Id);
        });
    }
    
    await next();
});
```

Transactions ensure structural integrity during complex operations. If any part of a move operation fails, the entire operation rolls back with no changes applied to the tree.

## Metadata System

Metadata serves as the primary communication channel between middleware layers, the operations service, and host applications. Each node maintains a metadata dictionary that can store arbitrary key-value pairs.

### Initial Metadata

Metadata specified during node creation is set before middleware executes, allowing handlers to read and augment initial values:

```csharp
var metadata = new Dictionary<string, object>
{
    ["department"] = "Engineering",
    ["budget"] = 100000
};

var project = await operations.AddContainerAsync(tree, null, "Project Alpha", metadata);

// Middleware can read and modify
operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
{
    var budget = (int)ctx.Container.Metadata["budget"];
    ctx.Container.Metadata["approved"] = budget <= 150000;
    await next();
});
```

### Query Metadata

Query operations provide dedicated metadata dictionaries in their contexts for bidirectional communication:

```csharp
operations.RegisterListAction<ProjectContainer>(async (ctx, next) =>
{
    // Communicate to caller
    ctx.ListingMetadata["totalBudget"] = CalculateTotalBudget(ctx.Container);
    ctx.ListingMetadata["isOverBudget"] = IsOverBudget(ctx.Container);
    
    await next();
});

// Caller reads metadata after operation
var results = await operations.ListAsync(projectContainer);
// Access ctx.ListingMetadata values through other means
```

The show operation similarly uses `ShowMetadata` for communicating display hints and synchronization results.

## Event System

The RealTree framework includes a comprehensive event system for observing tree modifications. Events are raised **after operations complete successfully**, providing notification of structural changes without the ability to prevent or modify those changes.

### Events vs Middleware

The event system operates independently from middleware and serves a different purpose:

| Feature | Middleware | Events |
|---------|-----------|--------|
| **When it runs** | Before the operation | After the operation completes |
| **Can prevent operation** | Yes (throw exception) | No (operation already completed) |
| **Can modify operation** | Yes (change context) | No (read-only notification) |
| **Use case** | Validation, authorization, augmentation | Notification, logging, side effects |
| **Error handling** | Exceptions propagate to caller | Exceptions isolated and logged |

**Key principle:** Middleware controls what happens; events observe what happened.

### Event Types and Ownership Model

Events follow the same type-based dispatch as middleware, using the **ownership model**:

- **Boundary events** (add/remove) are fired by the **parent** → register with parent type
- **Self events** (update/list/show) are fired by the **node** → register with node type

This ensures each component controls its own domain: parents own boundary events, nodes own self events.

#### Available Events

**Boundary Events (Parent-Fired):**
- `ContainerAdded` - Fired when a container is added to a parent
- `ItemAdded` - Fired when an item is added to a parent
- `ContainerRemoved` - Fired when a container is removed from a parent
- `ItemRemoved` - Fired when an item is removed from a parent
- `BulkContainersAdded` - Fired when multiple containers are added to a parent
- `BulkItemsAdded` - Fired when multiple items are added to a parent
- `BulkNodesRemoved` - Fired when multiple nodes are removed from a parent

**Self Events (Node-Fired):**
- `NodeUpdated` - Fired when a node is updated
- `NodeShown` - Fired when a node is shown (retrieved)
- `ContainerListed` - Fired when a container's contents are listed
- `NodeCopied` - Fired when a node is copied

### Registering Event Handlers

Event handlers are registered on the `IRealTreeOperations` service using type parameters:

```csharp
// Register for container additions to RealTreeRoot
operations.RegisterContainerAddedEvent<RealTreeRoot>(async (ctx) =>
{
    // Only fires when containers are added TO a RealTreeRoot
    Console.WriteLine($"Container '{ctx.Container.Name}' added to root");
    await auditLog.LogAsync($"Root container added: {ctx.Container.Name}");
});

// Register for item additions to a custom container type
operations.RegisterItemAddedEvent<DocumentsContainer>(async (ctx) =>
{
    // Only fires when items are added TO a DocumentsContainer
    await indexService.IndexDocumentAsync(ctx.Item);
});

// Register for node updates on a custom item type
operations.RegisterNodeUpdatedEvent<DocumentItem>(async (ctx) =>
{
    // Only fires when a DocumentItem is updated
    await searchService.ReindexAsync(ctx.Node.Id);
});
```

### Controlling Event Emission

The `triggerEvents` parameter on operations controls whether events are fired:

```csharp
// Events will fire (default behavior)
var container = await operations.AddContainerAsync(tree, null, "Docs");

// Events will NOT fire
var container = await operations.AddContainerAsync(
    tree,
    null,
    "Docs",
    triggerEvents: false
);
```

**When to disable events:**
- Bulk operations where you want to fire a single bulk event instead
- Internal operations that shouldn't trigger notifications
- Performance-critical code paths
- During data migrations or imports

### Event Context Objects

Each event provides a context object with relevant operation details:

```csharp
operations.RegisterContainerAddedEvent<RealTreeRoot>(async (ctx) =>
{
    // Access operation details
    IRealTreeContainer container = ctx.Container;  // The container that was added
    IRealTreeNode parent = ctx.Parent;             // The parent it was added to
    IRealTree tree = ctx.Tree;                     // The tree instance
    CancellationToken token = ctx.CancellationToken;  // Cancellation token

    await ProcessAsync(container, token);
});

operations.RegisterNodeUpdatedEvent<DocumentItem>(async (ctx) =>
{
    // Update events include old and new metadata
    IRealTreeNode node = ctx.Node;
    IReadOnlyDictionary<string, object>? oldMetadata = ctx.OldMetadata;
    IReadOnlyDictionary<string, object>? newMetadata = ctx.NewMetadata;

    // Detect specific changes
    if (oldMetadata?.ContainsKey("status") == true &&
        newMetadata?.ContainsKey("status") == true)
    {
        var oldStatus = oldMetadata["status"];
        var newStatus = newMetadata["status"];
        if (!Equals(oldStatus, newStatus))
        {
            await NotifyStatusChangeAsync(node, oldStatus, newStatus);
        }
    }
});
```

### Error Handling in Events

Event handlers execute in a **fire-and-forget** pattern with error isolation:

```csharp
operations.RegisterContainerAddedEvent<RealTreeRoot>(async (ctx) =>
{
    // If this throws, the exception is logged but doesn't propagate
    throw new Exception("Event handler failed!");
    // The tree operation has already completed successfully
    // Other event handlers will still execute
});
```

**Key behaviors:**
- Exceptions in event handlers are caught and logged
- Exceptions do NOT propagate to the caller
- One failing handler doesn't prevent other handlers from executing
- Events execute in parallel for performance

**Best practices:**
- Add try-catch blocks for specific error handling
- Log errors within your handler
- Don't throw exceptions to "cancel" operations (they're already complete)

### Deregistering Event Handlers

```csharp
// Store reference to handler
ContainerAddedEventDelegate handler = async (ctx) =>
{
    await ProcessAsync(ctx.Container);
};

// Register
operations.RegisterContainerAddedEvent<RealTreeRoot>(handler);

// Deregister later
bool removed = operations.DeregisterContainerAddedEvent<RealTreeRoot>(handler);
```

### Bulk Operations and Events

Bulk operations disable individual events by default and fire a single bulk event:

```csharp
var containers = new[] { container1, container2, container3 };

// Individual ContainerAdded events will NOT fire
// Instead, BulkContainersAdded event fires once
await operations.AddAllContainersAsync(tree, containers);

// Register for bulk event
operations.RegisterBulkContainersAddedEvent<RealTreeRoot>(async (ctx) =>
{
    IReadOnlyList<IRealTreeContainer> containers = ctx.Containers;
    IRealTreeNode parent = ctx.Parent;

    Console.WriteLine($"Added {containers.Count} containers to {parent.Name}");
});
```

This prevents duplicate notifications when you're already handling the bulk event.

### Common Event Patterns

**Audit Logging:**
```csharp
operations.RegisterContainerAddedEvent<RealTreeRoot>(async (ctx) =>
{
    await auditLog.LogAsync(new AuditEntry
    {
        Action = "ContainerAdded",
        ContainerName = ctx.Container.Name,
        ParentName = ctx.Parent.Name,
        Timestamp = DateTime.UtcNow,
        User = currentUser.Id
    });
});
```

**Search Indexing:**
```csharp
operations.RegisterItemAddedEvent<DocumentsContainer>(async (ctx) =>
{
    await searchIndex.AddAsync(new SearchDocument
    {
        Id = ctx.Item.Id,
        Name = ctx.Item.Name,
        Path = ctx.Item.GetPath(),
        Metadata = ctx.Item.Metadata
    });
});

operations.RegisterItemRemovedEvent<DocumentsContainer>(async (ctx) =>
{
    await searchIndex.RemoveAsync(ctx.Item.Id);
});
```

**Cache Invalidation:**
```csharp
operations.RegisterNodeUpdatedEvent<DocumentItem>(async (ctx) =>
{
    await cache.InvalidateAsync($"document:{ctx.Node.Id}");

    // Invalidate parent cache too
    if (ctx.Node.Parent != null)
    {
        await cache.InvalidateAsync($"container:{ctx.Node.Parent.Id}");
    }
});
```

**Real-time Notifications:**
```csharp
operations.RegisterContainerAddedEvent<ProjectContainer>(async (ctx) =>
{
    // Notify all connected clients via SignalR
    await hubContext.Clients
        .Group($"project-{ctx.Parent.Id}")
        .SendAsync("ContainerAdded", new
        {
            ContainerId = ctx.Container.Id,
            ContainerName = ctx.Container.Name,
            ParentId = ctx.Parent.Id
        });
});
```

**Cascade Operations:**
```csharp
operations.RegisterContainerAddedEvent<TenantsContainer>(async (ctx) =>
{
    // Auto-create default folders when a new tenant is added
    await operations.AddContainerAsync(
        ctx.Container,
        null,
        "Documents",
        triggerEvents: false  // Don't trigger events for internal operations
    );
    await operations.AddContainerAsync(
        ctx.Container,
        null,
        "Settings",
        triggerEvents: false
    );
});
```

### Type-Based Event Dispatch

Like middleware, events use type-based dispatch to target specific node types:

```csharp
// This fires when adding TO a RealTreeRoot
operations.RegisterContainerAddedEvent<RealTreeRoot>(async (ctx) =>
{
    Console.WriteLine("Added to root");
});

// This fires when adding TO a custom container type
operations.RegisterContainerAddedEvent<ProjectContainer>(async (ctx) =>
{
    Console.WriteLine("Added to project");
});

// Add container to root - fires first handler only
await operations.AddContainerAsync(tree, null, "Container1");

// Add container to project - fires second handler only
var project = await operations.AddContainerAsync(tree, null, "Project1");
await operations.AddContainerAsync(project, null, "Container2");
```

**Important:** The type parameter represents the **type that fires the event**, not the type being added/removed:
- For boundary events (add/remove): use the **parent's type**
- For self events (update/list/show): use the **node's type**

### Event System Summary

The event system provides a robust way to observe tree operations without interfering with them:

**Use events for:**
- Audit logging
- Search indexing
- Cache invalidation
- Real-time notifications
- Analytics and metrics
- Cascade operations (adding related data)
- External system synchronization

**Don't use events for:**
- Validation (use middleware instead)
- Authorization (use middleware instead)
- Preventing operations (use middleware instead)
- Modifying operation behavior (use middleware instead)

Events execute after operations complete, ensuring they represent actual tree state changes rather than attempted operations.

## Navigation

The framework provides several mechanisms for navigating tree structures. The root tree supports path-based and ID-based lookups:

```csharp
var node = tree.FindByPath("/Documents/2024/Report.pdf");
var found = tree.FindById(nodeId);
```

Path lookups use forward slashes as delimiters and traverse from the root. Performance is proportional to path depth. ID lookups scan the entire tree and have linear complexity.

All nodes expose navigation properties including parent references, child collections, calculated paths, and depth values. Containers provide filtered access to their containers and items separately, while items only expose their container children.

## Custom Node Types

The framework supports custom node implementations that extend the base types:

```csharp
public class ProjectContainer : RealTreeContainer
{
    public decimal Budget { get; set; }
    public DateTime Deadline { get; set; }
    
    public ProjectContainer() : base(null, null) { }
    
    public ProjectContainer(Guid? id, string name, decimal budget, DateTime deadline)
        : base(id, name)
    {
        Budget = budget;
        Deadline = deadline;
    }
}
```

Generic operation methods work with custom types when they have parameterless constructors. Properties are set after construction but before middleware execution. Custom factories can override default types returned by non-generic operations.

## Common Patterns

### Hierarchical Authorization

Different node types often require different authorization rules. The type-based middleware system naturally supports hierarchical permission models:

```csharp
operations.RegisterAddContainerAction<RealTreeRoot>(async (ctx, next) =>
{
    // Only administrators can create root-level containers
    if (!CurrentUser.HasRole("Admin"))
        throw new UnauthorizedAccessException("Root access requires admin role");
    await next();
});

operations.RegisterAddContainerAction<DepartmentFolder>(async (ctx, next) =>
{
    // Department managers control their folders
    var managerId = ctx.Parent.Metadata.GetValueOrDefault("managerId");
    if (CurrentUser.Id != managerId)
        throw new UnauthorizedAccessException("Only department manager can modify");
    await next();
});
```

### External System Synchronization

Nodes often represent resources in external systems. Query operations provide natural synchronization points:

```csharp
operations.RegisterListAction<CloudStorageContainer>(async (ctx, next) =>
{
    // Refresh from cloud storage
    var cloudItems = await cloudStorage.ListAsync(ctx.Container.Metadata["bucketId"]);
    
    // Update metadata
    ctx.ListingMetadata["syncTime"] = DateTime.UtcNow;
    ctx.ListingMetadata["itemCount"] = cloudItems.Count;
    
    // Reconcile tree with external state
    await ReconcileItems(ctx.Container, cloudItems);
    
    await next();
});
```

### Validation Layers

Multiple middleware handlers can form validation layers that execute in sequence:

```csharp
// Layer 1: Name validation
operations.RegisterAddItemAction<DocumentContainer>(async (ctx, next) =>
{
    if (!IsValidFileName(ctx.Item.Name))
        throw new ArgumentException("Invalid file name");
    await next();
});

// Layer 2: Size validation
operations.RegisterAddItemAction<DocumentContainer>(async (ctx, next) =>
{
    var size = (long)ctx.Item.Metadata.GetValueOrDefault("fileSize", 0L);
    if (size > MaxFileSize)
        throw new InvalidOperationException("File too large");
    await next();
});

// Layer 3: Virus scanning
operations.RegisterAddItemAction<DocumentContainer>(async (ctx, next) =>
{
    var scanResult = await virusScanner.ScanAsync(ctx.Item.Metadata["filePath"]);
    if (!scanResult.IsClean)
        throw new InvalidOperationException("File failed virus scan");
    await next();
});
```

## Performance Considerations

The framework is designed for flexibility and correctness rather than maximum performance. Several architectural decisions impact performance characteristics.

Middleware pipelines add overhead proportional to the number of registered handlers. Each handler invocation involves async state machine machinery. For high-frequency operations, middleware overhead may become significant.

Path-based lookups have O(depth) complexity, making them efficient for balanced trees but potentially slow for deeply nested structures. ID-based lookups scan the entire tree with O(n) complexity and should be avoided in performance-critical paths.

The metadata system uses standard dictionary implementations without optimization for large datasets. Recursive operations traverse entire subtrees, which can be expensive for large hierarchies.

For optimal performance, applications should use bulk operations when adding multiple nodes, minimize middleware layers in hot paths, maintain reasonably balanced tree structures, and implement caching strategies where appropriate.

## Error Handling

The framework validates inputs and maintains structural integrity through several exception types. `ArgumentException` indicates invalid parameters such as null or empty names. `InvalidOperationException` signals illegal operations like removing the root or adding items to items. `CyclicReferenceException` prevents operations that would create cycles in the tree structure.

Middleware can throw custom exceptions to enforce business rules. These exceptions propagate through the pipeline and back to the operation caller. The framework does not catch or handle middleware exceptions, allowing applications to implement their own error handling strategies.

## Thread Safety

The framework is not thread-safe. Operations on the same tree from multiple threads require external synchronization. Node properties including metadata dictionaries are not protected against concurrent modification.

Applications requiring concurrent access should implement appropriate locking strategies at the tree level rather than the node level to prevent deadlocks and ensure consistency.

## Dependency Injection

The framework integrates with standard dependency injection containers:

```csharp
services.AddSingleton<IRealTreeFactory, RealTreeFactory>();
services.AddScoped<IRealTreeOperations, RealTreeOperations>();

// With custom factory
services.AddSingleton<IRealTreeFactory, CustomTreeFactory>();

// With logging
services.AddScoped<IRealTreeOperations>(provider =>
{
    var factory = provider.GetRequiredService<IRealTreeFactory>();
    var logger = provider.GetService<ILogger<RealTreeOperations>>();
    return new RealTreeOperations(factory, logger);
});
```

Singleton registration is appropriate for factories since they maintain no state. Operations services can be scoped or transient depending on middleware registration patterns.

## License

MIT License - see LICENSE file for details