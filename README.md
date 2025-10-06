# Mesch.RealTree
A tree hierarchy management abstraction with plugin architecture and eventing.

## Overview

RealTree provides a flexible tree implementation where:
- **Containers** can hold both other containers and items
- **Items** can hold containers but not other items
- **Operations** support middleware-style actions and fire-and-forget events
- **Validation** prevents cyclic references automatically

## Core Concepts

### Node Types

```csharp
IRealTreeNode       // Base interface for all nodes
├── IRealTreeContainer  // Can contain containers and items
├── IRealTreeItem      // Can contain containers only
└── IRealTree          // Root node with global operations (extends IRealTreeNode)
```

### Operations Service

All tree modifications go through `IRealTreeOperations` which provides:
- Middleware-style action pipelines (can intercept/modify operations)
- Event notifications (fire-and-forget, executed after operations)
- Bulk operations for performance
- Copy operations with recursive traversal

## Quick Start

```csharp
// Create tree
var factory = new RealTreeFactory();
var tree = factory.CreateTree(name: "MyTree");
var operations = new RealTreeOperations(factory);

// Add nodes
var container = await operations.AddContainerAsync(tree, null, "Documents");
var item = await operations.AddItemAsync(container, null, "Report.pdf");

// Navigate
var found = tree.FindByPath("/Documents/Report.pdf");
Console.WriteLine(found?.Name); // "Report.pdf"
```

## Middleware Actions

Actions execute before the operation and can intercept, modify, or cancel it:

```csharp
container.RegisterAddContainerAction(async (context, next) =>
{
    // Pre-operation logic
    // Parent is IRealTreeNode, use ParentAsContainer helper for container-specific access
    var parentName = context.ParentAsContainer?.Name ?? context.Parent.Name;
    Console.WriteLine($"Adding {context.Container.Name} to {parentName}");

    // Validate
    if (context.Container.Name.StartsWith("temp"))
        throw new InvalidOperationException("Temp folders not allowed");

    // Continue pipeline
    await next();

    // Post-operation logic
    Console.WriteLine("Container added successfully");
});
```

### Available Middleware Actions Reference

Middleware actions form a pipeline that executes before operations. Each action must call `await next()` to continue the pipeline, or throw an exception to cancel the operation.

#### Modification Actions

| Action | Delegate Type | Signature |
|--------|--------------|-----------|
| `RegisterAddContainerAction` | `AddContainerDelegate` | `Task (AddContainerContext context, Func<Task> next)` |
| `RegisterAddItemAction` | `AddItemDelegate` | `Task (AddItemContext context, Func<Task> next)` |
| `RegisterRemoveContainerAction` | `RemoveContainerDelegate` | `Task (RemoveContainerContext context, Func<Task> next)` |
| `RegisterRemoveItemAction` | `RemoveItemDelegate` | `Task (RemoveItemContext context, Func<Task> next)` |
| `RegisterUpdateAction` | `UpdateNodeDelegate` | `Task (UpdateContext context, Func<Task> next)` |
| `RegisterMoveAction` | `MoveNodeDelegate` | `Task (MoveContext context, Func<Task> next)` |

**Context Properties:** Same as event contexts (see Event Handling section below)

#### Bulk Operation Actions

| Action | Delegate Type | Signature |
|--------|--------------|-----------|
| `RegisterBulkAddContainerAction` | `BulkAddContainerDelegate` | `Task (BulkAddContainerContext context, Func<Task> next)` |
| `RegisterBulkAddItemAction` | `BulkAddItemDelegate` | `Task (BulkAddItemContext context, Func<Task> next)` |
| `RegisterBulkRemoveAction` | `BulkRemoveContainerDelegate` | `Task (BulkRemoveContext context, Func<Task> next)` |

#### Query Actions

| Action | Delegate Type | Signature |
|--------|--------------|-----------|
| `RegisterListContainerAction` | `ListContainerDelegate` | `Task (ListContainerContext context, Func<Task> next)` |

**Important Notes:**
- Actions can run **before** or **after** the operation by placing code before/after the `await next()` call
- Throwing an exception **cancels** the operation and propagates to the caller
- Actions execute in order from the node up through its parent hierarchy
- Forgetting to call `await next()` will **prevent** the operation from completing

## Event Handling

Events fire after operations complete and run in parallel:

```csharp
container.RegisterContainerAddedEvent(async context =>
{
    // Log to database
    await LogActivity($"Container {context.Container.Name} added");
});

container.RegisterContainerAddedEvent(async context =>
{
    // Send notification (runs in parallel with logging)
    await NotifyUsers($"New folder: {context.Container.Name}");
});
```

### Available Events Reference

All events are fire-and-forget notifications that execute after operations complete. They run in parallel and exceptions are logged but don't propagate.

#### Modification Events

| Event | Delegate Type | Signature |
|-------|--------------|-----------|
| `RegisterContainerAddedEvent` | `ContainerAddedEventDelegate` | `Task (AddContainerContext context)` |
| `RegisterItemAddedEvent` | `ItemAddedEventDelegate` | `Task (AddItemContext context)` |
| `RegisterContainerRemovedEvent` | `ContainerRemovedEventDelegate` | `Task (RemoveContainerContext context)` |
| `RegisterItemRemovedEvent` | `ItemRemovedEventDelegate` | `Task (RemoveItemContext context)` |
| `RegisterNodeUpdatedEvent` | `NodeUpdatedEventDelegate` | `Task (UpdateContext context)` |
| `RegisterNodeMovedEvent` | `NodeMovedEventDelegate` | `Task (MoveContext context)` |

**Context Properties:**
- `AddContainerContext`: `.Container` (IRealTreeContainer), `.Parent` (IRealTreeNode), `.Tree`, `.CancellationToken`
- `AddItemContext`: `.Item` (IRealTreeItem), `.Parent` (IRealTreeContainer), `.Tree`, `.CancellationToken`
- `RemoveContainerContext`: `.Container` (IRealTreeContainer), `.Parent` (IRealTreeContainer), `.Tree`, `.CancellationToken`
- `RemoveItemContext`: `.Item` (IRealTreeItem), `.Parent` (IRealTreeContainer), `.Tree`, `.CancellationToken`
- `UpdateContext`: `.Node`, `.OldName`, `.NewName`, `.OldMetadata`, `.NewMetadata`, `.Tree`, `.CancellationToken`
- `MoveContext`: `.Node`, `.OldParent`, `.NewParent`, `.Tree`, `.CancellationToken`

#### Bulk Operation Events

| Event | Delegate Type | Signature |
|-------|--------------|-----------|
| `RegisterBulkContainersAddedEvent` | `BulkContainersAddedEventDelegate` | `Task (BulkAddContainerContext context)` |
| `RegisterBulkItemsAddedEvent` | `BulkItemsAddedEventDelegate` | `Task (BulkAddItemContext context)` |
| `RegisterBulkNodesRemovedEvent` | `BulkNodesRemovedEventDelegate` | `Task (BulkRemoveContext context)` |

**Context Properties:**
- `BulkAddContainerContext`: `.Containers` (IReadOnlyList), `.Parent` (IRealTreeNode), `.Tree`, `.CancellationToken`
- `BulkAddItemContext`: `.Items` (IReadOnlyList), `.Parent` (IRealTreeContainer), `.Tree`, `.CancellationToken`
- `BulkRemoveContext`: `.Nodes` (IReadOnlyList), `.Parent` (IRealTreeNode), `.Tree`, `.CancellationToken`

#### Query Events

| Event | Delegate Type | Signature |
|-------|--------------|-----------|
| `RegisterContainerListedEvent` | `ContainerListedEventDelegate` | `Task (ListContainerContext context, IReadOnlyList<IRealTreeNode> result, CancellationToken cancellationToken)` |

**Context Properties:**
- `ListContainerContext`: `.Container`, `.Tree`, `.CancellationToken`

**Note:** Events can be registered at multiple levels:
- On specific nodes (container/item) - fires for operations on that subtree
- On the tree root - fires for all operations globally

**Example:**
```csharp
// Local event - only fires for this container's operations
myContainer.RegisterItemAddedEvent(async ctx =>
    await LogLocal($"Item added: {ctx.Item.Name}"));

// Global event - fires for all item additions in the entire tree
tree.RegisterItemAddedEvent(async ctx =>
    await LogGlobal($"Item added anywhere: {ctx.Item.Name}"));
```

## Bulk Operations

Efficient operations for multiple nodes:

```csharp
var containers = new List<IRealTreeContainer>
{
    factory.CreateContainer(name: "Folder1"),
    factory.CreateContainer(name: "Folder2"),
    factory.CreateContainer(name: "Folder3")
};

await operations.BulkAddContainersAsync(tree, containers);
```

## Common Patterns

### Hierarchical Permissions

```csharp
// Register at tree level - applies to all operations
tree.RegisterAddContainerAction(async (context, next) =>
{
    var user = GetCurrentUser();
    var parent = context.Parent;
    
    if (!HasPermission(user, parent, "create"))
        throw new UnauthorizedAccessException();
    
    await next();
});
```

### Audit Logging

```csharp
// Update events
tree.RegisterNodeUpdatedEvent(async context =>
{
    await auditLogger.LogAsync(new AuditEntry
    {
        UserId = GetCurrentUser().Id,
        Action = "Update",
        NodeId = context.Node.Id,
        OldName = context.OldName,
        NewName = context.NewName,
        Timestamp = DateTime.UtcNow
    });
});

// Removal events - strongly typed contexts
tree.RegisterContainerRemovedEvent(async context =>
{
    await auditLogger.LogAsync(new AuditEntry
    {
        UserId = GetCurrentUser().Id,
        Action = "RemoveContainer",
        NodeId = context.Container.Id,
        NodeName = context.Container.Name,
        ParentId = context.Parent.Id,
        Timestamp = DateTime.UtcNow
    });
});

tree.RegisterItemRemovedEvent(async context =>
{
    await auditLogger.LogAsync(new AuditEntry
    {
        UserId = GetCurrentUser().Id,
        Action = "RemoveItem",
        NodeId = context.Item.Id,
        NodeName = context.Item.Name,
        ParentId = context.Parent.Id,
        Timestamp = DateTime.UtcNow
    });
});
```

### Validation with Context

```csharp
container.RegisterAddItemAction(async (context, next) =>
{
    var parent = context.Parent;
    var item = context.Item;
    
    // Business rule: max 10 items per container
    if (parent.Items.Count >= 10)
        throw new InvalidOperationException("Container full");
    
    // Check for duplicates
    if (parent.Items.Any(i => i.Name == item.Name))
        throw new DuplicateNameException(item.Name);
    
    await next();
});
```

## Context Types

### Strongly-Typed Removal Contexts

Removal operations use type-specific contexts for better type safety:

```csharp
// Container removal - access .Container and .Parent (both strongly typed)
tree.RegisterRemoveContainerAction(async (context, next) =>
{
    Console.WriteLine($"Removing container: {context.Container.Name}");
    Console.WriteLine($"From parent: {context.Parent.Name}");

    // context.Container is IRealTreeContainer
    // and context.Parent is IRealTreeContainer

    await next();
});

// Item removal - access .Item and .Parent (both strongly typed)
tree.RegisterRemoveItemAction(async (context, next) =>
{
    Console.WriteLine($"Removing item: {context.Item.Name}");
    Console.WriteLine($"From parent: {context.Parent.Name}");

    // context.Item is IRealTreeItem
    // and context.Parent is IRealTreeContainer

    await next();
});
```

### AddContainerContext Helpers

`AddContainerContext.Parent` is `IRealTreeNode` because both containers and items can hold containers. Use helper properties to avoid manual casting:

```csharp
container.RegisterAddContainerAction(async (context, next) =>
{
    // Access parent as specific type
    if (context.ParentAsContainer != null)
    {
        Console.WriteLine($"Parent has {context.ParentAsContainer.Items.Count} items");
    }
    else if (context.ParentAsItem != null)
    {
        Console.WriteLine($"Parent is an item: {context.ParentAsItem.Name}");
    }

    await next();
});
```

## Advanced Usage

### Custom Node Types

```csharp
public class FileContainer : RealTreeContainer
{
    public string ContentType { get; set; }
    public long MaxSize { get; set; }
    
    public FileContainer(string contentType, long maxSize) 
        : base(name: contentType)
    {
        ContentType = contentType;
        MaxSize = maxSize;
    }
}
```

### Operation Context Access

```csharp
tree.RegisterMoveAction(async (context, next) =>
{
    var node = context.Node;
    var oldPath = node.Path;
    
    await next(); // Perform the move
    
    var newPath = node.Path;
    await UpdateReferences(oldPath, newPath);
});
```

### Delegate Management

```csharp
// Store reference for later removal
AddContainerDelegate validator = async (context, next) => { /* validation */ };
container.RegisterAddContainerAction(validator);

// Remove when no longer needed
container.DeregisterAddContainerAction(validator);
```

### Exception Handling

```csharp
// Actions can throw exceptions to cancel operations
container.RegisterAddContainerAction(async (context, next) =>
{
    if (SomeValidationFails())
        throw new TreeValidationException("Operation not allowed");
    
    await next();
});

// Events log exceptions but don't propagate them
container.RegisterContainerAddedEvent(async context =>
{
    try
    {
        await SomeExternalService.NotifyAsync(context);
    }
    catch (Exception ex)
    {
        // Exception logged automatically, operation continues
    }
});
```

## Thread Safety

- Tree structure modifications are not thread-safe by design
- Use external synchronization if needed
- Events execute in parallel but individual event handlers should be thread-safe
- `ReaderWriterLockSlim` is available on `RealTreeRoot.Lock` for custom locking

## Dependency Injection

```csharp
services.AddSingleton<IRealTreeFactory, RealTreeFactory>();
services.AddScoped<IRealTreeOperations, RealTreeOperations>();

// With logging
services.AddScoped<IRealTreeOperations>(provider =>
{
    var factory = provider.GetService<IRealTreeFactory>();
    var logger = provider.GetService<ILogger<RealTreeOperations>>();
    return new RealTreeOperations(factory, logger);
});
```

## Performance Considerations

- Use bulk operations for multiple related changes
- Event handlers execute in parallel but should be lightweight
- Consider unregistering delegates when no longer needed
- Path lookups are O(depth), ID lookups are O(n)
- Metadata dictionaries are not optimized for large datasets

## Error Types

```csharp
TreeValidationException      // Base validation error
├── DuplicateNameException   // Duplicate sibling names
├── CyclicReferenceException // Would create cycle
└── InvalidContainmentException // Invalid parent-child relationship
```