namespace Mesch.RealTree;

/// <summary>
/// Concrete item implementation - structural only.
/// All middleware and events managed by RealTreeOperations via type registry.
/// </summary>
public class RealTreeItem : RealTreeNodeBase, IRealTreeItem
{
    private readonly List<IRealTreeContainer> _containers = new();

    public IReadOnlyList<IRealTreeContainer> Containers => _containers.AsReadOnly();

    public override IRealTree Tree => Parent?.Tree ??
        throw new InvalidOperationException("Item is not attached to a tree");

    public RealTreeItem() : base(null, null) { }
    public RealTreeItem(Guid? id, string? name) : base(id, name) { }

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
}
