using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Models.Nominators;

public class NominatorForm
{
    public static NominatorForm From(Nominator nominator)
    {
        return new NominatorForm
        {
            Name = nominator.Name,
            Attributes = nominator.Attributes!
                .Select(it => new NominatorAttributeForm
                {
                    AttributeName = it.AttributeName,
                    EffectiveAtDate = DateOnly.FromDateTime(it.EffectiveAt),
                    EffectiveAtTime = TimeOnly.FromDateTime(it.EffectiveAt),
                    EffectiveEndAtDate =
                        it.EffectiveEndAt != null ? DateOnly.FromDateTime(it.EffectiveEndAt.Value) : null,
                    EffectiveEndAtTime = it.EffectiveEndAt != null
                        ? TimeOnly.FromDateTime(it.EffectiveEndAt.Value)
                        : null
                })
                .ToList()
        };
    }

    public required string Name { get; init; }
    public required IList<NominatorAttributeForm> Attributes { get; init; }
}