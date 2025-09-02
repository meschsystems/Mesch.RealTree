namespace Mesch.RealTree;

/// <summary>
/// Abstract base class providing common node functionality.
/// </summary>
public abstract class RealTreeNodeBase : IRealTreeNode
{
    private readonly object _lockObject = new object();
    private string _name;
    private IRealTreeNode? _parent;

    public Guid Id { get; }

    public string Name
    {
        get { return _name; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Name cannot be null or whitespace", nameof(value));
            }
            _name = value;
        }
    }

    public Dictionary<string, object> Metadata { get; }

    public IRealTreeNode? Parent
    {
        get { return _parent; }
        internal set { _parent = value; }
    }

    public abstract IRealTree Tree { get; }

    public string Path
    {
        get
        {
            if (Parent == null)
            {
                return $"/{Name}";
            }
            return $"{Parent.Path}/{Name}";
        }
    }

    public int Depth
    {
        get
        {
            int depth = 0;
            var current = Parent;
            while (current != null)
            {
                depth++;
                current = current.Parent;
            }
            return depth;
        }
    }

    protected RealTreeNodeBase(Guid? id = null, string? name = null)
    {
        Id = id ?? Guid.NewGuid();
        _name = name ?? Id.ToString();
        Metadata = new Dictionary<string, object>();
    }

    internal void SetParent(IRealTreeNode? parent)
    {
        lock (_lockObject)
        {
            _parent = parent;
        }
    }
}