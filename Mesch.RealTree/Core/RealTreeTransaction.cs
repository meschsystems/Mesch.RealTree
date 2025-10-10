namespace Mesch.RealTree;

/// <summary>
/// Concrete implementation of a tree transaction with commit/rollback support.
/// </summary>
internal class RealTreeTransaction : IRealTreeTransaction
{
    private readonly List<Action<RealTreeTransaction>> _structuralOperations = new();
    private readonly List<Action> _commitCallbacks = new();
    private readonly List<Func<Task>> _asyncCommitCallbacks = new();
    private readonly List<Action> _rollbackCallbacks = new();
    private readonly List<Func<Task>> _asyncRollbackCallbacks = new();
    private bool _disposed;

    public bool IsActive { get; private set; } = true;
    public bool IsCommitted { get; private set; }
    public bool IsRolledBack { get; private set; }

    /// <summary>
    /// Records a structural operation to be executed during commit.
    /// </summary>
    internal void RecordStructuralOperation(Action<RealTreeTransaction> operation)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Cannot record operations on an inactive transaction");
        }

        _structuralOperations.Add(operation);
    }

    public void OnCommit(Action onCommit)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Cannot register commit callbacks on an inactive transaction");
        }

        _commitCallbacks.Add(onCommit);
    }

    public void OnCommit(Func<Task> onCommit)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Cannot register commit callbacks on an inactive transaction");
        }

        _asyncCommitCallbacks.Add(onCommit);
    }

    public void OnRollback(Action onRollback)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Cannot register rollback callbacks on an inactive transaction");
        }

        _rollbackCallbacks.Add(onRollback);
    }

    public void OnRollback(Func<Task> onRollback)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Cannot register rollback callbacks on an inactive transaction");
        }

        _asyncRollbackCallbacks.Add(onRollback);
    }

    public async Task CommitAsync()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Transaction is not active");
        }

        try
        {
            // Execute structural operations
            foreach (var operation in _structuralOperations)
            {
                operation(this);
            }

            // Execute commit callbacks
            foreach (var callback in _commitCallbacks)
            {
                callback();
            }

            foreach (var callback in _asyncCommitCallbacks)
            {
                await callback();
            }

            IsCommitted = true;
            IsActive = false;
        }
        catch
        {
            // If commit fails, rollback
            await RollbackAsync();
            throw;
        }
    }

    public async Task RollbackAsync()
    {
        if (!IsActive && !IsCommitted)
        {
            return; // Already rolled back
        }

        IsActive = false;
        IsRolledBack = true;

        // Execute rollback callbacks in reverse order
        for (int i = _rollbackCallbacks.Count - 1; i >= 0; i--)
        {
            try
            {
                _rollbackCallbacks[i]();
            }
            catch
            {
                // Continue rolling back even if individual callbacks fail
            }
        }

        for (int i = _asyncRollbackCallbacks.Count - 1; i >= 0; i--)
        {
            try
            {
                await _asyncRollbackCallbacks[i]();
            }
            catch
            {
                // Continue rolling back even if individual callbacks fail
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // If transaction is still active when disposed, rollback
        if (IsActive)
        {
            Task.Run(async () => await RollbackAsync()).Wait();
        }
    }
}
