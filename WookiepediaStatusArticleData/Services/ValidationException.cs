using System.Collections.Immutable;

namespace WookiepediaStatusArticleData.Services;

public class ValidationException(IList<ValidationIssue> issues) : Exception
{
    public IReadOnlyList<ValidationIssue> Issues { get; } = issues.ToImmutableArray();
    
    public ValidationException(ValidationIssue issue) : this([issue])
    {
    }
}
