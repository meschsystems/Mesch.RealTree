namespace Mesch.RealTree;

/// <summary>
/// The exception that is thrown when attempting to create a node with a name that already exists among its siblings.
/// </summary>
public class DuplicateNameException : TreeValidationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateNameException"/> class with the specified duplicate name.
    /// </summary>
    /// <param name="name">The name that caused the duplication error.</param>
    public DuplicateNameException(string name) : base($"A sibling with name '{name}' already exists") { }
}
