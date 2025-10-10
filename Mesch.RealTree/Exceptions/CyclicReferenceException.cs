namespace Mesch.RealTree;

/// <summary>
/// The exception that is thrown when an operation would create a cyclic reference in the tree structure.
/// </summary>
public class CyclicReferenceException : TreeValidationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CyclicReferenceException"/> class.
    /// </summary>
    public CyclicReferenceException() : base("Operation would create a cyclic reference") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CyclicReferenceException"/> class with a specified error message.
    /// </summary>
    public CyclicReferenceException(string message) : base(message) { }
}
