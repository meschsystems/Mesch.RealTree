namespace Mesch.RealTree;

/// <summary>
/// Middleware-style delegate for handling tree operations with pipeline support.
/// Allows interception, modification, or cancellation of operations before they execute.
/// </summary>
/// <typeparam name="T">The context type for the operation.</typeparam>
/// <param name="context">The operation context containing details about the operation.</param>
/// <param name="next">The next delegate in the pipeline. Call this to continue execution.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task TreeOperationDelegate<T>(T context, Func<Task> next) where T : OperationContext;

/// <summary>
/// Middleware delegate for container addition operations.
/// </summary>
/// <param name="context">Context containing the container being added and its parent.</param>
/// <param name="next">The next handler in the pipeline.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task AddContainerDelegate(AddContainerContext context, Func<Task> next);

/// <summary>
/// Middleware delegate for item addition operations.
/// </summary>
/// <param name="context">Context containing the item being added and its parent.</param>
/// <param name="next">The next handler in the pipeline.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task AddItemDelegate(AddItemContext context, Func<Task> next);

/// <summary>
/// Middleware delegate for container removal operations.
/// </summary>
/// <param name="context">Context containing the container being removed and its parent.</param>
/// <param name="next">The next handler in the pipeline.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task RemoveContainerDelegate(RemoveContext context, Func<Task> next);

/// <summary>
/// Middleware delegate for item removal operations.
/// </summary>
/// <param name="context">Context containing the item being removed and its parent.</param>
/// <param name="next">The next handler in the pipeline.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task RemoveItemDelegate(RemoveContext context, Func<Task> next);

/// <summary>
/// Middleware delegate for bulk container removal operations.
/// </summary>
/// <param name="context">Context containing the containers being removed.</param>
/// <param name="next">The next handler in the pipeline.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task BulkRemoveContainerDelegate(BulkRemoveContext context, Func<Task> next);

/// <summary>
/// Middleware delegate for bulk item removal operations.
/// </summary>
/// <param name="context">Context containing the items being removed.</param>
/// <param name="next">The next handler in the pipeline.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task BulkRemoveItemDelegate(BulkRemoveContext context, Func<Task> next);

/// <summary>
/// Middleware delegate for bulk container addition operations.
/// </summary>
/// <param name="context">Context containing the containers being added and their parent.</param>
/// <param name="next">The next handler in the pipeline.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task BulkAddContainerDelegate(BulkAddContainerContext context, Func<Task> next);

/// <summary>
/// Middleware delegate for bulk item addition operations.
/// </summary>
/// <param name="context">Context containing the items being added and their parent.</param>
/// <param name="next">The next handler in the pipeline.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task BulkAddItemDelegate(BulkAddItemContext context, Func<Task> next);

/// <summary>
/// Middleware delegate for node update operations.
/// </summary>
/// <param name="context">Context containing the node being updated and the old/new values.</param>
/// <param name="next">The next handler in the pipeline.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task UpdateNodeDelegate(UpdateContext context, Func<Task> next);

/// <summary>
/// Middleware delegate for node move operations.
/// </summary>
/// <param name="context">Context containing the node being moved and its old/new parents.</param>
/// <param name="next">The next handler in the pipeline.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task MoveNodeDelegate(MoveContext context, Func<Task> next);

/// <summary>
/// Event delegate that executes after a container has been successfully added.
/// Events are fire-and-forget and execute in parallel.
/// </summary>
/// <param name="context">Context containing details about the container addition.</param>
/// <returns>A task representing the asynchronous event handling.</returns>
public delegate Task ContainerAddedEventDelegate(AddContainerContext context);

/// <summary>
/// Event delegate that executes after a container has been successfully removed.
/// </summary>
/// <param name="context">Context containing details about the container removal.</param>
/// <returns>A task representing the asynchronous event handling.</returns>
public delegate Task ContainerRemovedEventDelegate(RemoveContext context);

/// <summary>
/// Event delegate that executes after an item has been successfully added.
/// </summary>
/// <param name="context">Context containing details about the item addition.</param>
/// <returns>A task representing the asynchronous event handling.</returns>
public delegate Task ItemAddedEventDelegate(AddItemContext context);

/// <summary>
/// Event delegate that executes after an item has been successfully removed.
/// </summary>
/// <param name="context">Context containing details about the item removal.</param>
/// <returns>A task representing the asynchronous event handling.</returns>
public delegate Task ItemRemovedEventDelegate(RemoveContext context);

/// <summary>
/// Event delegate that executes after a node has been successfully updated.
/// </summary>
/// <param name="context">Context containing details about the node update.</param>
/// <returns>A task representing the asynchronous event handling.</returns>
public delegate Task NodeUpdatedEventDelegate(UpdateContext context);

/// <summary>
/// Event delegate that executes after a node has been successfully moved.
/// </summary>
/// <param name="context">Context containing details about the node move.</param>
/// <returns>A task representing the asynchronous event handling.</returns>
public delegate Task NodeMovedEventDelegate(MoveContext context);

/// <summary>
/// Event delegate that executes after containers have been successfully added in bulk.
/// </summary>
/// <param name="context">Context containing details about the bulk container addition.</param>
/// <returns>A task representing the asynchronous event handling.</returns>
public delegate Task BulkContainersAddedEventDelegate(BulkAddContainerContext context);

/// <summary>
/// Event delegate that executes after items have been successfully added in bulk.
/// </summary>
/// <param name="context">Context containing details about the bulk item addition.</param>
/// <returns>A task representing the asynchronous event handling.</returns>
public delegate Task BulkItemsAddedEventDelegate(BulkAddItemContext context);

/// <summary>
/// Event delegate that executes after nodes have been successfully removed in bulk.
/// </summary>
/// <param name="context">Context containing details about the bulk removal.</param>
/// <returns>A task representing the asynchronous event handling.</returns>
public delegate Task BulkNodesRemovedEventDelegate(BulkRemoveContext context);