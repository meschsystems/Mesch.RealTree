namespace Mesch.RealTree;
/// <summary>
/// Tree implementation that serves as the root container.
/// </summary>
public class RealTreeRoot : RealTreeContainer, IRealTree
{
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    public override IRealTree Tree => this;

    public RealTreeRoot(Guid? id = null, string? name = null) : base(id, name ?? "Root") { }

    public IRealTreeNode? FindByPath(string path)
    {
        if (string.IsNullOrEmpty(path) || path == "/")
        {
            return this;
        }

        var segments = path.TrimStart('/').Split('/');
        IRealTreeNode? current = this;

        foreach (var segment in segments)
        {
            if (current == null)
            {
                return null;
            }

            current = current switch
            {
                IRealTreeContainer container => container.Children.FirstOrDefault(c => c.Name == segment),
                IRealTreeItem item => item.Containers.FirstOrDefault(c => c.Name == segment),
                _ => null
            };
        }

        return current;
    }

    public IRealTreeNode? FindById(Guid id)
    {
        return FindByIdRecursive(this, id);
    }

    private IRealTreeNode? FindByIdRecursive(IRealTreeNode node, Guid id)
    {
        if (node.Id == id)
        {
            return node;
        }

        if (node is IRealTreeContainer container)
        {
            foreach (var child in container.Children)
            {
                var result = FindByIdRecursive(child, id);
                if (result != null)
                {
                    return result;
                }
            }
        }
        else if (node is IRealTreeItem item)
        {
            foreach (var child in item.Containers)
            {
                var result = FindByIdRecursive(child, id);
                if (result != null)
                {
                    return result;
                }
            }
        }

        return null;
    }

    internal ReaderWriterLockSlim Lock => _lock;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _lock?.Dispose();
        }
    }
}