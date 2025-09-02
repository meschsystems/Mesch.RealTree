# Mesch.RealTree
A thread-safe tree hierarchy management abstraction with plugin architecture and eventing.

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
└── IRealTree          // Root node (extends container)
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
    Console.WriteLine($"Adding {context.Container.Name} to {context.Parent.Name}");
    
    // Validate
    if (context.Container.Name.StartsWith("temp"))
        throw new InvalidOperationException("Temp folders not allowed");
    
    // Continue pipeline
    await next();
    
    // Post-operation logic
    Console.WriteLine("Container added successfully");
});
```

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
tree.RegisterNodeUpdatedEvent(async context =>
{
    await auditLogger.LogAsync(new AuditEntry
    {
        UserId = GetCurrentUser().Id,
        Action = "Update",
        NodeId = context.Node.Id,
        OldName = context.OldName,
        NewName = context.NewName,
        Timestamp = context.OperationTime
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
container.UnregisterAddContainerAction(validator);
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