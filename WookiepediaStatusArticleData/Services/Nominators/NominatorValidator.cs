using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominators;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Services.Nominators;

public class NominatorValidator(WookiepediaDbContext db)
{
    public async Task<List<ValidationIssue>> ValidateNameAsync(
        int? id,
        string name,
        CancellationToken cancellationToken
    )
    {
        var nominatorWithSameName = await db.Set<Nominator>()
            .AnyAsync(it => it.Name == name && it.Id != id, cancellationToken);

        return nominatorWithSameName
            ?
            [
                new ValidationIssue(
                    nameof(NominatorForm.Name),
                    $"{name} is already used by another nominator."
                ),
            ]
            : [];
    }

    public List<ValidationIssue> ValidateAttributes(NominatorForm form)
    {
        return FindOverlaps(form.Attributes)
            .SelectMany<DateOverlap, ValidationIssue>(overlap =>
            {
                HashSet<NominatorAttributeType> attributeNameSet 
                    = [overlap.First.AttributeName, overlap.Second.AttributeName];

                // you can't have the same attribute applied multiple times for the same timeframe
                if (attributeNameSet.Count == 1)
                {
                    return [
                        new ValidationIssue(
                            $"Attributes[{overlap.FirstIndex}].EffectiveAt",
                            $"Cannot have multiple overlapping {overlap.First.AttributeName} attributes. Remove one timeframe and combine it with the other."
                        ),
                        new ValidationIssue(
                            $"Attributes[{overlap.SecondIndex}].EffectiveAt",
                            $"Cannot have multiple overlapping {overlap.First.AttributeName} attributes. Remove one timeframe and combine it with the other."
                        )
                    ];
                }

                // you also can't be banned and something else at the same time
                if (attributeNameSet.Contains(NominatorAttributeType.Banned))
                {
                    return [
                        new ValidationIssue(
                            $"Attributes[{overlap.FirstIndex}].EffectiveAt",
                            $"Attributes {overlap.First.AttributeName} and {overlap.Second.AttributeName} cannot overlap. Correct the timeframe or remove one."
                        ),
                        new ValidationIssue(
                            $"Attributes[{overlap.SecondIndex}].EffectiveAt",
                            $"Attributes {overlap.Second.AttributeName} and {overlap.First.AttributeName} cannot overlap. Correct the timeframe or remove one."
                        )
                    ];
                }

                return [];
            })
            .ToList();
    }

    private IEnumerable<DateOverlap> FindOverlaps(IList<NominatorAttributeViewModel> attributes)
    {
        for (var i = 0; i < attributes.Count; i++)
        {
            for (var j = i + 1; j < attributes.Count; j++)
            {
                if (DateRangesOverlap(attributes[i], attributes[j]))
                {
                    yield return new DateOverlap
                    {
                        FirstIndex = i,
                        First = attributes[i],
                        SecondIndex = j,
                        Second = attributes[j]
                    };
                }
            }
        }
    }
    
    private static bool DateRangesOverlap(NominatorAttributeViewModel obj1, NominatorAttributeViewModel obj2)
    {
        var end1 = obj1.EffectiveUntil ?? DateOnly.MaxValue;
        var end2 = obj2.EffectiveUntil ?? DateOnly.MaxValue;

        return obj1.EffectiveAt <= end2 && obj2.EffectiveAt <= end1;
    }

    private class DateOverlap
    {
        public required int FirstIndex { get; init;  }
        public required NominatorAttributeViewModel First { get; init; }
        public required int SecondIndex { get; init; }
        public required NominatorAttributeViewModel Second { get; init; }
    }
}

