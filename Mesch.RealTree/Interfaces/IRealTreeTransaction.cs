namespace Mesch.RealTree;

/// <summary>
/// Represents a transaction scope for tree operations, allowing middleware to participate
/// in transactional operations with commit/rollback semantics.
/// </summary>
public interface IRealTreeTransaction : IDisposable
{
    /// <summary>
    /// Gets whether this transaction is currently active (not committed or rolled back).
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Gets whether this transaction has been committed.
    /// </summary>
    bool IsCommitted { get; }

    /// <summary>
    /// Gets whether this transaction has been rolled back.
    /// </summary>
    bool IsRolledBack { get; }

    /// <summary>
    /// Registers a callback to execute when the transaction commits successfully.
    /// Useful for middleware to perform side effects only after structural changes succeed.
    /// </summary>
    /// <param name="onCommit">Action to execute on commit.</param>
    void OnCommit(Action onCommit);

    /// <summary>
    /// Registers an async callback to execute when the transaction commits successfully.
    /// </summary>
    /// <param name="onCommit">Async action to execute on commit.</param>
    void OnCommit(Func<Task> onCommit);

    /// <summary>
    /// Registers a callback to execute when the transaction rolls back.
    /// Useful for middleware to clean up resources or undo external side effects.
    /// </summary>
    /// <param name="onRollback">Action to execute on rollback.</param>
    void OnRollback(Action onRollback);

    /// <summary>
    /// Registers an async callback to execute when the transaction rolls back.
    /// </summary>
    /// <param name="onRollback">Async action to execute on rollback.</param>
    void OnRollback(Func<Task> onRollback);

    /// <summary>
    /// Commits the transaction, executing all registered commit callbacks and
    /// applying all structural changes to the tree.
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// Rolls back the transaction, executing all registered rollback callbacks
    /// and undoing any structural changes.
    /// </summary>
    Task RollbackAsync();
}
