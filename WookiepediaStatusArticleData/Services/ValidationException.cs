using System.Collections.Immutable;

namespace WookiepediaStatusArticleData.Services;

public class ValidationException(IList<ValidationIssue> issues) : Exception
{
    public IReadOnlyList<ValidationIssue> Issues { get; } = [.. issues];

    public ValidationException(ValidationIssue issue) : this([issue])
    {
    }

    public ValidationException(string name, string message) : this([new ValidationIssue(name, message)])
    {
    }
}
