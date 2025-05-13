using JetBrains.Annotations;

namespace WookiepediaStatusArticleData.Nominations.Nominators;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class Nominator
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required bool IsRedacted { get; set; }
    public IList<NominatorAttribute>? Attributes { get; set; }
}