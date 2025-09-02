namespace Mesch.RealTree;

/// <summary>
/// The exception that is thrown when an invalid containment operation is attempted.
/// </summary>
public class InvalidContainmentException : TreeValidationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidContainmentException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the invalid containment error.</param>
    public InvalidContainmentException(string message) : base(message) { }
}
