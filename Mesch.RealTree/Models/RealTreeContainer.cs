namespace Mesch.RealTree;

/// <summary>
/// Concrete container implementation - structural only.
/// All middleware and events managed by RealTreeOperations via type registry.
/// </summary>
public class RealTreeContainer : RealTreeNodeBase, IRealTreeContainer
{
    private readonly List<IRealTreeContainer> _containers = new();
    private readonly List<IRealTreeItem> _items = new();

    public virtual IReadOnlyList<IRealTreeContainer> Containers => _containers.AsReadOnly();
    public virtual IReadOnlyList<IRealTreeItem> Items => _items.AsReadOnly();
    public virtual IReadOnlyList<IRealTreeNode> Children =>
        _containers.Cast<IRealTreeNode>().Concat(_items.Cast<IRealTreeNode>()).ToList().AsReadOnly();

    public override IRealTree Tree => Parent?.Tree ?? (this as IRealTree) ??
        throw new InvalidOperationException("Container is not attached to a tree");

    public RealTreeContainer() : base(null, null) { }
    public RealTreeContainer(Guid? id, string? name) : base(id, name) { }

    public void AddContainer(IRealTreeContainer container)
    {
        // Validate cyclic references
        if (container == this)
        {
            throw new CyclicReferenceException("Cannot add container to itself");
        }

        if (IsAncestorOf(container))
        {
            throw new CyclicReferenceException("Cannot add an ancestor as a child - this would create a cycle");
        }

        _containers.Add(container);
        ((RealTreeNodeBase)container).SetParent(this);
    }

    private bool IsAncestorOf(IRealTreeNode potentialDescendant)
    {
        var current = this.Parent;
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

    public void RemoveContainer(IRealTreeContainer container)
    {
        _containers.Remove(container);
        ((RealTreeNodeBase)container).SetParent(null);
    }

    public void AddItem(IRealTreeItem item)
    {
        _items.Add(item);
        ((RealTreeNodeBase)item).SetParent(this);
    }

    public void RemoveItem(IRealTreeItem item)
    {
        _items.Remove(item);
        ((RealTreeNodeBase)item).SetParent(null);
    }
}
